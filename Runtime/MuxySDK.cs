using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using MuxyGameLink.Imports;
using MuxyGameLink.Imports.Schema;

#if UNITY_EDITOR || UNITY_STANDALONE
using AOT;
using UnityEngine;
#endif

namespace MuxyGameLink
{
    public class SDK
    {
        /// <summary> Generates connection address for Websocket </summary>
        /// <param name="Stage"> Stage Production or Sandbox </param>
        /// <returns> Connection address </returns>
        public String ConnectionAddress(Stage Stage)
        {
            IntPtr Ptr = Imported.ProjectionWebsocketConnectionURL(this.ClientId, (Int32)Stage, "csharp", 0, 0, 1);
            return NativeString.StringFromUTF8AndDeallocate(Ptr);
        }

        /// <summary> Constructs the SDK </summary>
        /// <param name="ClientId"> Your given Muxy ClientId </param>
        public SDK(String ClientId)
        {
            this.Instance = Imported.Make();

            this.ClientId = ClientId;

            OnDatastreamHandles = new Dictionary<UInt32, GCHandle>();
            OnTransactionHandles = new Dictionary<UInt32, GCHandle>();
            OnStateUpdateHandles = new Dictionary<UInt32, GCHandle>();
            OnPollUpdateHandles = new Dictionary<UInt32, GCHandle>();
            OnConfigUpdateHandles = new Dictionary<UInt32, GCHandle>();
            OnMatchmakingUpdateHandles = new Dictionary<UInt32, GCHandle>();
        }

        public SDK(String ClientId, String GameId) : this(ClientId)
        {
            this.GameId = GameId;
        }

        ~SDK()
        {
            Imported.Kill(this.Instance);
        }

        private void LogMessage(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            LogMessage(message);
#else
            Console.Error.WriteLine(message);
#endif
        }

        #region Authentication and User Management
        /// <summary> Check if we are currently authenticated </summary>
        /// <returns> Returns true if we are currently authenticated </returns>
        public bool IsAuthenticated()
        {
            return Imported.IsAuthenticated(this.Instance) != 0;
        }

        public delegate void AuthenticationCallback(AuthenticationResponse Payload);

        private class AuthenticationInvocationParameters
        {
            public SDK SDK;
            public AuthenticationCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(AuthenticateResponseDelegate))]
#endif
        private static void InvokeAuthenticationCallback(IntPtr Data, AuthenticateResponse AuthResp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle Handle = GCHandle.FromIntPtr(Data);
            if (Handle.Target == null)
            {
                return;
            }

            AuthenticationInvocationParameters args = Handle.Target as AuthenticationInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                AuthenticationResponse Response = new AuthenticationResponse(AuthResp);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }

            Handle.Free();
        }

        /// <summary> Authenticate with refresh token, which is obtained from initially authenticating with a PIN </summary>
        /// <param name="RefreshToken"> The refresh token obtained from calling AuthenticateWithPIN </param>
        /// <param name="Callback"> The callback to be called when the authentication attempt finishes </param>
        /// <returns> RequestId </returns>
        public UInt16 AuthenticateWithRefreshToken(string RefreshToken, AuthenticationCallback Callback)
        {
            AuthenticationInvocationParameters args = new AuthenticationInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            return Imported.AuthenticateWithGameIDAndRefreshToken(this.Instance, this.ClientId, this.GameId, RefreshToken, InvokeAuthenticationCallback, GCHandle.ToIntPtr(Handle));
        }

        /// <summary> Authenticate with a PIN </summary>
        /// <param name="PIN"> User PIN </param>
        /// <param name="Callback"> The callback to be called when the authentication attempt finishes </param>
        /// <returns> RequestId </returns>
        public UInt16 AuthenticateWithPIN(string PIN, AuthenticationCallback Callback)
        {
            AuthenticationInvocationParameters args = new AuthenticationInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            return Imported.AuthenticateWithGameIDAndPIN(this.Instance, this.ClientId, this.GameId, PIN, InvokeAuthenticationCallback, GCHandle.ToIntPtr(Handle));
        }

        public UInt16 Deauthenticate()
        {
            return Imported.Deauthenticate(this.Instance);
        }

        public User User
        {
            get
            {
                if (!this.IsAuthenticated())
                {
                    return null;
                }

                if (this.CachedUserInstance != null)
                {
                    return this.CachedUserInstance;
                }

                this.CachedUserInstance = new User(Imported.GetSchemaUser(this.Instance));
                return this.CachedUserInstance;
            }
        }
        #endregion

        #region Network Interface
        /// <summary> Receive message for processing </summary>
        /// <param name="Message"> Message to be proccesed, commonly comes right from a Websocket </param>
        /// <returns> Returns true if the message was received correctly </returns>
        public bool ReceiveMessage(string Message)
        {
            return Imported.ReceiveMessage(this.Instance, Message, (uint)Message.Length) != 0;
        }

        public delegate void PayloadCallback(string Payload);

        private class PayloadInvocationParameters
        {
            public PayloadCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(PayloadDelegate))]
