using System.Text;
using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEditor;
using UnityEngine;
#endif

namespace MuxyGameLink
{
    public class WebsocketTransport : IDisposable
    {
        private ClientWebSocket Websocket;
        private CancellationTokenSource UnboundedCancellationSource = new CancellationTokenSource();
        private static readonly Encoding UTF8Encoding = new UTF8Encoding(false);

        private bool HandleMessagesInMainThread = false;
        private ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

        private Thread WriteThread;
        private Thread ReadThread;
        private bool Done = false;
        private bool IsDisposed = false;

        private Uri TargetUri;

        private CancellationTokenSource TokenSource()
        {
            CancellationTokenSource src = new CancellationTokenSource();
            src.CancelAfter(5000);

            return src;
        }

        private void LogMessage(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            UnityEngine.Debug.Log(message);
#else
            Console.WriteLine(message);
#endif
        }


#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // If a user forgets to stop the websocket transport, the editor locks up.
                // This is bad, prevent this by attaching an event to stop the websocket connection
                // when the editor stops the PIE mode.
                LogMessage("Stopping websocket transport due to editor state change.");
                LogMessage("This may cause errors while playing in editor, but prevents leaking a connection, which is worse.");

                Dispose();
            }
        }
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        private void OnQuitting()
        {
            LogMessage("Stopping due to application quit.");
            Dispose();
        }
