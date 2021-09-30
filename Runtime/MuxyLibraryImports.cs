using System;
using System.Runtime.InteropServices;

namespace MuxyGameLink.Imports
{
    // StringPtr should be made into MGL_String
    using StringPtr = System.IntPtr;

    // These pointers should be deallocated.
    using AllocatedStringPtr = System.IntPtr;

    // VoidPtr used for UserData, probably won't be used much in C#
    using VoidPtr = System.IntPtr;

    using RequestId = UInt16;

    public struct SDKInstance
    {
        IntPtr Instance;
    };

    public struct NativeError
    {
        IntPtr Obj;
    };

    public struct Payload
    {
        IntPtr Data;
    };

    namespace Schema
    {
        public struct User
        {
            public IntPtr Obj;
        };

        public struct AuthenticateResponse
        {
            public IntPtr Obj;
        };

        public struct DatastreamUpdate
        {
            public IntPtr Obj;
        };

        public struct DatastreamEvent
        {
            public IntPtr Obj;
        };

        public struct TransactionResponse
        {
            public IntPtr Obj;
        };

        public struct GetPollResponse
        {
            public IntPtr Obj;
        }

        public struct GetOutstandingTransactionsResponse
        {
            public IntPtr Obj;
        }

        public struct PollUpdateResponse
        {
            public IntPtr Obj;
        }

        public struct StateResponse
        {
            public IntPtr Obj;
        }

        public struct StateUpdate
        {
            public IntPtr Obj;
        }

        public struct ConfigResponse
        {
            public IntPtr Obj;
        }

        public struct ConfigUpdate
        {
            public IntPtr Obj;
        }
    }

    public class NativeString
    {
        public static String StringFromUTF8(StringPtr Ptr, int Length)
        {
            if (Ptr == null)
            {
                return String.Empty;
            }

            byte[] Copy = new byte[Length];
            Marshal.Copy(Ptr, Copy, 0, Length);

            return System.Text.Encoding.UTF8.GetString(Copy);
        }

        public static String StringFromUTF8(StringPtr Ptr)
        {
            UInt32 Length = Imported.StrLen(Ptr);
            return StringFromUTF8(Ptr, ((int)Length));
        }

        public static String StringFromUTF8AndDeallocate(AllocatedStringPtr Ptr)
        {
            String Result = StringFromUTF8(Ptr);
            Imported.FreeString(Ptr);
            return Result;
        }
    }

    public class NativeTimestamp
    {
        public static DateTime DateTimeFromMilliseconds(Int64 milliseconds)
        {
            TimeSpan Interval = TimeSpan.FromMilliseconds(milliseconds);
            return new DateTime(1970, 1, 1) + Interval;
        }
    }

    public delegate void AuthenticateResponseDelegate(VoidPtr UserData, Schema.AuthenticateResponse AuthResp);
    public delegate void PayloadDelegate(VoidPtr UserData, Payload Payload);
    public delegate void DatastreamUpdateDelegate(VoidPtr UserData, Schema.DatastreamUpdate DatastreamUpdate);
    public delegate void TransactionResponseDelegate(VoidPtr UserData, Schema.TransactionResponse TPBResp);
    public delegate void GetOutstandingTransactionsDelegate(VoidPtr UserData, Schema.GetOutstandingTransactionsResponse Resp);

    public delegate void DebugMessageDelegate(VoidPtr UserData, [MarshalAs(UnmanagedType.LPStr)] String Message);

    public delegate void GetPollResponseDelegate(VoidPtr UserData, Schema.GetPollResponse PollResp);
    public delegate void PollUpdateResponseDelegate(VoidPtr UserData, Schema.PollUpdateResponse PollResp);

    public delegate void StateGetDelegate(VoidPtr UserData, Schema.StateResponse StateGet);
    public delegate void StateUpdateDelegate(VoidPtr UserData, Schema.StateUpdate StateUpdate);

    public delegate void ConfigGetDelegate(VoidPtr UserData, Schema.ConfigResponse ConfigGet);
    public delegate void ConfigUpdateDelegate(VoidPtr UserData, Schema.ConfigUpdate ConfigUpdate);
    public class Imported
    {
        // URL Derivation
        [DllImport("cgamelink.dll", EntryPoint="MuxyGameLink_ProjectionWebsocketConnectionURL")]
        public static extern AllocatedStringPtr ProjectionWebsocketConnectionURL(
            [MarshalAs(UnmanagedType.LPStr)] String clientID, 
            Int32 stage,
            [MarshalAs(UnmanagedType.LPStr)] String projection, 
            int projectionMajor,
            int projectionMinor,
            int projectionPatch);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Strlen")]
        public static extern UInt32 StrLen(StringPtr Str);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_FreeString")]
        public static extern void FreeString(StringPtr Str);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Make")]
        public static extern SDKInstance Make();
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Kill")]
        public static extern void Kill(SDKInstance GameLink);