#endif
        private static void InvokeForeachPayload(IntPtr Data, Payload Payload)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle Handle = GCHandle.FromIntPtr(Data);
            if (Handle.Target == null)
            {
                return;
            }

            PayloadInvocationParameters args = Handle.Target as PayloadInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                IntPtr ptr = Imported.Payload_GetData(Payload);
                UInt64 len = Imported.Payload_GetSize(Payload);

                string str = NativeString.StringFromUTF8(ptr, ((int)len));

                args.Callback(str);
            }
        }

        /// <summary> Calls given callback on each payload waiting to be sent, generally used to send the payload through a Websocket </summary>
        /// <param name="Callback"> Callback to be called on each iteration </param>
        public void ForEachPayload(PayloadCallback Callback)
        {
            PayloadInvocationParameters args = new PayloadInvocationParameters();
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            Imported.ForeachPayload(this.Instance, InvokeForeachPayload, GCHandle.ToIntPtr(Handle));
            Handle.Free();
        }

        public void HandleReconnect()
        {
            Imported.HandleReconnect(Instance);
        }
        #endregion

        #region State Operations
        /// <summary> Set target state with JSON </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Json"> Json message to be stored in state </param>
        /// <returns> RequestId </returns>
        public UInt16 SetState(StateTarget Target, string Json)
        {
            return Imported.SetState(this.Instance, (Int32)Target, Json);
        }

        public delegate void GetStateCallback(StateResponse Response);

        private class GetStateInvocationParameters
        {
            public SDK SDK;
            public GetStateCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(StateGetDelegate))]
#endif
        private static void InvokeGetState(IntPtr Data, Imports.Schema.StateResponse StateResp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            GetStateInvocationParameters args = handle.Target as GetStateInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                StateResponse Response = new StateResponse(StateResp);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }

            handle.Free();
        }

        /// <summary> Get target state </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Callback"> Callback to be called with state info</param>
        /// <returns> RequestId </returns>
        public UInt16 GetState(StateTarget Target, GetStateCallback Callback)
        {
            GetStateInvocationParameters args = new GetStateInvocationParameters();
            args.Callback = Callback;
            args.SDK = this;

            GCHandle handle = GCHandle.Alloc(args);
            return Imported.GetState(this.Instance, (Int32)Target, InvokeGetState, GCHandle.ToIntPtr(handle));
        }

        /// <summary> Set target state with integer </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="Value"> Integer to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateStateWithInteger(StateTarget Target, Operation Operation, string Path, Int64 Value)
        {
            return Imported.UpdateStateWithInteger(this.Instance, (Int32)Target, (Int32)Operation, Path, Value);
        }

        /// <summary> Set target state with doule </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="Value"> Double to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateStateWithDouble(StateTarget Target, Operation Operation, string Path, Double Value)
        {
            return Imported.UpdateStateWithDouble(this.Instance, (Int32)Target, (Int32)Operation, Path, Value);
        }

        /// <summary> Set target state with string </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="Value"> String to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateStateWithString(StateTarget Target, Operation Operation, string Path, string Value)
        {
            return Imported.UpdateStateWithString(this.Instance, (Int32)Target, (Int32)Operation, Path, Value);
        }

        /// <summary> Set target state with json literal </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="Value"> Json literal to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateStateWithLiteral(StateTarget Target, Operation Operation, string Path, string JsonLiteral)
        {
            return Imported.UpdateStateWithLiteral(this.Instance, (Int32)Target, (Int32)Operation, Path, JsonLiteral);
        }

        /// <summary> Set target state with null </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateStateWithNull(StateTarget Target, Operation Operation, string Path)
        {
            return Imported.UpdateStateWithNull(this.Instance, (Int32)Target, (Int32)Operation, Path);
        }

        /// <summary> Updates target state with PatchList </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateStateWithPatchList(StateTarget Target, PatchList PList)
        {
            return Imported.UpdateStateWithPatchList(this.Instance, (Int32)Target, PList.Obj);
        }

        /// <summary> Subscribe to state updates, a callback must first be set with OnStateUpdate </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <returns> RequestId </returns>
        public UInt16 SubscribeToStateUpdates(StateTarget Target)
        {
            return Imported.SubscribeToStateUpdates(this.Instance, (Int32)Target);
        }

        /// <summary> Unsubscribe from state updates </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <returns> RequestId </returns>
        public UInt16 UnsubscribeFromStateUpdates(StateTarget Target)
        {
            return Imported.UnsubscribeFromStateUpdates(this.Instance, (Int32)Target);
        }

        public delegate void UpdateStateCallback(StateUpdate Response);

        private class UpdateStateInvocationParameters
        {
            public SDK SDK;
            public UpdateStateCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(UpdateStateCallback))]