#endif

        /// <summary>
        ///  Creates a websocket transport without an associated Gamelink instance or stage.
        /// <param name="HandleMessagesInMainThread">If you are using Unity or an engine that doesn't work nicely with multithreading this should be set to true. If set to true, you must call Update() for GameLink to receive messages</param>
        /// </summary>
        public WebsocketTransport(bool HandleMessagesInMainThread)
        {
            Websocket = new ClientWebSocket();
            this.HandleMessagesInMainThread = HandleMessagesInMainThread;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnQuitting;
#endif

#if UNITY_STANDALONE
            UnityEngine.Application.quitting += OnQuitting;
#endif
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.quitting -= OnQuitting;
#endif

#if UNITY_STANDALONE
            UnityEngine.Application.quitting -= OnQuitting;
#endif

            try
            {
                StopAsync().Wait();
            }
            catch (InvalidOperationException ex)
            {
                LogMessage(ex.ToString());
            }
        }

        ~WebsocketTransport()
        {
            Dispose();
        }

        /// <summary>
        ///  Opens a websocket connection to the given uri, usually computed by calling SDK.ConnectionAddress
        /// </summary>
        /// <param name="uri">URI to connect to. Must be prefixed with the protocol, usually "ws://"</param>
        /// <returns></returns>
        public async Task<bool> Open(string uri)
        {
            TargetUri = new Uri(uri);

            try
            {
                using (CancellationTokenSource src = TokenSource())
                {
                    await Websocket.ConnectAsync(TargetUri, src.Token)
                        .ConfigureAwait(false);
                }
            }
            catch (WebSocketException ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///  Combined call to Open and then Run. Computes the connection URL by calling SDK.ConnectionAddress.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="stage"></param>
        public async Task<bool> OpenAndRunInStage(SDK instance, Stage stage)
        {
            if (await Open("wss://" + instance.ConnectionAddress(stage)).ConfigureAwait(false))
            {
                Run(instance);
                return true;
            }

            return false;
        }

        private async Task OpenAndRunInStage(MuxyGateway.SDK instance, Stage stage)
        {
            switch (stage)
            {
                case Stage.Sandbox:
                    {
                        string url = instance.GetSandboxURL();
                        await Open("wss://" + url)
                            .ConfigureAwait(false);
                        break;
                    }

                case Stage.Production:
                    {
                        string url = instance.GetProductionURL();
                        await Open("wss://" + url)
                            .ConfigureAwait(false);
                        break;
                    }
            }

            Run(instance);
        }

        public Task OpenAndRunInSandbox(MuxyGateway.SDK instance)
        {
            return OpenAndRunInStage(instance, Stage.Sandbox);
        }

        public Task OpenAndRunInProduction(MuxyGateway.SDK instance)
        {
            return OpenAndRunInStage(instance, Stage.Production);
        }

        public void Disconnect()
        {
            StopAsync().Wait();
        }

        /// <summary>
        ///  Invokes SendMessages and ReceiveMessage on different threads until a call to Stop()
        ///  Any callbacks invoked from `instance` will be called on a background thread, not the main thread.
        /// </summary>
        /// <param name="instance">The instance to use for sending and receiving messages</param>
        public void Run(SDK instance)
        {
            if (WriteThread != null || ReadThread != null)
            {
                Disconnect();
            }

            Done = false;
            WriteThread = new Thread(async () =>
            {
                try 
                {
                    while (!Done)
                    {
                        await SendMessages(instance);
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(ex.ToString());
                }

            });
            WriteThread.Start();

            ReadThread = new Thread(async () =>
            {
                try 
                {
                    while (!Done)
                    {
                        await ReceiveMessage(instance);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(ex.ToString());
                }
            });
            ReadThread.Start();
        }

        public void Run(MuxyGateway.SDK instance)
        {
            if (WriteThread != null || ReadThread != null)
            {
                Disconnect();
            }

            Done = false;
            WriteThread = new Thread(async () =>
            {
                try
                {
                    while (!Done)
                    {
                        await SendMessages(instance);
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(ex.ToString());
                }
                finally 
                {
                    LogMessage("Stopping write thread");
                }
            });
            WriteThread.Start();

            ReadThread = new Thread(async () =>
            {
                try 
                {
                    while (!Done)
                    {
                        await ReceiveMessage(instance);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(ex.ToString());
                }
                finally
                {
                    LogMessage("Stopping read thread");
                }
            });
            ReadThread.Start();
        }

        /// <summary>
        ///  Updates the Websocket Transport, it's only required to call this if you set HandleMessagesInMainThread to true when initializing the WebsocketTransport
        /// </summary>
        /// <param name="instance">The instance to use for sending and receiving messages</param>
        public void Update(SDK instance)
        {
            string m;
            while (Messages.TryDequeue(out m))
            {
                instance.ReceiveMessage(m);
            }
        }

        public void Update(MuxyGateway.SDK instance)
        {
            string m;
            while (Messages.TryDequeue(out m))
            {
                instance.ReceiveMessage(m);
            }
        }

        /// <summary>
        ///  Stops writing and reading threads.
        ///  When running in unity, this must be called on a MonoBehavior's OnDisable callback.
        ///  for a clean shutdown.
        /// </summary>
        public async Task StopAsync()
        {
            Done = true;
            UnboundedCancellationSource.Cancel();

            try
            {
                if (Websocket.State != WebSocketState.Closed && Websocket.State != WebSocketState.Aborted)
                {
                    using (CancellationTokenSource src = TokenSource())
                    {
                        await Websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "going away", src.Token)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ignore
                LogMessage(ex.ToString());
            }

            if (WriteThread != null)
            {
                WriteThread.Join(TimeSpan.FromSeconds(5));
                WriteThread = null;
            }

            if (ReadThread != null)
            {
                ReadThread.Join(TimeSpan.FromSeconds(5));
                WriteThread = null;
            }

            UnboundedCancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Sends all queued messages in the instance
        /// </summary>
        /// <param name="instance">instance to send messages from</param>
        /// <returns></returns>
        public async Task SendMessages(SDK instance)
        {
            if (Websocket.State != WebSocketState.Open)
            {
                return;
            }

            if (Reconnecting)
            {
                return;
            }

            List<string> messages = new List<string>();
            instance.ForEachPayload((string Payload) =>
            {
                messages.Add(Payload);
            });

            try
            {
                foreach (string msg in messages)
                {
                    var bytes = new ArraySegment<byte>(UTF8Encoding.GetBytes(msg));

                    using (CancellationTokenSource src = TokenSource())
                    {
                        await Websocket.SendAsync(bytes, WebSocketMessageType.Text, true, src.Token)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Don't try.
                return;
            }
            catch (Exception ex)
            {
                LogMessage(ex.ToString());

                if (!Done)
                {
                    await AttemptReconnect(instance);
                }
                else
                {
                    return;
                }
            }
        }

        public async Task SendMessages(MuxyGateway.SDK instance)
        {
            if (Websocket.State != WebSocketState.Open)
            {
                return;
            }

            if (Reconnecting)
            {
                return;
            }

            List<byte[]> messages = new List<byte[]>();

            instance.ForeachPayload((MuxyGateway.Payload Payload) =>
            {
                messages.Add(Payload.Bytes);
            });

            try
            {
                foreach (byte[] msg in messages)
                {
                    var bytes = new ArraySegment<byte>(msg);

                    using (CancellationTokenSource src = TokenSource())
                    {
                        await Websocket.SendAsync(bytes, WebSocketMessageType.Text, true, src.Token)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Don't try.
                return;
            }
            catch (Exception ex)
            {
                LogMessage(ex.ToString());

                if (!Done)
                {
                    await AttemptReconnect(instance);
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        ///  Receives a single message to the SDK from the active websocket connection.
        /// </summary>
        /// <param name="instance">The instance to receive a mesage to</param>
        /// <returns></returns>
        public async Task ReceiveMessage(SDK instance)
        {
            MemoryStream memory = new MemoryStream();
            while (!Done)
            {
                if (Websocket.State != WebSocketState.Open)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (Reconnecting)
                {
                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    // Reading has an infinite timeout
                    ArraySegment<byte> segment = new ArraySegment<byte>(new byte[1024]);
                    var Result = await Websocket.ReceiveAsync(segment, UnboundedCancellationSource.Token)
                        .ConfigureAwait(false);

                    if (Result.MessageType == WebSocketMessageType.Close)
                    {
                        throw new EndOfStreamException("Closed");
                    }

                    if (Result.MessageType != WebSocketMessageType.Text)
                    {
                        throw new InvalidDataException("Message type was not text");
                    }

                    memory.Write(segment.Array, 0, Result.Count);
                    if (Result.EndOfMessage)
                    {
                        break;
                    }
                }
                catch (ThreadAbortException)
                {
                    // Don't try.
                    return;
                }
                catch (Exception ex)
                {
                    LogMessage(ex.ToString());

                    if (!Done)
                    {
                        await AttemptReconnect(instance);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (memory.Length == 0)
            {
                return;
            }

            string input = UTF8Encoding.GetString(memory.ToArray());
            if (HandleMessagesInMainThread)
            {
                Messages.Enqueue(input);
            }
            else
            {
                instance.ReceiveMessage(input);
            }
        }


        public async Task ReceiveMessage(MuxyGateway.SDK instance)
        {
            MemoryStream memory = new MemoryStream();
            while (!Done)
            {
                if (Websocket.State != WebSocketState.Open)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (Reconnecting)
                {
                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    // Reading has an infinite timeout
                    ArraySegment<byte> segment = new ArraySegment<byte>(new byte[1024]);
                    var Result = await Websocket.ReceiveAsync(segment, UnboundedCancellationSource.Token)
                        .ConfigureAwait(false);

                    if (Result.MessageType == WebSocketMessageType.Close)
                    {
                        throw new EndOfStreamException("Closed");
                    }

                    if (Result.MessageType != WebSocketMessageType.Text)
                    {
                        throw new InvalidDataException("Message type was not text");
                    }

                    memory.Write(segment.Array, 0, Result.Count);
                    if (Result.EndOfMessage)
                    {
                        break;
                    }
                }
                catch (ThreadAbortException)
                {
                    // Don't try.
                    return;
                }
                catch (Exception ex)
                {
                    LogMessage(ex.ToString());
                    if (!Done)
                    {
                        await AttemptReconnect(instance);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            string input = UTF8Encoding.GetString(memory.ToArray());

            if (HandleMessagesInMainThread)
            {
                Messages.Enqueue(input);
            }
            else
            {
                instance.ReceiveMessage(input);
            }
        }

        private bool Reconnecting = false;
        private async Task AttemptReconnect(SDK instance)
        {
            if (Reconnecting)
            {
                return;
            }

            Reconnecting = true;
            LogMessage("Attempting reconnect.");

            if (Websocket.State != WebSocketState.Aborted)
            {
                try
                {
                    using (CancellationTokenSource src = TokenSource())
                    {
                        await Websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "going away", src.Token)
                            .ConfigureAwait(false);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Ignore this one
                }
            }

            // Setup the reconnection setup.
            int i = 0;
            while (!Done)
            {
                Websocket = new ClientWebSocket();

                try
                {
                    using (CancellationTokenSource src = TokenSource())
                    {
                        await Websocket.ConnectAsync(TargetUri, src.Token)
                            .ConfigureAwait(false);

                        instance.HandleReconnect();
                        Reconnecting = false;
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Not connected.
                    int waitMillis = 500 * (i * i + 1);
                    if (waitMillis > 30000)
                    {
                        waitMillis = 30000;
                    }

                    LogMessage(string.Format("Attempting to reconnect. attempt={0} wait={1}ms", i + 1, waitMillis));
                    Thread.Sleep(waitMillis);
                }

                i++;
            }
        }

        private async Task AttemptReconnect(MuxyGateway.SDK instance)
        {
            if (Reconnecting)
            {
                return;
            }

            Reconnecting = true;
            LogMessage("Attempting reconnect.");

            if (Websocket.State != WebSocketState.Aborted)
            {
                try
                {
                    using (CancellationTokenSource src = TokenSource())
                    {
                        await Websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "going away", src.Token)
                            .ConfigureAwait(false);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Ignore this one
                }
            }

            // Setup the reconnection setup.
            int i = 0;
            while (!Done)
            {
                Websocket = new ClientWebSocket();

                try
                {
                    using (CancellationTokenSource src = TokenSource())
                    {
                        await Websocket.ConnectAsync(TargetUri, src.Token)
                            .ConfigureAwait(false);

                        instance.HandleReconnect();
                        Reconnecting = false;
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Not connected.
                    int waitMillis = 500 * (i * i + 1);
                    if (waitMillis > 30000)
                    {
                        waitMillis = 30000;
                    }

                    LogMessage(string.Format("Attempting to reconnect. attempt={0} wait={1}ms", i + 1, waitMillis));
                    Thread.Sleep(waitMillis);
                }

                i++;
            }
        }
    }
}