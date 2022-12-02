using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using MuxyGameLink.Imports;

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
        /// <param name="GameId"> Your Twitch GameId </param>
        public SDK(String ClientId, String GameId)
        {
            this.Instance = Imported.Make();

            this.ClientId = ClientId;
            this.GameId = GameId;  

            OnDatastreamHandles = new Dictionary<UInt32, GCHandle>();
            OnTransactionHandles = new Dictionary<UInt32, GCHandle>();
            OnStateUpdateHandles = new Dictionary<UInt32, GCHandle>();
            OnPollUpdateHandles = new Dictionary<UInt32, GCHandle>();
            OnConfigUpdateHandles = new Dictionary<UInt32, GCHandle>();
            OnMatchmakingUpdateHandles = new Dictionary<UInt32, GCHandle>();
        }

        /// <summary> Constructs the SDK </summary>
        /// <param name="ClientId"> Your given Muxy ClientId </param>
        public SDK(String ClientId)
        {
            SDK(ClientId, "");
        }

        ~SDK()
        {
            Imported.Kill(this.Instance);
        }

        #region Authentication and User Management
        /// <summary> Check if we are currently authenticated </summary>
        /// <returns> Returns true if we are currently authenticated </returns>
        public bool IsAuthenticated()
        {
            return Imported.IsAuthenticated(this.Instance);
        }

        public delegate void AuthenticationCallback(AuthenticationResponse Payload);
        /// <summary> Authenticate with refresh token, which is obtained from initially authenticating with a PIN </summary>
        /// <param name="RefreshToken"> The refresh token obtained from calling AuthenticateWithPIN </param>
        /// <param name="Callback"> The callback to be called when the authentication attempt finishes </param>
        /// <returns> RequestId </returns>
        public UInt16 AuthenticateWithRefreshToken(string RefreshToken, AuthenticationCallback Callback)
        {
            GCHandle? Handle = null;
            AuthenticateResponseDelegate WrapperCallback = ((UserData, AuthResp) =>
            {
                AuthenticationResponse Response = new AuthenticationResponse(AuthResp);
                Callback(Response);
                Handle?.Free();
            });

            Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            return Imported.AuthenticateWithRefreshToken(this.Instance, this.ClientId, this.GameId, RefreshToken, WrapperCallback, IntPtr.Zero);
     
        }

        /// <summary> Authenticate with a PIN </summary>
        /// <param name="PIN"> User PIN </param>
        /// <param name="Callback"> The callback to be called when the authentication attempt finishes </param>
        /// <returns> RequestId </returns>
        public UInt16 AuthenticateWithPIN(string PIN, AuthenticationCallback Callback)
        {
            GCHandle? Handle = null;
            AuthenticateResponseDelegate WrapperCallback = ((UserData, AuthResp) =>
            {
                AuthenticationResponse Response = new AuthenticationResponse(AuthResp);
                Callback(Response);
                Handle?.Free();
            });

            Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            return Imported.AuthenticateWithPIN(this.Instance, this.ClientId, this.GameId, PIN, WrapperCallback, IntPtr.Zero);
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
            return Imported.ReceiveMessage(this.Instance, Message, (uint)Message.Length);
        }

        public delegate void PayloadCallback(string Payload);
        /// <summary> Calls given callback on each payload waiting to be sent, generally used to send the payload through a Websocket </summary>
        /// <param name="Callback"> Callback to be called on each iteration </param>
        public void ForEachPayload(PayloadCallback Callback)
        {
            PayloadDelegate WrapperCallback = ((UserData, Payload) =>
            {
                IntPtr ptr = Imported.Payload_GetData(Payload);
                UInt64 len = Imported.Payload_GetSize(Payload);

                string str = NativeString.StringFromUTF8(ptr, ((int)len));
                Callback(str);
            });

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            Imported.ForeachPayload(this.Instance, WrapperCallback, IntPtr.Zero);
            Handle.Free();
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
        /// <summary> Get target state </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Callback"> Callback to be called with state info</param>
        /// <returns> RequestId </returns>
        public UInt16 GetState(StateTarget Target, GetStateCallback Callback)
        {
            GCHandle? Handle = null;
            StateGetDelegate WrapperCallback = ((UserData, StateResp) =>
            {
                StateResponse Response = new StateResponse(StateResp);
                Callback(Response);
                Handle?.Free();
            });

            Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            return Imported.GetState(this.Instance, (Int32)Target, WrapperCallback, IntPtr.Zero);
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
        /// <summary> Sets callback for state updates, requires call to SubscribeToStateUpdates to begin receiving updates </summary>
        /// <param name="Callback"> Callback that will be called when an update occurs</param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnStateUpdate(UpdateStateCallback Callback)
        {
            StateUpdateDelegate WrapperCallback = ((UserData, Update) =>
            {
                StateUpdate Response = new StateUpdate(Update);
                Callback(Response);
            });

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            UInt32 Result = Imported.OnStateUpdate(this.Instance, WrapperCallback, IntPtr.Zero);

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

        /// <summary> Get config </summary>
        /// <param name="Target"> Either STATE_TARGET_CHANNEL or STATE_TARGET_EXTENSION </param>
        /// <param name="Callback"> Callback to be called to receive config data </param>
        /// <returns> RequestId </returns>
        public UInt16 GetConfig(ConfigTarget Target, GetConfigCallback Callback)
        {
            GCHandle? Handle = null;

            ConfigGetDelegate WrapperCallback = ((UserData, ConfigResp) =>
            {
                ConfigResponse Response = new ConfigResponse(ConfigResp);
                Callback(Response);
                Handle?.Free();
            });

            Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            return Imported.GetConfig(this.Instance, (Int32)Target, WrapperCallback, IntPtr.Zero);
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
        /// <summary> Sets callback for config updates, requires call to SubscribeToConfigurationChanges to begin receiving updates </summary>
        /// <param name="Callback"> Callback that will be called when an update occurs</param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnConfigUpdate(UpdateConfigCallback Callback)
        {
            ConfigUpdateDelegate WrapperCallback = ((UserData, Update) =>
            {
                ConfigUpdate Response = new ConfigUpdate(Update);
                Callback(Response);
            });

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            UInt32 Result = Imported.OnConfigUpdate(this.Instance, WrapperCallback, IntPtr.Zero);

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
        /// <summary> Sets callback for datastream, requires call to SubscribeToDatastream to begin receiving updates </summary>
        /// <param name="Callback"> Callback that will be called when an update occurs</param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnDatastream(DatastreamCallback Callback)
        {
            DatastreamUpdateDelegate WrapperCallback = ((IntPtr UserData, Imports.Schema.DatastreamUpdate Update) =>
            {
                DatastreamUpdate Response = new DatastreamUpdate(Update);
                Callback(Response);
            });

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            UInt32 Result = Imported.OnDatastream(this.Instance, WrapperCallback, IntPtr.Zero);

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
        /// <summary> Sets callback for Twitch Bit Purchasing, requires call to SubscribeToSKU or SubscribeToAllPurchases to begin receiving purchase updates </summary>
        /// <param name="Callback"> Callback that will be called when a purchase occurs </param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnTransaction(TransactionCallback Callback)
        {
            TransactionResponseDelegate WrapperCallback = ((IntPtr UserData, Imports.Schema.TransactionResponse Response) =>
            {
                Transaction Converted = new Transaction(Response);
                Callback(Converted);
            });

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            UInt32 Result = Imported.OnTransaction(this.Instance, WrapperCallback, IntPtr.Zero);

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
        /// <summary> Get all outstanding transactions that need validation </summary>
        /// <param name="SKU"> SKU to get outstanding transactions for </param>
        /// <param name="Callback"> Callback to receive transactions info </param>
        /// <returns> RequestId </returns>
        public UInt16 GetOutstandingTransactions(String SKU, GetOutstandingTransactionsCallback Callback)
        {
            GCHandle? Handle = null;
            GetOutstandingTransactionsDelegate WrapperCallback = ((IntPtr UserData, Imports.Schema.GetOutstandingTransactionsResponse Response) =>
            {
                OutstandingTransactions Converted = new OutstandingTransactions(Response);
                Callback(Converted);
                Handle?.Free();
            });
            Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            return Imported.GetOutstandingTransactions(this.Instance, SKU, WrapperCallback, IntPtr.Zero);
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
        /// <summary> Get Poll information once </summary>
        /// <param name="PollId"> Poll Id to delete, given from CreatePoll </param>
        /// <param name="Callback"> Callback to receive poll info </param>
        /// <returns> RequestId </returns>
        public UInt16 GetPoll(String PollId, GetPollCallback Callback)
        {
            GCHandle? Handle = null;
            GetPollResponseDelegate WrapperCallback = ((IntPtr UserData, Imports.Schema.GetPollResponse Response) =>
            {
                GetPollResponse Converted = new GetPollResponse(Response);
                Callback(Converted);
                Handle?.Free();
            });

            Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            return Imported.GetPoll(this.Instance, PollId, WrapperCallback, IntPtr.Zero);
        }

        public delegate void PollUpdateResponseCallback(PollUpdateResponse PResp);
        /// <summary> Sets callback for Poll Updates, requires call to SubscribeToPoll to begin receiving poll updates </summary>
        /// <param name="Callback"> Callback that will be called when a poll update occurs </param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnPollUpdate(PollUpdateResponseCallback Callback)
        {
            PollUpdateResponseDelegate WrapperCallback = ((IntPtr UserData, Imports.Schema.PollUpdateResponse PResp) =>
            {
                PollUpdateResponse Response = new PollUpdateResponse(PResp);
                Callback(Response);
            });

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            UInt32 Result = Imported.OnPollUpdate(this.Instance, WrapperCallback, IntPtr.Zero);

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
        public delegate void MatchmakingUpdateCallback(MatchmakingUpdate MResp);
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

        /// <summary> Sets callback for QueueInvites, requires call to SubscribeToMatchmakingQueueInvite to begin receiving queue updates </summary>
        /// <param name="Callback"> Callback that will be called when a queue invite occurs </param>
        /// <returns> Handle to reference callback later for things like detaching </returns>
        public UInt32 OnMatchmakingQueueInvite(MatchmakingUpdateCallback Callback)
        {
            MatchmakingUpdateDelegate WrapperCallback = ((IntPtr UserData, Imports.Schema.MatchmakingUpdateResponse MResp) =>
            {
                MatchmakingUpdate Response = new MatchmakingUpdate(MResp);
                Callback(Response);
            });

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            UInt32 Result = Imported.OnMatchmakingQueueInvite(this.Instance, WrapperCallback, IntPtr.Zero);

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
                return Imported.PatchList_Empty(this.Obj);
            }
            public void Clear()
            {
                Imported.PatchList_Clear(this.Obj);
            }

            public Imports.Schema.PatchList Obj;
        }

        #region Debugging
        public delegate void OnDebugMessageCallback(String Message);
        /// <summary> Detach callback for poll updates </summary>
        /// <param name="Handle"> Handle to detach </param>
        public void OnDebugMessage(OnDebugMessageCallback Callback)
        {
            DebugMessageDelegate WrapperCallback = ((IntPtr UserData, String Message) =>
            {
                Callback(Message);
            });

            OnDebugMessageHandle = GCHandle.Alloc(WrapperCallback, GCHandleType.Pinned);
            Imported.OnDebugMessage(this.Instance, WrapperCallback, IntPtr.Zero);
        }  
        /// <summary> Detach callback for debug messages </summary>
        /// <param name="Handle"> Handle to detach </param>
        public void DetachOnDebugMessage()
        {
            Imported.DetachOnDebugMessage(this.Instance);
            if (OnDebugMessageHandle != null)
            {
                OnDebugMessageHandle.Free();
            }
        }
        #endregion



        #region Members
        public String ClientId {get; set;}
        public String GameId { get; set; }

        private SDKInstance Instance;
        private User CachedUserInstance;

        private GCHandle OnDebugMessageHandle;
        private Dictionary<UInt32, GCHandle> OnDatastreamHandles;
        private Dictionary<UInt32, GCHandle> OnTransactionHandles;
        private Dictionary<UInt32, GCHandle> OnStateUpdateHandles;
        private Dictionary<UInt32, GCHandle> OnPollUpdateHandles;
        private Dictionary<UInt32, GCHandle> OnConfigUpdateHandles;
        private Dictionary<UInt32, GCHandle> OnMatchmakingUpdateHandles;
        #endregion

    }
}