#endif
        private static void InvokeUpdateState(IntPtr Data, Imports.Schema.StateUpdate StateUpd)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            UpdateStateInvocationParameters args = handle.Target as UpdateStateInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                StateUpdate Response = new StateUpdate(StateUpd);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }
        }

        /// <summary> Sets callback for state updates, requires call to SubscribeToStateUpdates to begin receiving updates </summary>
        /// <param name="Callback"> Callback that will be called when an update occurs</param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnStateUpdate(UpdateStateCallback Callback)
        {
            UpdateStateInvocationParameters args = new UpdateStateInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            UInt32 Result = Imported.OnStateUpdate(this.Instance, InvokeUpdateState, GCHandle.ToIntPtr(Handle));

            OnStateUpdateHandles.Add(Result, Handle);
            return Result;
        }

        /// <summary> Detach callback for state updates</summary>
        /// <param name="Handle"> Handle given from OnStateUpdate </param>
        public void DetachOnStateUpdate(UInt32 Handle)
        {
            Imported.DetachOnStateUpdate(this.Instance, Handle);
            GCHandle GC = OnStateUpdateHandles[Handle];
            if (GC != null)
            {
                GC.Free();
                OnStateUpdateHandles.Remove(Handle);
            }
        }
        #endregion

        #region Configuration Operations
        /// <summary> Sets channel config </summary>
        /// <param name="JsonLiteral"> Json to store in channel config</param>
        /// <returns> RequestId </returns>
        public UInt16 SetChannelConfig(string JsonLiteral)
        {
            return Imported.SetChannelConfig(this.Instance, JsonLiteral);
        }

        public delegate void GetConfigCallback(ConfigResponse Response);

        private class GetConfigInvocationParameters
        {
            public SDK SDK;
            public GetConfigCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(ConfigGetDelegate))]
#endif
        private static void InvokeGetConfig(IntPtr Data, Imports.Schema.ConfigResponse CfgUpd)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            GetConfigInvocationParameters args = handle.Target as GetConfigInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                ConfigResponse Response = new ConfigResponse(CfgUpd);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }

            handle.Free();
        }

        /// <summary> Get config </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Callback"> Callback to be called to receive config data </param>
        /// <returns> RequestId </returns>
        public UInt16 GetConfig(ConfigTarget Target, GetConfigCallback Callback)
        {
            GetConfigInvocationParameters args = new GetConfigInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle handle = GCHandle.Alloc(args);
            return Imported.GetConfig(this.Instance, (Int32)Target, InvokeGetConfig, GCHandle.ToIntPtr(handle));
        }

        /// <summary> Update config with integer </summary>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="Value"> Integer to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateChannelConfigWithInteger(Operation Operation, string Path, Int64 Value)
        {
            return Imported.UpdateChannelConfigWithInteger(this.Instance, (Int32)Operation, Path, Value);
        }

        /// <summary> Update config with double </summary>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="Value"> Double to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateChannelConfigWithDouble(Operation Operation, string Path, Double Value)
        {
            return Imported.UpdateChannelConfigWithDouble(this.Instance, (Int32)Operation, Path, Value);
        }

        /// <summary> Update config with string </summary>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="Value"> String to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateChannelConfigWithString(Operation Operation, string Path, string Value)
        {
            return Imported.UpdateChannelConfigWithString(this.Instance, (Int32)Operation, Path, Value);
        }

        /// <summary> Update config with json literal </summary>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <param name="JsonLiteral"> Json literal to be set </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateChannelConfigWithLiteral(Operation Operation, string Path, string JsonLiteral)
        {
            return Imported.UpdateChannelConfigWithLiteral(this.Instance, (Int32)Operation, Path, JsonLiteral);
        }

        /// <summary> Update config with null </summary>
        /// <param name="Operation"> Patch operation </param>
        /// <param name="Path"> Json Pointer </param>
        /// <returns> RequestId </returns>
        public UInt16 UpdateChannelConfigWithNull(Operation Operation, string Path)
        {
            return Imported.UpdateChannelConfigWithNull(this.Instance, (Int32)Operation, Path);
        }

        /// <summary> Subscribe to configuration changes, a callback must first be set with OnConfigUpdate </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <returns> RequestId </returns>
        public UInt16 SubscribeToConfigurationChanges(ConfigTarget Target)
        {
            return Imported.SubscribeToConfigurationChanges(this.Instance, (Int32)Target);
        }

        /// <summary> Unsubscribe from configuration changes </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <returns> RequestId </returns>
        public UInt16 UnsubscribeFromConfigurationChanges(ConfigTarget Target)
        {
            return Imported.UnsubscribeFromConfigurationChanges(this.Instance, (Int32)Target);
        }

        public delegate void UpdateConfigCallback(ConfigUpdate Response);

        private class UpdateConfigInvocationParameters
        {
            public SDK SDK;
            public UpdateConfigCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(ConfigUpdateDelegate))]
