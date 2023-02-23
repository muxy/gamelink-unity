using MuxyGameLink.Imports.Schema;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System;

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

        public struct MatchmakingUpdateResponse
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

        public struct PatchList
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

        public struct GetDropsResponse
        {
            public IntPtr Obj;
        }

        public struct Drop
        {
            public IntPtr Obj;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GameMetadata
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String GameName;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String GameLogo;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Theme;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GatewaySDK
        {
            public IntPtr SDK;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_AuthenticateResponse
        {
            public IntPtr JWT;
            public IntPtr RefreshToken;
            public IntPtr TwitchName;
            public UInt32 HasError;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_Payload
        {
            public IntPtr Bytes;
            public UInt64 Length;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_GameMetadata
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String GameName;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String GameLogo;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Theme;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_GameText
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Label;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Value;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Icon;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_Action
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String ID;

            public Int32 Category;
            public Int32 State;
            public Int32 Impact;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Name;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Description;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Icon;

            public Int32 Count;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_PollUpdate
        {
            public Int32 Winner;
            public Int32 WinningVoteCount;

            public IntPtr Results;
            public UInt64 ResultCount;

            public Int32 Count;
            public double Mean;
            public UInt32 IsFinal;
        }

        public delegate void GatewayPollUpdateDelegate(VoidPtr User, IntPtr Update);

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_PollConfiguration
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public string Prompt;

            public Int32 Location;
            public Int32 Mode;

            public IntPtr Options;
            public UInt64 OptionsCount;

            public Int32 Duration;

            public GatewayPollUpdateDelegate OnUpdate;
            public VoidPtr User;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_BitsUsed
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String TransactionID;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String SKU;

            public Int32 Bits;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String UserID;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Username;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MGW_ActionUsed
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String TransactionID;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String ActionID;

            public Int32 Cost;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String UserID;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public String Username;
        }
    }

    public class NativeString
    {
        public static String StringFromUTF8(StringPtr Ptr, int Length)
        {
            if (Ptr.Equals(StringPtr.Zero))
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

    public delegate void MatchmakingUpdateDelegate(VoidPtr UserData, Schema.MatchmakingUpdateResponse MatchmakingUpdate);

    public delegate void GetPollResponseDelegate(VoidPtr UserData, Schema.GetPollResponse PollResp);
    public delegate void PollUpdateResponseDelegate(VoidPtr UserData, Schema.PollUpdateResponse PollResp);

    public delegate void StateGetDelegate(VoidPtr UserData, Schema.StateResponse StateGet);
    public delegate void StateUpdateDelegate(VoidPtr UserData, Schema.StateUpdate StateUpdate);

    public delegate void ConfigGetDelegate(VoidPtr UserData, Schema.ConfigResponse ConfigGet);
    public delegate void ConfigUpdateDelegate(VoidPtr UserData, Schema.ConfigUpdate ConfigUpdate);

    public delegate void GetDropsResponseDelegate(VoidPtr UserData, Schema.GetDropsResponse Resp);

    public delegate void GatewayAuthenticateResponseDelegate(VoidPtr UserData, IntPtr Response);
    public delegate void GatewayForeachPayloadDelegate(VoidPtr UserData, IntPtr Payload);
    public delegate void GatewayDebugMessageDelegate(VoidPtr UserData, [MarshalAs(UnmanagedType.LPUTF8Str)] String Message);
    public delegate void GatewayOnBitsUsedDelegate(VoidPtr UserData, IntPtr BitsUsed);
    public delegate void GatewayOnActionUsedDelegate(VoidPtr UserData, IntPtr ActionUsed);


    public class Imported
    {
        // URL Derivation
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ProjectionWebsocketConnectionURL")]
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
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_AuthenticateWithGameIDAndPIN")]
        public static extern UInt16 AuthenticateWithGameIDAndPIN(SDKInstance GameLink,
                                                                   String ClientId, String GameId, String PIN,
                                                                   AuthenticateResponseDelegate Callback,
                                                                   VoidPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_AuthenticateWithGameIDAndRefreshToken")]
        public static extern UInt16 AuthenticateWithGameIDAndRefreshToken(SDKInstance GameLink,
                                                                   String ClientId, String GameId, String RefreshToken,
                                                                   AuthenticateResponseDelegate Callback,
                                                                   VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_IsAuthenticated")]
        public static extern bool IsAuthenticated(SDKInstance GameLink);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ReceiveMessage")]
        public static extern bool ReceiveMessage(SDKInstance GameLink, [MarshalAs(UnmanagedType.LPUTF8Str)] String Bytes, uint BytesLength);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ForeachPayload")]
        public static extern void ForeachPayload(SDKInstance GameLink, PayloadDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_HandleReconnect")]
        public static extern void HandleReconnect(SDKInstance SDK);

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
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetFirstError")]
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
        public static extern UInt16 SetState(SDKInstance GameLink, Int32 Target, String JsonString);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetState")]
        public static extern UInt16 GetState(SDKInstance GameLink, Int32 Target, StateGetDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_StateResponse_GetJson")]
        public static extern AllocatedStringPtr Schema_StateResponse_GetJson(Schema.StateResponse Resp);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToStateUpdates")]
        public static extern UInt16 SubscribeToStateUpdates(SDKInstance GameLink, Int32 Target);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeFromStateUpdates")]
        public static extern UInt16 UnsubscribeFromStateUpdates(SDKInstance GameLink, Int32 Target);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnStateUpdate")]
        public static extern UInt32 OnStateUpdate(SDKInstance GameLink, StateUpdateDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnStateUpdate")]
        public static extern void DetachOnStateUpdate(SDKInstance GameLink, UInt32 Id);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithInteger")]
        public static extern UInt16 UpdateStateWithInteger(SDKInstance GameLink, Int32 Target, Int32 Operation, String Path, Int64 Value);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithDouble")]
        public static extern UInt16 UpdateStateWithDouble(SDKInstance GameLink, Int32 Target, Int32 Operation, String Path, Double Value);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithString")]
        public static extern UInt16 UpdateStateWithString(SDKInstance GameLink, Int32 Target, Int32 Operation, String Path, String Value);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithLiteral")]
        public static extern UInt16 UpdateStateWithLiteral(SDKInstance GameLink, Int32 Target, Int32 Operation, String Path, String JsonLiteral);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithNull")]
        public static extern UInt16 UpdateStateWithNull(SDKInstance GameLink, Int32 Target, Int32 Operation, String Path);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateStateWithPatchList")]
        public static extern UInt16 UpdateStateWithPatchList(SDKInstance GameLink, Int32 Target, Schema.PatchList PList);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_StateUpdateResponse_GetTarget")]
        public static extern StringPtr Schema_StateUpdate_GetTarget(Schema.StateUpdate Object);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_StateUpdateResponse_GetJson")]
        public static extern AllocatedStringPtr Schema_StateUpdate_GetJson(Schema.StateUpdate Object);
        #endregion

        #region PatchList
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_Make")]
        public static extern Schema.PatchList PatchList_Make();
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_Kill")]
        public static extern void PatchList_Kill(Schema.PatchList PList);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithInteger")]
        public static extern void PatchList_UpdateStateWithInteger(Schema.PatchList PList, Int32 Operation, String Path, Int64 Val);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithDouble")]
        public static extern void PatchList_UpdateStateWithDouble(Schema.PatchList PList, Int32 Operation, String Path, double Val);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithBoolean")]
        public static extern void PatchList_UpdateStateWithBoolean(Schema.PatchList PList, Int32 Operation, String Path, bool Val);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithString")]
        public static extern void PatchList_UpdateStateWithString(Schema.PatchList PList, Int32 Operation, String Path, String Val);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithLiteral")]
        public static extern void PatchList_UpdateStateWithLiteral(Schema.PatchList PList, Int32 Operation, String Path, String Val);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithNull")]
        public static extern void PatchList_UpdateStateWithNull(Schema.PatchList PList, Int32 Operation, String Path);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithJson")]
        public static extern void PatchList_UpdateStateWithJson(Schema.PatchList PList, Int32 Operation, String Path, String Val);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_UpdateStateWithEmptyArray")]
        public static extern void PatchList_UpdateStateWithEmptyArray(Schema.PatchList PList, Int32 Operation, String Path);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_Empty")]
        public static extern bool PatchList_Empty(Schema.PatchList PList);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_PatchList_Clear")]
        public static extern void PatchList_Clear(Schema.PatchList PList);


        #endregion

        #region Config
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SetChannelConfig")]
        public static extern UInt16 SetChannelConfig(SDKInstance GameLink, String JsonString);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetConfig")]
        public static extern UInt16 GetConfig(SDKInstance GameLink, Int32 Target, ConfigGetDelegate Callback, VoidPtr UserData);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_ConfigResponse_GetConfigID")]
        public static extern StringPtr Schema_ConfigResponse_GetConfigID(Schema.ConfigResponse Response);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_ConfigResponse_GetJson")]
        public static extern AllocatedStringPtr Schema_ConfigResponse_GetJson(Schema.ConfigResponse Response);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithInteger")]
        public static extern UInt16 UpdateChannelConfigWithInteger(SDKInstance GameLink, Int32 Operation, String Path, Int64 Value);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithDouble")]
        public static extern UInt16 UpdateChannelConfigWithDouble(SDKInstance GameLink, Int32 Operation, String Path, double Value);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithString")]
        public static extern UInt16 UpdateChannelConfigWithString(SDKInstance GameLink, Int32 Operation, String Path, String Value);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithLiteral")]
        public static extern UInt16 UpdateChannelConfigWithLiteral(SDKInstance GameLink, Int32 Operation, String Path, String JsonLiteral);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UpdateChannelConfigWithNull")]
        public static extern UInt16 UpdateChannelConfigWithNull(SDKInstance GameLink, Int32 Operation, String Path);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToConfigurationChanges")]
        public static extern UInt16 SubscribeToConfigurationChanges(SDKInstance GameLink, Int32 Target);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeToConfigurationChanges")]
        public static extern UInt16 UnsubscribeFromConfigurationChanges(SDKInstance GameLink, Int32 Target);

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
        public static extern UInt16 SendBroadcast(SDKInstance GameLink, String Topic, String JsonString);

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
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Schema_Transaction_GetJson")]
        public static extern AllocatedStringPtr Schema_Transaction_GetJson(Schema.TransactionResponse TPBResp);

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
        public struct PollConfiguration
        {
            /// When userIdVoting is true, only users that have shared their ID will
            /// be able to vote.
            public bool userIdVoting;

            /// distinctOptionsPerUser controls how many options a user can vote for.
            /// Must be in the range [1, 258].
            public int distinctOptionsPerUser;

            /// totalVotesPerUser controls how many votes any user can cast.
            /// Must be in the range [1, 1024]
            public int totalVotesPerUser;

            /// votesPerOption controls how many votes per option a user can cast.
            /// Must be in the range [1, 1024]
            public int votesPerOption;

            /// When disabled is true, the poll will not accept any votes.
            public bool disabled;

            /// startsAt should be a unix timestamp, in seconds, at which point the poll
            /// will become enabled and will be able to be voted on.
            /// If no startAt time is desired, set this to 0.
            public Int64 startsAt;

            /// endsAt shoudl be a unix timestamp, in seconds, at which point the poll
            /// will become disabled.
            /// If no endsAt time is desired, set this to 0.
            public Int64 endsAt;
        };
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_CreatePoll")]
        public static extern RequestId CreatePoll(SDKInstance GameLink, String PollId, String Prompt, [In] String[] Options, UInt32 OptionsCount);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_CreatePollWithConfiguration")]
        public static extern RequestId CreatePollWithConfiguration(SDKInstance GameLink, String PollId, String Prompt, ref PollConfiguration Config, [In] String[] Options, UInt32 OptionsCount);
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

        #region Matchmaking
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SubscribeToMatchmakingQueueInvite")]
        public static extern UInt16 SubscribeToMatchmakingQueueInvite(SDKInstance GameLink);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_UnsubscribeFromMatchmakingQueueInvite")]
        public static extern UInt16 UnsubscribeFromMatchmakingQueueInvite(SDKInstance GameLink);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ClearMatchmakingQueue")]
        public static extern UInt16 ClearMatchmakingQueue(SDKInstance GameLink);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_RemoveMatchmakingEntry")]
        public static extern UInt16 RemoveMatchmakingEntry(SDKInstance GameLink, [MarshalAs(UnmanagedType.LPStr)] String Id);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_OnMatchmakingQueueInvite")]
        public static extern UInt32 OnMatchmakingQueueInvite(SDKInstance GameLink, MatchmakingUpdateDelegate Callback, VoidPtr UserData);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_DetachOnMatchmakingQueueInvite")]
        public static extern UInt16 DetachOnMatchmakingQueueInvite(SDKInstance GameLink, UInt32 Id);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_MatchmakingUpdate_GetData")]
        public static extern AllocatedStringPtr MatchmakingUpdate_GetData(Schema.MatchmakingUpdateResponse Resp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_MatchmakingUpdate_GetTwitchUsername")]
        public static extern StringPtr MatchmakingUpdate_GetTwitchUsername(Schema.MatchmakingUpdateResponse Resp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_MatchmakingUpdate_GetTwitchID")]
        public static extern StringPtr MatchmakingUpdate_GetTwitchID(Schema.MatchmakingUpdateResponse Resp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_MatchmakingUpdate_GetTimestamp")]
        public static extern Int64 MatchmakingUpdate_GetTimestamp(Schema.MatchmakingUpdateResponse Resp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_MatchmakingUpdate_GetIsFollower")]
        public static extern bool MatchmakingUpdate_IsFollower(Schema.MatchmakingUpdateResponse Resp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_MatchmakingUpdate_GetSubscriptionTier")]
        public static extern int MatchmakingUpdate_GetSubscriptionTier(Schema.MatchmakingUpdateResponse Resp);
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_MatchmakingUpdate_GetBitsSpent")]
        public static extern int MatchmakingUpdate_GetBitsSpent(Schema.MatchmakingUpdateResponse Resp);
        #endregion

        #region Metadata
        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_SetGameMetadata")]
        public static extern RequestId SetGameMetadata(SDKInstance SDK, GameMetadata Meta);
        #endregion

        #region Drops

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetDrops")]
        public static extern UInt16 GetDrops(SDKInstance SDK, String Status, GetDropsResponseDelegate Callback, VoidPtr User);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_ValidateDrop")]
        public static extern UInt16 ValidateDrop(SDKInstance SDK, String DropId);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetDropsResponse_GetAt")]
        public static extern Schema.Drop GetDropsResponse_GetAt(Schema.GetDropsResponse Resp, UInt64 Index);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_GetDropsResponse_GetCount")]
        public static extern UInt64 GetDropsResponse_GetCount(Schema.GetDropsResponse Resp);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Drop_GetId")]
        public static extern StringPtr Drop_GetId(Schema.Drop Drop);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Drop_GetBenefitId")]
        public static extern StringPtr Drop_GetBenefitId(Schema.Drop Drop);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Drop_GetUserId")]
        public static extern StringPtr Drop_GetUserId(Schema.Drop Drop);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Drop_GetStatus")]
        public static extern StringPtr Drop_GetStatus(Schema.Drop Drop);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Drop_GetService")]
        public static extern StringPtr Drop_GetService(Schema.Drop Drop);

        [DllImport("cgamelink.dll", EntryPoint = "MuxyGameLink_Drop_GetUpdatedAt")]
        public static extern StringPtr Drop_GetUpdatedAt(Schema.Drop Drop);

        #endregion

        #region Gateway
        [DllImport("cgamelink.dll", EntryPoint = "MGW_MakeSDK")]
        public static extern Schema.GatewaySDK MGW_MakeSDK([MarshalAs(UnmanagedType.LPUTF8Str)] String GameID);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_KillSDK")]
        public static extern void MGW_KillSDK(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_GetSandboxURL")]
        public static extern StringPtr MGW_SDK_GetSandboxURL(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_GetProductionURL")]
        public static extern StringPtr MGW_SDK_GetProductionURL(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_AuthenticateWithPIN")]
        public static extern RequestId MGW_SDK_AuthenticateWithPIN(Schema.GatewaySDK SDK, [MarshalAs(UnmanagedType.LPUTF8Str)] String PIN, GatewayAuthenticateResponseDelegate Delegate, VoidPtr User);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_AuthenticateWithRefreshToken")]
        public static extern RequestId MGW_SDK_AuthenticateWithRefreshToken(Schema.GatewaySDK SDK, [MarshalAs(UnmanagedType.LPUTF8Str)] String Refresh, GatewayAuthenticateResponseDelegate Delegate, VoidPtr User);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_ReceiveMessage")]
        public static extern bool MGW_SDK_ReceiveMessage(Schema.GatewaySDK SDK, byte[] Message, uint BytesLength);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_HasPayloads")]
        public static extern bool MGW_SDK_HasPayloads(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_HandleReconnect")]
        public static extern void MGW_SDK_HandleReconnect(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_OnDebugMessage")]
        public static extern void MGW_SDK_OnDebugMessage(Schema.GatewaySDK SDK, GatewayDebugMessageDelegate Callback, VoidPtr User);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_DetachOnDebugMessage")]
        public static extern void MGW_SDK_DetachOnDebugMessage(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_ForeachPayload")]
        public static extern void MGW_SDK_ForeachPayload(Schema.GatewaySDK SDK, GatewayForeachPayloadDelegate Delegate, VoidPtr User);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_IsAuthenticated")]
        public static extern bool MGW_SDK_IsAuthenticated(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_SetGameMetadata")]
        public static extern RequestId MGW_SDK_SetGameMetadata(Schema.GatewaySDK SDK, MGW_GameMetadata Meta);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_SetGameTexts")]
        public static extern void MGW_SDK_SetGameTexts(Schema.GatewaySDK SDK, MGW_GameText[] Texts, UInt64 Count);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_StartPoll")]
        public static extern void MGW_SDK_StartPoll(Schema.GatewaySDK SDK, MGW_PollConfiguration Config);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_StopPoll")]
        public static extern void MGW_SDK_StopPoll(Schema.GatewaySDK SDK);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_SetActions")]
        public static extern void MGW_SDK_SetActions(Schema.GatewaySDK SDK, MGW_Action[] Actions, UInt64 Count);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_EnableAction")]
        public static extern void MGW_SDK_EnableAction(Schema.GatewaySDK Gateway, [MarshalAs(UnmanagedType.LPUTF8Str)] String ID);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_DisableAction")]
        public static extern void MGW_SDK_DisableAction(Schema.GatewaySDK Gateway, [MarshalAs(UnmanagedType.LPUTF8Str)] String ID);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_SetActionCount")]
        public static extern void MGW_SDK_SetActionCount(Schema.GatewaySDK Gateway, [MarshalAs(UnmanagedType.LPUTF8Str)] String ID, Int32 count);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_OnBitsUsed")]
        public static extern void MGW_SDK_OnBitsUsed(Schema.GatewaySDK Gateway, GatewayOnBitsUsedDelegate Callback, VoidPtr User);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_OnActionUsed")]
        public static extern void MGW_SDK_OnActionUsed(Schema.GatewaySDK Gateway, GatewayOnActionUsedDelegate Callback, VoidPtr User);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_AcceptAction")]
        public static extern void MGW_SDK_AcceptAction(Schema.GatewaySDK Gateway, MGW_ActionUsed Coins, [MarshalAs(UnmanagedType.LPUTF8Str)] String Reason);

        [DllImport("cgamelink.dll", EntryPoint = "MGW_SDK_RefundAction")]
        public static extern void MGW_SDK_RefundAction(Schema.GatewaySDK Gateway, MGW_ActionUsed Coins, [MarshalAs(UnmanagedType.LPUTF8Str)] String Reason);
        #endregion
    }
}