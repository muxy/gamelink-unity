using System.Text;
using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace MuxyGameLink
{
    public class WebsocketTransport
    {
        private ClientWebSocket Websocket;
        private CancellationTokenSource UnboundedCancellationSource = new CancellationTokenSource();
        private static readonly Encoding UTF8Encoding = new UTF8Encoding(false);

        private bool HandleMessagesInMainThread = false;
        private ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

        private Thread WriteThread;
        private Thread ReadThread;
        private bool Done = false;

        private Uri TargetUri;

        private CancellationTokenSource TokenSource()
        {
            CancellationTokenSource src = new CancellationTokenSource();
            src.CancelAfter(5000);

            return src;
        }

        /// <summary>
        ///  Creates a websocket transport without an associated Gamelink instance or stage.
        /// <param name="HandleMessagesInMainThread">If you are using Unity or an engine that doesn't work nicely with multithreading this should be set to true. If set to true, you must call Update() for GameLink to receive messages</param>
        /// </summary>
        public WebsocketTransport(bool HandleMessagesInMainThread)
        {
            Websocket = new ClientWebSocket();
            this.HandleMessagesInMainThread = HandleMessagesInMainThread;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    // If a user forgets to stop the websocket transport, the editor locks up.
                    // This is bad, prevent this by attaching an event to stop the websocket connection
                    // when the editor stops the PIE mode.
                    UnityEngine.Debug.Log("Stopping websocket transport due to editor state change.");
                    StopAsync().RunSynchronously();
                    UnityEngine.Debug.Log("This may cause errors while playing in editor, but prevents leaking a connection, which is worse.");
                }
            };

            EditorApplication.quitting += () =>
            {
                StopAsync().RunSynchronously();
            };
#endif
        }

        ~WebsocketTransport()
        {
            StopAsync().RunSynchronously();
        }

        /// <summary>
        ///  Opens a websocket connection to the given uri, usually computed by calling SDK.ConnectionAddress
        /// </summary>
        /// <param name="uri">URI to connect to. Must be prefixed with the protocol, usually "ws://"</param>
        /// <returns></returns>
        public async Task Open(string uri)
        {
            TargetUri = new Uri(uri);

            using (CancellationTokenSource src = TokenSource())
            {
                await Websocket.ConnectAsync(TargetUri, src.Token)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        ///  Combined call to Open and then Run. Computes the connection URL by calling SDK.ConnectionAddress.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="stage"></param>
        public async void OpenAndRunInStage(SDK instance, Stage stage)
        {
            await Open("ws://" + instance.ConnectionAddress(stage))
                .ConfigureAwait(false);
            Run(instance);
        }

        private async Task OpenAndRunInStage(MuxyGateway.SDK instance, Stage stage)
        {
            switch (stage)
            {
                case Stage.Sandbox:
                    {
                        string url = instance.GetSandboxURL();
                        await Open("ws://" + url)
                            .ConfigureAwait(false);
                        break;
                    }

                case Stage.Production:
                    {
                        string url = instance.GetProductionURL();
                        await Open("ws://" + url)
                            .ConfigureAwait(false);
                        break;
                    }
            }

            Run(instance);
        }

        public void OpenAndRunInSandbox(MuxyGateway.SDK instance)
        {
            OpenAndRunInStage(instance, Stage.Sandbox);
        }

        public void OpenAndRunInProduction(MuxyGateway.SDK instance)
        {
            OpenAndRunInStage(instance, Stage.Production);
        }

        public void Disconnect()
        {
            StopAsync();
        }

        /// <summary>
        /// Sends all queued messages in the instance
        /// </summary>
        /// <param name="instance">instance to send messages from</param>
        /// <returns></returns>
        public async Task SendMessages(SDK instance)
        {
            List<string> messages = new List<string>();
            instance.ForEachPayload((string Payload) =>
            {
                messages.Add(Payload);
            });

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

        public async Task SendMessages(MuxyGateway.SDK instance)
        {
            List<byte[]> messages = new List<byte[]>();

            instance.ForeachPayload((MuxyGateway.Payload Payload) =>
            {
                messages.Add(Payload.Bytes);
            });

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

        /// <summary>
        ///  Invokes SendMessages and ReceiveMessage on different threads until a call to Stop()
        ///  Any callbacks invoked from `instance` will be called on a background thread, not the main thread.
        /// </summary>
        /// <param name="instance">The instance to use for sending and receiving messages</param>
        public void Run(SDK instance)
        {
            Done = false;
            WriteThread = new Thread(async () =>
            {
                while (!Done)
                {
                    await SendMessages(instance);
                    Thread.Sleep(100);
                }
            });
            WriteThread.Start();

            ReadThread = new Thread(async () =>
            {
                while (!Done)
                {
                    await ReceiveMessage(instance);
                }
            });
            ReadThread.Start();
        }

        public void Run(MuxyGateway.SDK instance)
        {
            Done = false;
            WriteThread = new Thread(async () =>
            {
                while (!Done)
                {
                    await SendMessages(instance);
                    Thread.Sleep(100);
                }
            });
            WriteThread.Start();

            ReadThread = new Thread(async () =>
            {
                while (!Done)
                {
                    await ReceiveMessage(instance);
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
        /// </summary>
        public async Task StopAsync()
        {
            Done = true;

            try
            {
                using (CancellationTokenSource src = TokenSource())
                {
                    await Websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "going away", src.Token)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            UnboundedCancellationSource.Cancel();
            if (WriteThread != null)
            {
                WriteThread.Join();
            }

            if (ReadThread != null)
            {
                ReadThread.Join();
            }

            UnboundedCancellationSource = new CancellationTokenSource();
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

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

        private async Task AttemptReconnect(SDK instance)
        {
            if (Websocket.State != WebSocketState.Aborted)
            {
                using (CancellationTokenSource src = TokenSource())
                {
                    await Websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "going away", src.Token)
                        .ConfigureAwait(false);
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

                    Console.WriteLine("Attempting to reconnect. attempt={0} wait={1}ms", i + 1, waitMillis);
                    Thread.Sleep(waitMillis);
                }

                i++;
            }
        }

        public async Task ReceiveMessage(MuxyGateway.SDK instance)
        {
            MemoryStream memory = new MemoryStream();
            while (!Done)
            {
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

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

        private async Task AttemptReconnect(MuxyGateway.SDK instance)
        {
            if (Websocket.State != WebSocketState.Aborted)
            {
                using (CancellationTokenSource src = TokenSource())
                {
                    await Websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "going away", src.Token)
                        .ConfigureAwait(false);
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

                        Console.WriteLine("Connected?");

                        instance.HandleReconnect();
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

                    Console.WriteLine("Attempting to reconnect. attempt={0} wait={1}ms", i + 1, waitMillis);
                    Thread.Sleep(waitMillis);
                }

                i++;
            }
        }
    }
}