#endif
        private static void InvokeUpdateConfig(IntPtr Data, Imports.Schema.ConfigUpdate Upd)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            UpdateConfigInvocationParameters args = handle.Target as UpdateConfigInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                ConfigUpdate Response = new ConfigUpdate(Upd);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }
        }

        /// <summary> Sets callback for config updates, requires call to SubscribeToConfigurationChanges to begin receiving updates </summary>
        /// <param name="Callback"> Callback that will be called when an update occurs</param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnConfigUpdate(UpdateConfigCallback Callback)
        {
            UpdateConfigInvocationParameters args = new UpdateConfigInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            UInt32 Result = Imported.OnConfigUpdate(this.Instance, InvokeUpdateConfig, GCHandle.ToIntPtr(Handle));

            OnConfigUpdateHandles.Add(Result, Handle);
            return Result;
        }

        /// <summary> Detach callback for config updates</summary>
        /// <param name="Handle"> Handle given from OnConfigUpdate </param>
        public void DetachOnConfigUpdate(UInt32 Handle)
        {
            Imported.DetachOnConfigUpdate(this.Instance, Handle);
            GCHandle GC = OnConfigUpdateHandles[Handle];
            if (GC != null)
            {
                GC.Free();
                OnConfigUpdateHandles.Remove(Handle);
            }
        }
        #endregion

        #region Broadcasts
        /// <summary> Broadcast message </summary>
        /// <param name="Topic"> Topic of the broadcast </param>
        /// <param name="Json"> Json to be sent in the broadcast</param> 
        /// <returns> RequestId </returns>
        public UInt16 SendBroadcast(string Topic, string Json)
        {
            return Imported.SendBroadcast(this.Instance, Topic, Json);
        }

        /// <summary> Broadcast message </summary>
        /// <param name="Topic"> Topic of the broadcast </param>
        /// <returns> RequestId </returns>
        public UInt16 SendBroadcast(string Topic)
        {
            return Imported.SendBroadcast(this.Instance, Topic, "{}");
        }
        #endregion

        #region Datastream
        /// <summary> Subscribe to datastream, a callback must first be set with OnDatastream </summary>
        /// <returns> RequestId </returns>
        public UInt16 SubscribeToDatastream()
        {
            return Imported.SubscribeToDatastream(this.Instance);
        }

        /// <summary> Unsubscribe from datastream </summary>
        /// <returns> RequestId </returns>
        public UInt16 UnsubscribeFromDatastream()
        {
            return Imported.UnsubscribeFromDatastream(this.Instance);
        }

        public delegate void DatastreamCallback(DatastreamUpdate Update);

        private class DatastreamInvocationParameters
        {
            public SDK SDK;
            public DatastreamCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(DatastreamUpdateDelegate))]
#endif
        private static void InvokeDatastream(IntPtr Data, Imports.Schema.DatastreamUpdate Upd)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            DatastreamInvocationParameters args = handle.Target as DatastreamInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                DatastreamUpdate Response = new DatastreamUpdate(Upd);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }
        }

        /// <summary> Sets callback for datastream, requires call to SubscribeToDatastream to begin receiving updates </summary>
        /// <param name="Callback"> Callback that will be called when an update occurs</param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnDatastream(DatastreamCallback Callback)
        {
            DatastreamInvocationParameters args = new DatastreamInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            UInt32 Result = Imported.OnDatastream(this.Instance, InvokeDatastream, GCHandle.ToIntPtr(Handle));

            OnDatastreamHandles.Add(Result, Handle);
            return Result;
        }

        /// <summary> Detach callback for datastream</summary>
        /// <param name="Handle"> Handle given from OnDatastream </param>
        public void DetachOnDatastream(UInt32 Handle)
        {
            Imported.DetachOnDatastream(this.Instance, Handle);
            GCHandle GC = OnDatastreamHandles[Handle];
            if (GC != null)
            {
                GC.Free();
                OnDatastreamHandles.Remove(Handle);
            }
        }
        #endregion

        #region Twitch Purchases
        /// <summary> Subscribe to SKU, a callback must first be set with OnTransaction </summary>
        /// <param name="SKU"> SKU to subscribe to </param>
        /// <returns> RequestId </returns>
        public UInt16 SubscribeToSKU(string SKU)
        {
            return Imported.SubscribeToSKU(this.Instance, SKU);
        }

        /// <summary> Unsubscribe from SKU </summary>
        /// <param name="SKU"> SKU to unsubscribe from</param>
        /// <returns> RequestId </returns>
        public UInt16 UnsubscribeFromSKU(string SKU)
        {
            return Imported.UnsubscribeFromSKU(this.Instance, SKU);
        }

        /// <summary> Subscribe to all Twitch Bit Purchases </summary>
        /// <returns> RequestId </returns>
        public UInt16 SubscribeToAllPurchases()
        {
            return Imported.SubscribeToAllPurchases(this.Instance);
        }

        /// <summary> Unsubscribe from all Twitch Bit Purchases </summary>
        /// <returns> RequestId </returns>
        public UInt16 UnsubscribeFromAllPurchases()
        {
            return Imported.UnsubscribeFromAllPurchases(this.Instance);
        }

        public delegate void TransactionCallback(Transaction Purchase);

        private class TransactionInvocationParameters
        {
            public SDK SDK;
            public TransactionCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(TransactionResponseDelegate))]
