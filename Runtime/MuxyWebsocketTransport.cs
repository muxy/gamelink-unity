using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

using UnityEngine;

namespace MuxyGameLink
{
	public class WebsocketTransport
	{
		private ClientWebSocket Websocket;
		private CancellationTokenSource CancellationSource = new CancellationTokenSource();
		private CancellationToken CancelToken => CancellationSource.Token;
		private static readonly Encoding UTF8Encoding = new UTF8Encoding(false);

		private bool HandleMessagesInMainThread = false;
		private ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

		private Thread WriteThread;
		private Thread ReadThread;
		private bool Done = false;


		/// <summary>
		///  Creates a websocket transport without an associated Gamelink instance or stage.
		/// <param name="HandleMessagesInMainThread">If you are using Unity or an engine that doesn't work nicely with multithreading this should be set to true. If set to true, you must call Update() for GameLink to receive messages</param>
		/// </summary>
		public WebsocketTransport(bool HandleMessagesInMainThread)
		{
			Websocket = new ClientWebSocket();
			this.HandleMessagesInMainThread = HandleMessagesInMainThread;
		}

		/// <summary>
		///  Opens a websocket connection to the given uri, usually computed by calling SDK.ConnectionAddress
		/// </summary>
		/// <param name="uri">URI to connect to. Must be prefixed with the protocol, usually "ws://"</param>
		/// <returns></returns>
		public async Task Open(string uri)
		{
			await Websocket.ConnectAsync(new Uri(uri), CancelToken).ConfigureAwait(false);
		}

		/// <summary>
		///  Combined call to Open and then Run. Computes the connection URL by calling SDK.ConnectionAddress.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="stage"></param>
		public async void OpenAndRunInStage(SDK instance, Stage stage)
		{
			await Open("ws://" + instance.ConnectionAddress(stage));
			Run(instance);
		}

		/// <summary>
		/// Sends all queued messages in the instance
		/// </summary>
		/// <param name="instance">instance to send messages from</param>
		/// <returns></returns>
		public async Task SendMessages(SDK instance)
		{
			List<string> messages = new List<string>();
			instance.ForEachPayload((string Payload) => {
				messages.Add(Payload);
			});

			foreach (string msg in messages)
			{
				var bytes = new ArraySegment<byte>(UTF8Encoding.GetBytes(msg));
				await Websocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancelToken).ConfigureAwait(false);
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

		/// <summary>
		///  Stops writing and reading threads.
		/// </summary>
		public void Stop()
        {
			Done = true;
			Websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "going away", CancelToken);
			WriteThread.Join();
			ReadThread.Join();
        }

		/// <summary>
		///  Receives a single message to the SDK from the active websocket connection.
		/// </summary>
		/// <param name="instance">The instance to receive a mesage to</param>
		/// <returns></returns>
		public async Task ReceiveMessage(SDK instance)
		{
			MemoryStream memory = new MemoryStream();
			while (true)
            {
				ArraySegment<byte> segment = new ArraySegment<byte>(new byte[1024]);
				var Result = await Websocket.ReceiveAsync(segment, CancelToken).ConfigureAwait(false);

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
	}
}