        #region Debugging
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnDebugMessage")]
        public static extern void OnDebugMessage(SDKInstance GameLink, DebugMessageDelegate Callback, VoidPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnDebugMessage")]
        public static extern void DetachOnDebugMessage(SDKInstance GameLink);
        #endregion

        #region Authentication
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_AuthenticateWithPIN")]
        public static extern UInt16 AuthenticateWithPIN(SDKInstance GameLink,
                                                                   String ClientId, String PIN,
                                                                   AuthenticateResponseDelegate Callback,
                                                                   VoidPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_AuthenticateWithRefreshToken")]
        public static extern UInt16 AuthenticateWithRefreshToken(SDKInstance GameLink,
                                                                   String ClientId, String RefreshToken,
                                                                   AuthenticateResponseDelegate Callback,
                                                                   VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_IsAuthenticated")]
        public static extern bool IsAuthenticated(SDKInstance GameLink);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ReceiveMessage")]
        public static extern bool ReceiveMessage(SDKInstance GameLink, [MarshalAs(UnmanagedType.LPStr)] String Bytes, uint BytesLength);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ForeachPayload")]
        public static extern void ForeachPayload(SDKInstance GameLink, PayloadDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Payload_GetData")]
        public static extern StringPtr Payload_GetData(Payload Payload);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Payload_GetSize")]
        public static extern UInt64 Payload_GetSize(Payload Payload);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetSchemaUser")]
        public static extern Schema.User GetSchemaUser(SDKInstance GameLink);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_User_GetJWT")]
        public static extern StringPtr Schema_User_GetJWT(Schema.User User);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_User_GetRefreshToken")]
        public static extern StringPtr Schema_User_GetRefreshToken(Schema.User User);
        #endregion

        #region Errors
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetFirstError")]
        public static extern NativeError Schema_GetFirstError(VoidPtr Resp);
        
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Error_IsValid")]
        public static extern bool Error_IsValid(NativeError Error);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Error_GetCode")]
        public static extern UInt32 Error_GetCode(NativeError Error);
        
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Error_GetTitle")]
        public static extern StringPtr Error_GetTitle(NativeError Error);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Error_GetDetail")]
        public static extern StringPtr Error_GetDetail(NativeError Error);
        #endregion

        #region State
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SetState")]
        public static extern UInt16 SetState(SDKInstance GameLink, String Target, String JsonString);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetState")]
        public static extern UInt16 GetState(SDKInstance GameLink, String Target, StateGetDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_StateResponse_GetJson")]
        public static extern AllocatedStringPtr Schema_StateResponse_GetJson(Schema.StateResponse Resp);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToStateUpdates")]
        public static extern UInt16 SubscribeToStateUpdates(SDKInstance GameLink, String Target);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeFromStateUpdates")]
        public static extern UInt16 UnsubscribeFromStateUpdates(SDKInstance GameLink, String target);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnStateUpdate")]
        public static extern UInt32 OnStateUpdate(SDKInstance GameLink, StateUpdateDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnStateUpdate")]
        public static extern void DetachOnStateUpdate(SDKInstance GameLink, UInt32 Id);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithInteger")]
        public static extern UInt16 UpdateStateWithInteger(SDKInstance GameLink, String Target, String Operation, String Path, Int64 Value);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithDouble")]
        public static extern UInt16 UpdateStateWithDouble(SDKInstance GameLink, String Target, String Operation, String Path, Double Value);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithString")]
        public static extern UInt16 UpdateStateWithString(SDKInstance GameLink, String Target, String Operation, String Path, String Value);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithLiteral")]
        public static extern UInt16 UpdateStateWithLiteral(SDKInstance GameLink, String Target, String Operation, String Path, String JsonLiteral);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithNull")]
        public static extern UInt16 UpdateStateWithNull(SDKInstance GameLink, String Target, String Operation, String Path);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_StateUpdateResponse_GetTarget")]
        public static extern StringPtr Schema_StateUpdate_GetTarget(Schema.StateUpdate Object);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_StateUpdateResponse_GetJson")]
        public static extern AllocatedStringPtr Schema_StateUpdate_GetJson(Schema.StateUpdate Object);
        #endregion

        #region Config
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SetChannelConfig")]
        public static extern UInt16 SetChannelConfig(SDKInstance GameLink, String JsonString);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetConfig")]
        public static extern UInt16 GetConfig(SDKInstance GameLink, String Target, ConfigGetDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_ConfigResponse_GetConfigID")]
        public static extern StringPtr Schema_ConfigResponse_GetConfigID(Schema.ConfigResponse Response);
        
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_ConfigResponse_GetJson")]
        public static extern AllocatedStringPtr Schema_ConfigResponse_GetJson(Schema.ConfigResponse Response);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithInteger")]
        public static extern UInt16 UpdateChannelConfigWithInteger(SDKInstance GameLink, String Operation, String Path, Int64 Value);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithDouble")]
        public static extern UInt16 UpdateChannelConfigWithDouble(SDKInstance GameLink, String Operation, String Path, double Value);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithString")]
        public static extern UInt16 UpdateChannelConfigWithString(SDKInstance GameLink, String Operation, String Path, String Value);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithLiteral")]
        public static extern UInt16 UpdateChannelConfigWithLiteral(SDKInstance GameLink, String Operation, String Path, String JsonLiteral);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithNull")]
        public static extern UInt16 UpdateChannelConfigWithNull(SDKInstance GameLink, String Operation, String Path);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToConfigurationChanges")]
        public static extern UInt16 SubscribeToConfigurationChanges(SDKInstance GameLink, String Target);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeToConfigurationChanges")]
        public static extern UInt16 UnsubscribeFromConfigurationChanges(SDKInstance GameLink, String Target);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_ConfigUpdateResponse_GetConfigID")]
        public static extern StringPtr Schema_ConfigUpdateResponse_GetConfigID(Schema.ConfigUpdate Update);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_ConfigUpdateResponse_GetJson")]
        public static extern AllocatedStringPtr Schema_ConfigUpdateResponse_GetJson(Schema.ConfigUpdate Update);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnConfigUpdate")]
        public static extern UInt32 OnConfigUpdate(SDKInstance GameLink, ConfigUpdateDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnConfigUpdate")]
        public static extern void DetachOnConfigUpdate(SDKInstance GameLink, UInt32 Id);
        #endregion

        #region Datastream
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SendBroadcast")]
        public static extern UInt16 SendBroadcast(SDKInstance GameLink, String Target, String JsonString);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToDatastream")]
        public static extern UInt16 SubscribeToDatastream(SDKInstance GameLink);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeFromDatastream")]
        public static extern UInt16 UnsubscribeFromDatastream(SDKInstance GameLink);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnDatastream")]
        public static extern UInt32 OnDatastream(SDKInstance GameLink, DatastreamUpdateDelegate Callback, VoidPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnDatastream")]
        public static extern void DetachOnDatastream(SDKInstance GameLink, UInt32 OnDatastreamHandle);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_DatastreamUpdate_GetEventCount")]
        public static extern UInt32 Schema_DatastreamUpdate_GetEventCount(Schema.DatastreamUpdate DatastreamUpdate);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_DatastreamUpdate_GetEventAt")]
        public static extern Schema.DatastreamEvent Schema_DatastreamUpdate_GetEventAt(Schema.DatastreamUpdate DatastreamUpdate, UInt32 AtIndex);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_DatastreamEvent_GetJson")]
        public static extern AllocatedStringPtr Schema_DatastreamEvent_GetJson(Schema.DatastreamEvent DatastreamEvent);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_DatastreamEvent_GetTimestamp")]
        public static extern Int64 Schema_DatastreamEvent_GetTimestamp(Schema.DatastreamEvent DatastreamEvent);
        #endregion

        #region Transactions
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToSKU")]
        public static extern UInt16 SubscribeToSKU(SDKInstance GameLink, String SKU);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeFromSKU")]
        public static extern UInt16 UnsubscribeFromSKU(SDKInstance GameLink, String SKU);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToAllPurchases")]
        public static extern UInt16 SubscribeToAllPurchases(SDKInstance GameLink);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeFromAllPurchases")]
        public static extern UInt16 UnsubscribeFromAllPurchases(SDKInstance GameLink);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnTransaction")]
        public static extern UInt32 OnTransaction(SDKInstance GameLink, TransactionResponseDelegate Callback, IntPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnTransaction")]
        public static extern void DetachOnTransaction(SDKInstance GameLink, UInt32 id);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetId")]
        public static extern StringPtr Schema_Transaction_GetId(Schema.TransactionResponse TPBResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetSKU")]
        public static extern StringPtr Schema_Transaction_GetSKU(Schema.TransactionResponse TPBResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetDisplayName")]
        public static extern StringPtr Schema_Transaction_GetDisplayName(Schema.TransactionResponse TPBResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetUserId")]
        public static extern StringPtr Schema_Transaction_GetUserId(Schema.TransactionResponse TPBResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetUserName")]
        public static extern StringPtr Schema_Transaction_GetUserName(Schema.TransactionResponse TPBResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetCost")]
        public static extern Int32 Schema_Transaction_GetCost(Schema.TransactionResponse TPBResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetTimestamp")]
        public static extern Int64 Schema_Transaction_GetTimestamp(Schema.TransactionResponse TPBResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetAdditionalJson")]
        public static extern AllocatedStringPtr Schema_Transaction_GetAdditionalJson(Schema.TransactionResponse TPBResp);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetOutstandingTransactions")]
        public static extern RequestId GetOutstandingTransactions(SDKInstance GameLink, String SKU, GetOutstandingTransactionsDelegate Resp, IntPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetOutstandingTransactionsResponse_GetTransactionCount")]
        public static extern UInt32 Schema_GetOutstandingTransactionsResponse_GetTransactionCount(Schema.GetOutstandingTransactionsResponse Resp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetOutstandingTransactionsResponse_GetTransactionAt")]
        public static extern Schema.TransactionResponse Schema_GetOutstandingTransactionsResponse_GetTransactionAt(Schema.GetOutstandingTransactionsResponse Resp, UInt32 Index);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_RefundTransactionBySKU")]
        public static extern RequestId RefundTransactionBySKU(SDKInstance GameLink, String SKU, String UserId);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_RefundTransactionByID")]
        public static extern RequestId RefundTransactionByID(SDKInstance GameLink, String TxId, String UserId);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ValidateTransaction")]
        public static extern RequestId ValidateTransaction(SDKInstance GameLink, String TxId, String Details);
        #endregion

        #region Polling 
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_CreatePoll")]
        public static extern RequestId CreatePoll(SDKInstance GameLink, String PollId, String Prompt, [In] String[] Options, UInt32 OptionsCount);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToPoll")]
        public static extern RequestId SubscribeToPoll(SDKInstance GameLink, String PollId);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeFromPoll")]
        public static extern RequestId UnsubscribeFromPoll(SDKInstance GameLink, String PollId);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DeletePoll")]
        public static extern RequestId DeletePoll(SDKInstance GameLink, String PollId);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetPoll")]
        public static extern RequestId GetPoll(SDKInstance GameLink, String PollId, GetPollResponseDelegate Callback, VoidPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnPollUpdate")]
        public static extern UInt32 OnPollUpdate(SDKInstance GameLink, PollUpdateResponseDelegate Callback, VoidPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnPollUpdate")]
        public static extern UInt32 DetachOnPollUpdate(SDKInstance GameLink, UInt32 Id);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetPollId")]
        public static extern StringPtr Schema_GetPollResponse_GetPollId(Schema.GetPollResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetPrompt")]
        public static extern StringPtr Schema_GetPollResponse_GetPrompt(Schema.GetPollResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetOptionCount")]
        public static extern UInt32 Schema_GetPollResponse_GetOptionCount(Schema.GetPollResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetOptionAt")]
        public static extern StringPtr Schema_GetPollResponse_GetOptionAt(Schema.GetPollResponse PResp, UInt32 Index);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetResultCount")]
        public static extern UInt32 Schema_GetPollResponse_GetResultCount(Schema.GetPollResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetResultAt")]
        public static extern Int32 Schema_GetPollResponse_GetResultAt(Schema.GetPollResponse PResp, UInt32 Index);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetMean")]
        public static extern double Schema_GetPollResponse_GetMean(Schema.GetPollResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetSum")]
        public static extern double Schema_GetPollResponse_GetSum(Schema.GetPollResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_GetPollResponse_GetCount")]
        public static extern Int32 Schema_GetPollResponse_GetCount(Schema.GetPollResponse PResp);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetPollId")]
        public static extern StringPtr Schema_PollUpdateResponse_GetPollId(Schema.PollUpdateResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetPrompt")]
        public static extern StringPtr Schema_PollUpdateResponse_GetPrompt(Schema.PollUpdateResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetOptionCount")]
        public static extern UInt32 Schema_PollUpdateResponse_GetOptionCount(Schema.PollUpdateResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetOptionAt")]
        public static extern StringPtr Schema_PollUpdateResponse_GetOptionAt(Schema.PollUpdateResponse PResp, UInt32 Index);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetResultCount")]
        public static extern UInt32 Schema_PollUpdateResponse_GetResultCount(Schema.PollUpdateResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetResultAt")]
        public static extern Int32 Schema_PollUpdateResponse_GetResultAt(Schema.PollUpdateResponse PResp, UInt32 Index);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetMean")]
        public static extern double Schema_PollUpdateResponse_GetMean(Schema.PollUpdateResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetSum")]
        public static extern double Schema_PollUpdateResponse_GetSum(Schema.PollUpdateResponse PResp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_PollUpdateResponse_GetCount")]
        public static extern Int32 Schema_PollUpdateResponse_GetCount(Schema.PollUpdateResponse PResp);
        #endregion
    }
}