#endif
        private static void InvokeTransaction(IntPtr Data, Imports.Schema.TransactionResponse Resp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            TransactionInvocationParameters args = handle.Target as TransactionInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                Transaction Response = new Transaction(Resp);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }
        }

        /// <summary> Sets callback for Twitch Bit Purchasing, requires call to SubscribeToSKU or SubscribeToAllPurchases to begin receiving purchase updates </summary>
        /// <param name="Callback"> Callback that will be called when a purchase occurs </param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnTransaction(TransactionCallback Callback)
        {
            TransactionInvocationParameters args = new TransactionInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            UInt32 Result = Imported.OnTransaction(this.Instance, InvokeTransaction, GCHandle.ToIntPtr(Handle));

            OnTransactionHandles.Add(Result, Handle);
            return Result;
        }

        /// <summary> Detach callback for Twitch Bit Purchases</summary>
        /// <param name="Handle"> Handle given from OnTransaction </param>
        public void DetachOnTransaction(UInt32 Handle)
        {
            Imported.DetachOnTransaction(this.Instance, Handle);
            GCHandle GC = OnTransactionHandles[Handle];
            if (GC != null)
            {
                GC.Free();
                OnTransactionHandles.Remove(Handle);
            }
        }

        public delegate void GetOutstandingTransactionsCallback(OutstandingTransactions Transactions);

        private class OutstandingTransactionsInvocationParameters
        {
            public SDK SDK;
            public GetOutstandingTransactionsCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(GetOutstandingTransactionsDelegate))]
#endif
        private static void InvokeGetOutstandingTransactions(IntPtr Data, Imports.Schema.GetOutstandingTransactionsResponse Resp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            OutstandingTransactionsInvocationParameters args = handle.Target as OutstandingTransactionsInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                OutstandingTransactions Response = new OutstandingTransactions(Resp);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }

            handle.Free();
        }

        /// <summary> Get all outstanding transactions that need validation </summary>
        /// <param name="SKU"> SKU to get outstanding transactions for </param>
        /// <param name="Callback"> Callback to receive transactions info </param>
        /// <returns> RequestId </returns>
        public UInt16 GetOutstandingTransactions(String SKU, GetOutstandingTransactionsCallback Callback)
        {
            OutstandingTransactionsInvocationParameters args = new OutstandingTransactionsInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            return Imported.GetOutstandingTransactions(this.Instance, SKU, InvokeGetOutstandingTransactions, GCHandle.ToIntPtr(Handle));
        }

        /// <summary> Refund given transaction for user </summary>
        /// <param name="TxId"> TransactionId for refund </param>
        /// <param name="UserId"> UserId to receive refund </param>
        /// <returns> RequestId </returns>
        public UInt16 RefundTransactionByID(String TxId, String UserId)
        {
            return Imported.RefundTransactionByID(this.Instance, TxId, UserId);
        }

        /// <summary> Refund given transaction for user </summary>
        /// <param name="SKU"> SKU for refund </param>
        /// <param name="UserId"> UserId to receive refund </param>
        /// <returns> RequestId </returns>
        public UInt16 RefundTransactionBySKU(String SKU, String UserId)
        {
            return Imported.RefundTransactionBySKU(this.Instance, SKU, UserId);
        }

        /// <summary> Validate given transaction </summary>
        /// <param name="TxId"> TransactionId to validate </param>
        /// <param name="Details"> Extra details for validation </param>
        /// <returns> RequestId </returns>
        public UInt16 ValidateTransaction(String TxId, String Details)
        {
            return Imported.ValidateTransaction(this.Instance, TxId, Details);
        }

        #endregion

        #region Polling
        /// <summary> Create Poll with prompt and options </summary>
        /// <param name="PollId"> Id to refer to this poll later </param>
        /// <param name="Prompt"> Message prompt for the poll </param>
        /// <param name="Options"> List of poll options to choose from </param>
        /// <returns> RequestId </returns>
        public UInt16 CreatePoll(String PollId, String Prompt, List<String> Options)
        {
            return Imported.CreatePoll(this.Instance, PollId, Prompt, Options.ToArray(), (UInt32)Options.Count);
        }

        /// <summary> Create Poll with prompt and options along with extra configuration</summary>
        /// <param name="PollId"> Id to refer to this poll later </param>
        /// <param name="Prompt"> Message prompt for the poll </param>
        /// <param name="Config"> Poll configuration options </param>
        /// <param name="Options"> List of poll options to choose from </param>
        /// <returns> RequestId </returns>
        public UInt16 CreatePollWithConfiguration(String PollId, String Prompt, Imported.PollConfiguration Config, List<String> Options)
        {
            return Imported.CreatePollWithConfiguration(this.Instance, PollId, Prompt, ref Config, Options.ToArray(), (UInt32)Options.Count);
        }

        /// <summary> Subscribe to Poll, a callback must first be set with OnPollUpdate </summary>
        /// <param name="PollId"> Poll Id to subscribe to, given from CreatePoll </param>
        /// <returns> RequestId </returns>
        public UInt16 SubscribeToPoll(String PollId)
        {
            return Imported.SubscribeToPoll(this.Instance, PollId);
        }

        /// <summary> Unsubscribe from Poll </summary>
        /// <param name="PollId"> Poll Id to unsubcribe from, given from CreatePoll </param>
        /// <returns> RequestId </returns>
        public UInt16 UnsubscribeFromPoll(String PollId)
        {
            return Imported.UnsubscribeFromPoll(this.Instance, PollId);
        }

        /// <summary> Delete Poll </summary>
        /// <param name="PollId"> Poll Id to delete, given from CreatePoll </param>
        /// <returns> RequestId </returns>
        public UInt16 DeletePoll(String PollId)
        {
            return Imported.DeletePoll(this.Instance, PollId);
        }

        public delegate void GetPollCallback(GetPollResponse Response);

        private class GetPollInvocationParameters
        {
            public SDK SDK;
            public GetPollCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(GetPollResponseDelegate))]
#endif
        private static void InvokeGetPollCallback(IntPtr Data, Imports.Schema.GetPollResponse Resp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            GetPollInvocationParameters args = handle.Target as GetPollInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                GetPollResponse Response = new GetPollResponse(Resp);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }

            handle.Free();
        }

        /// <summary> Get Poll information once </summary>
        /// <param name="PollId"> Poll Id to delete, given from CreatePoll </param>
        /// <param name="Callback"> Callback to receive poll info </param>
        /// <returns> RequestId </returns>
        public UInt16 GetPoll(String PollId, GetPollCallback Callback)
        {
            GetPollInvocationParameters args = new GetPollInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            return Imported.GetPoll(this.Instance, PollId, InvokeGetPollCallback, GCHandle.ToIntPtr(Handle));
        }

        public delegate void PollUpdateResponseCallback(PollUpdateResponse PResp);
        private class PollUpdateInvocationParameters
        {
            public SDK SDK;
            public PollUpdateResponseCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(PollUpdateResponseDelegate))]
#endif
        private static void InvokePollUpdateCallback(IntPtr Data, Imports.Schema.PollUpdateResponse Resp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            PollUpdateInvocationParameters args = handle.Target as PollUpdateInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                PollUpdateResponse Response = new PollUpdateResponse(Resp);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }
        }

        /// <summary> Sets callback for Poll Updates, requires call to SubscribeToPoll to begin receiving poll updates </summary>
        /// <param name="Callback"> Callback that will be called when a poll update occurs </param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnPollUpdate(PollUpdateResponseCallback Callback)
        {
            PollUpdateInvocationParameters args = new PollUpdateInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            UInt32 Result = Imported.OnPollUpdate(this.Instance, InvokePollUpdateCallback, GCHandle.ToIntPtr(Handle));

            OnPollUpdateHandles.Add(Result, Handle);
            return Result;
        }

        /// <summary> Detach callback for poll updates </summary>
        /// <param name="Handle"> Handle to detach </param>
        public void DetachOnPollUpdate(UInt32 Handle)
        {
            Imported.DetachOnPollUpdate(this.Instance, Handle);
            GCHandle GC = OnPollUpdateHandles[Handle];
            if (GC != null)
            {
                GC.Free();
                OnPollUpdateHandles.Remove(Handle);
            }
        }

        #endregion

        #region Matchmaking
        /// <summary> Subscribe to MatchmakingQueueInvite, a callback must first be set with OnMatchmakingQueueInvite </summary>
        /// <returns> RequestId </returns>
        public UInt16 SubscribeToMatchmakingQueueInvite()
        {
            return Imported.SubscribeToMatchmakingQueueInvite(this.Instance);
        }
        /// <summary> Unsubscribe from MatchmakingQueueInvite </summary>
        /// <returns> RequestId </returns>
        public UInt16 UnsubscribeFromMatchmakingQueueInvite()
        {
            return Imported.UnsubscribeFromMatchmakingQueueInvite(this.Instance);
        }
        /// <summary> Clears the current matchmaking queue </summary>
        /// <returns> RequestId </returns>
        public UInt16 ClearMatchmakingQueue()
        {
            return Imported.ClearMatchmakingQueue(this.Instance);
        }
        /// <summary> Removes specified entry from matchmaking queue </summary>
        /// <param name="Id"> Id of the entry to remove </param>
        /// <returns> ReturnId </returns>
        public UInt16 RemoveMatchmakingEntry(String Id)
        {
            return Imported.RemoveMatchmakingEntry(this.Instance, Id);
        }

        public delegate void MatchmakingUpdateCallback(MatchmakingUpdate MResp);
        private class MatchmakingUpdateInvocationParameters
        {
            public SDK SDK;
            public MatchmakingUpdateCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(MatchmakingUpdateDelegate))]
#endif
        private static void InvokeMatchmakingUpdateCallback(IntPtr Data, Imports.Schema.MatchmakingUpdateResponse Resp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle handle = GCHandle.FromIntPtr(Data);
            if (handle.Target == null)
            {
                return;
            }

            MatchmakingUpdateInvocationParameters args = handle.Target as MatchmakingUpdateInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                MatchmakingUpdate Response = new MatchmakingUpdate(Resp);
                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }
        }

        /// <summary> Sets callback for QueueInvites, requires call to SubscribeToMatchmakingQueueInvite to begin receiving queue updates </summary>
        /// <param name="Callback"> Callback that will be called when a queue invite occurs </param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnMatchmakingQueueInvite(MatchmakingUpdateCallback Callback)
        {
            MatchmakingUpdateInvocationParameters args = new MatchmakingUpdateInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            UInt32 Result = Imported.OnMatchmakingQueueInvite(this.Instance, InvokeMatchmakingUpdateCallback, GCHandle.ToIntPtr(Handle));

            OnMatchmakingUpdateHandles.Add(Result, Handle);
            return Result;
        }

        /// <summary> Detach callback for matchmaking queue invite </summary>
        /// <param name="Handle"> Handle to detach </param>
        public void DetachOnMatchmakingQueueInvite(UInt32 Handle)
        {
            Imported.DetachOnMatchmakingQueueInvite(this.Instance, Handle);
            GCHandle GC = OnMatchmakingUpdateHandles[Handle];
            if (GC != null)
            {
                GC.Free();
                OnMatchmakingUpdateHandles.Remove(Handle);
            }
        }

        #endregion

        #region Metadata
        public class GameMetadata
        {
            public string Name { get; set; } = string.Empty;
            public string Logo { get; set; } = string.Empty;
            public string Theme { get; set; } = string.Empty;
        }

        public UInt16 SetGameMetadata(GameMetadata InMeta)
        {
            Imports.Schema.GameMetadata Meta = new();
            Meta.GameLogo = InMeta.Logo;
            Meta.GameName = InMeta.Name;
            Meta.Theme = InMeta.Theme;

            return Imported.SetGameMetadata(Instance, Meta);
        }
        #endregion

        #region Drops
        public delegate void GetDropsCallback(GetDropsResponse Resp);

        private class GetDropsInvocationParameters
        {
            public SDK SDK;
            public GetDropsCallback Callback;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(GetDropsResponseDelegate))]
#endif
        private static void InvokeGetDropsCallback(IntPtr Data, Imports.Schema.GetDropsResponse Resp)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle Handle = GCHandle.FromIntPtr(Data);
            if (Handle.Target == null)
            {
                return;
            }

            GetDropsInvocationParameters args = Handle.Target as GetDropsInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.Callback != null)
            {
                GetDropsResponse Response = new GetDropsResponse(Resp);

                try
                {
                    args.Callback(Response);
                }
                catch (Exception e)
                {
                    args.SDK?.LogMessage("Callbacks cannot throw, caught and discarded exception: " + e);
                }
            }

            Handle.Free();
        }

        public UInt16 GetDrops(String Status, GetDropsCallback Callback)
        {
            GetDropsInvocationParameters args = new GetDropsInvocationParameters();
            args.SDK = this;
            args.Callback = Callback;

            GCHandle Handle = GCHandle.Alloc(args);
            UInt16 Result = Imported.GetDrops(this.Instance, Status, InvokeGetDropsCallback, GCHandle.ToIntPtr(Handle));

            return Result;
        }

        public UInt16 ValidateDrop(String DropId)
        {
            return Imported.ValidateDrop(this.Instance, DropId);
        }
        #endregion

        public class PatchList
        {
            public PatchList()
            {
                this.Obj = Imported.PatchList_Make();
            }
            public void FreeMemory()
            {
                Imported.PatchList_Kill(this.Obj);
            }

            public void UpdateStateWithInteger(Operation Operation, String Path, Int64 Val)
            {
                Imported.PatchList_UpdateStateWithInteger(this.Obj, (Int32)Operation, Path, Val);
            }

            public void UpdateStateWithDouble(Operation Operation, String Path, double Val)
            {
                Imported.PatchList_UpdateStateWithDouble(this.Obj, (Int32)Operation, Path, Val);
            }

            public void UpdateStateWithBoolean(Operation Operation, String Path, bool Val)
            {
                Imported.PatchList_UpdateStateWithBoolean(this.Obj, (Int32)Operation, Path, Val);
            }

            public void UpdateStateWithString(Operation Operation, String Path, String Val)
            {
                Imported.PatchList_UpdateStateWithString(this.Obj, (Int32)Operation, Path, Val);
            }

            public void UpdateStateWithLiteral(Operation Operation, String Path, String Val)
            {
                Imported.PatchList_UpdateStateWithLiteral(this.Obj, (Int32)Operation, Path, Val);
            }

            public void UpdateStateWithNull(Operation Operation, String Path)
            {
                Imported.PatchList_UpdateStateWithNull(this.Obj, (Int32)Operation, Path);
            }

            public void UpdateStateWithJson(Operation Operation, String Path, String Val)
            {
                Imported.PatchList_UpdateStateWithJson(this.Obj, (Int32)Operation, Path, Val);
            }

            public void UpdateStateWithEmptyArray(Operation Operation, String Path)
            {
                Imported.PatchList_UpdateStateWithEmptyArray(this.Obj, (Int32)Operation, Path);
            }

            public bool Empty()
            {
                return Imported.PatchList_Empty(this.Obj) != 0;
            }
            public void Clear()
            {
                Imported.PatchList_Clear(this.Obj);
            }

            public Imports.Schema.PatchList Obj;
        }

        #region Debugging
        public delegate void OnDebugMessageCallback(String Message);
        private OnDebugMessageCallback DebugMessageCallback;
        private GCHandle OnDebugMessageHandle;

        private class OnDebugMessageInvocationParameters
        {
            public SDK sdk;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        [MonoPInvokeCallback(typeof(DebugMessageDelegate))]
#endif
        private static void InvokeDebugMessageCallback(IntPtr Data, String Message)
        {
            if (Data == IntPtr.Zero)
            {
                return;
            }

            GCHandle Handle = GCHandle.FromIntPtr(Data);
            if (Handle.Target == null)
            {
                return;
            }

            OnDebugMessageInvocationParameters args = Handle.Target as OnDebugMessageInvocationParameters;

            if (args == null)
            {
                return;
            }

            if (args.sdk?.DebugMessageCallback != null)
            {
                args.sdk?.DebugMessageCallback(Message);
            }
        }

        /// <summary> Detach callback for poll updates </summary>
        /// <param name="Handle"> Handle to detach </param>
        public void OnDebugMessage(OnDebugMessageCallback Callback)
        {
            DebugMessageCallback = Callback;

            OnDebugMessageInvocationParameters args = new OnDebugMessageInvocationParameters();
            args.sdk = this;

            OnDebugMessageHandle = GCHandle.Alloc(args);
            Imported.OnDebugMessage(this.Instance, InvokeDebugMessageCallback, GCHandle.ToIntPtr(OnDebugMessageHandle));
        }

        /// <summary> Detach callback for debug messages </summary>
        /// <param name="Handle"> Handle to detach </param>
        public void DetachOnDebugMessage()
        {
            Imported.DetachOnDebugMessage(this.Instance);
            try
            {
                OnDebugMessageHandle.Free();
            }
            catch (InvalidOperationException)
            {
                // Ignore
            }
            DebugMessageCallback = null;
        }
        #endregion

        #region Members
        public String ClientId { get; set; } = string.Empty;
        public String GameId { get; set; } = string.Empty;

        private SDKInstance Instance;
        private User CachedUserInstance;

        private Dictionary<UInt32, GCHandle> OnDatastreamHandles;
        private Dictionary<UInt32, GCHandle> OnTransactionHandles;
        private Dictionary<UInt32, GCHandle> OnStateUpdateHandles;
        private Dictionary<UInt32, GCHandle> OnPollUpdateHandles;
        private Dictionary<UInt32, GCHandle> OnConfigUpdateHandles;
        private Dictionary<UInt32, GCHandle> OnMatchmakingUpdateHandles;
        #endregion

    }
}
