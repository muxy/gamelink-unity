using System;
using System.Runtime.InteropServices;
using MuxyGameLink.Imports;

namespace MuxyGameLink
{
    public enum Stage
    {
        Production = 0, 
        Sandbox = 1
    };

    public class Error
    {
        public Error(NativeError err)
        {
            this.Object = err;
        }

        public string Title
        {
            get
            {
                if (CachedTitle == null)
                {
                    CachedTitle = NativeString.StringFromUTF8(Imported.Error_GetTitle(this.Object));
                }
                return CachedTitle;
            }
        }

        public string Detail
        {
            get
            {
                if (CachedDetail == null)
                {
                    CachedDetail = NativeString.StringFromUTF8(Imported.Error_GetDetail(this.Object));
                }

                return CachedDetail;
            }
        }

        public override string ToString()
        {
            return String.Format("Muxy GameLink Error: {0} ({1})", Title, Detail);
        }

        private NativeError Object;
        private string CachedTitle;
        private string CachedDetail;
    }

    public class User
    {
        public User(Imports.Schema.User User)
        {
            this.Object = User;
        }

        public string RefreshToken
        {
            get
            {
                if (CachedRefreshToken == null)
                {
                    CachedRefreshToken = NativeString.StringFromUTF8(Imported.Schema_User_GetRefreshToken(this.Object));
                }

                return CachedRefreshToken;
            }
        }

        private Imports.Schema.User Object;
        private string CachedRefreshToken;
    }

    public class AuthenticationResponse
    {
        public AuthenticationResponse(Imports.Schema.AuthenticateResponse resp)
        {
            this.Object = resp;
        }

        /// <summary> Gets First Error for AuthenticationResponse </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }

        private Imports.Schema.AuthenticateResponse Object;
    }

    public class DatastreamUpdate
    {
        public class Event
        {
            public Event(Imports.Schema.DatastreamEvent Obj)
            {
                this.Object = Obj;
            } 

            public Int64 Timestamp
            {
                get
                {
                    return Imported.Schema_DatastreamEvent_GetTimestamp(this.Object);
                }
            }

            public String Json
            {
                get
                {
                    if (CachedJson == null)
                    {
                        CachedJson = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_DatastreamEvent_GetJson(this.Object));
                    }

                    return CachedJson;
                }
            }

            private String CachedJson;
            private Imports.Schema.DatastreamEvent Object;
        }

        public DatastreamUpdate(Imports.Schema.DatastreamUpdate Obj)
        {
            this.Object = Obj;
        }
        /// <summary> Gets First Error for DatastreamUpdate </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }

        public Event At(UInt32 Index)
        {
            return new Event(Imported.Schema_DatastreamUpdate_GetEventAt(this.Object, Index));
        }

        public UInt32 Count
        {
            get
            {
                return Imported.Schema_DatastreamUpdate_GetEventCount(this.Object);
            }
        }

        private Imports.Schema.DatastreamUpdate Object;
    }

    public class Transaction
    {
        public Transaction(Imports.Schema.TransactionResponse Obj)
        {
            this.Object = Obj;
        }
        /// <summary> Gets First Error for Transaction </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }

        public string Id
        {
            get
            {
                if (CachedId == null)
                {
                    CachedId = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetId(this.Object));
                }

                return CachedId;
            }
        }
        private string CachedId;

        public string SKU
        {
            get
            {
                if (CachedSKU == null)
                {
                    CachedSKU = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetSKU(this.Object));
                }

                return CachedSKU;
            }
        }
        private string CachedSKU;

        public string DisplayName
        {
            get
            {
                if (CachedDisplayName == null)
                {
                    CachedDisplayName = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetDisplayName(this.Object));
                }

                return CachedDisplayName;
            }
        }
        private string CachedDisplayName;

        public string UserId
        {
            get
            {
                if (CachedUserId == null)
                {
                    CachedUserId = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetUserId(this.Object));
                }

                return CachedUserId;
            }
        }
        private string CachedUserId;

        public string UserName
        {
            get
            {
                if (CachedUserName == null)
                {
                    CachedUserName = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetUserName(this.Object));
                }

                return CachedUserName;
            }
        }
        private string CachedUserName;

        public Int32 Cost
        {
            get
            {
                return Imported.Schema_Transaction_GetCost(this.Object);
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return NativeTimestamp.DateTimeFromMilliseconds(Imported.Schema_Transaction_GetTimestamp(this.Object));
            }
        }

        public string AdditionalJson
        {
            get
            {
                if (CachedAdditionalJson == null)
                {
                    CachedAdditionalJson = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_Transaction_GetAdditionalJson(this.Object));
                }

                return CachedAdditionalJson;
            }
        }
        private string CachedAdditionalJson;

        private Imports.Schema.TransactionResponse Object;
    }

    public class OutstandingTransactions
    {
        public OutstandingTransactions(Imports.Schema.GetOutstandingTransactionsResponse Obj)
        {
            this.Object = Obj;
        }
        /// <summary> Gets First Error for GetOutstandingTransactionsResponse </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }

        public UInt32 Count
        {
            get
            {
                return Imported.Schema_GetOutstandingTransactionsResponse_GetTransactionCount(this.Object);
            }
        }

        public Transaction At(UInt32 Index)
        {
            Transaction Trans = new Transaction(Imported.Schema_GetOutstandingTransactionsResponse_GetTransactionAt(this.Object, Index));
            return Trans;
        }

        private Imports.Schema.GetOutstandingTransactionsResponse Object;
    }

    public class StateResponse
    {
        public StateResponse(Imports.Schema.StateResponse Obj)
        {
            this.Object = Obj;
        }
        /// <summary> Gets First Error for StateResponse </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }
        public String Json
        {
            get
            {
                if (CachedJson == null)
                {
                    CachedJson = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_StateResponse_GetJson(this.Object));
                }
                return CachedJson;
            }
        }
        private String CachedJson;

        private Imports.Schema.StateResponse Object;
    }

    public class StateUpdate
    {
        public StateUpdate(Imports.Schema.StateUpdate Obj)
        {
            this.Object = Obj;
        }
        /// <summary> Gets First Error for StateUpdate </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }
        public String Target
        {
            get
            {
                if (CachedTarget == null)
                {
                    CachedTarget = NativeString.StringFromUTF8(Imported.Schema_StateUpdate_GetTarget(this.Object));
                }
                return CachedTarget;
            }
        }
        private String CachedTarget;

        public String Json
        {
            get
            {
                if (CachedJson == null)
                {
                    CachedJson = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_StateUpdate_GetJson(this.Object));
                }
                return CachedJson;
            }
        }
        private String CachedJson;

        private Imports.Schema.StateUpdate Object;
    }

    public class ConfigResponse
    {
        public ConfigResponse(Imports.Schema.ConfigResponse Obj)
        {
            this.Object = Obj;
        }

        /// <summary> Gets First Error for ConfigResponse </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }

        public String ConfigId
        {
            get
            {
                if (CachedConfigId == null)
                {
                    CachedConfigId = NativeString.StringFromUTF8(Imported.Schema_ConfigResponse_GetConfigID(this.Object));
                }
                return CachedConfigId;
            }
        }
        private String CachedConfigId;

        public String Json
        {
            get
            {
                if (CachedJson == null)
                {
                    CachedJson = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_ConfigResponse_GetJson(this.Object));
                }
                return CachedJson;
            }
        }
        private String CachedJson;

        private Imports.Schema.ConfigResponse Object;
    }

    public class ConfigUpdate
    {
        public ConfigUpdate(Imports.Schema.ConfigUpdate Obj)
        {
            this.Object = Obj;
        }

        /// <summary> Gets First Error for ConfigUpdate </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }

        public String ConfigId
        {
            get
            {
                if (CachedConfigId == null)
                {
                    CachedConfigId = NativeString.StringFromUTF8(Imported.Schema_ConfigUpdateResponse_GetConfigID(this.Object));
                }
                return CachedConfigId;
            }
        }
        private String CachedConfigId;

        public String Json
        {
            get
            {
                if (CachedJson == null)
                {
                    CachedJson = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_ConfigUpdateResponse_GetJson(this.Object));
                }
                return CachedJson;
            }
        }
        private String CachedJson;


        private Imports.Schema.ConfigUpdate Object;
    }

    public class GetPollResponse
    {
        public GetPollResponse(Imports.Schema.GetPollResponse Obj)
        {
            this.Object = Obj;
        }
        /// <summary> Gets First Error for GetPollResponse </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }

        public string PollId
        {
            get
            {
                if (CachedPollId == null)
                {
                    CachedPollId = NativeString.StringFromUTF8(Imported.Schema_GetPollResponse_GetPollId(this.Object));
                }

                return CachedPollId;
            }
        }
        private string CachedPollId;

        public string Prompt
        {
            get
            {
                if (CachedPrompt == null)
                {
                    CachedPrompt = NativeString.StringFromUTF8(Imported.Schema_GetPollResponse_GetPrompt(this.Object));
                }

                return CachedPrompt;
            }
        }
        private string CachedPrompt;

        public UInt32 OptionCount
        {
            get
            {
                return Imported.Schema_GetPollResponse_GetOptionCount(this.Object);
            }
        }

        public String OptionAt(UInt32 Index)
        {
            return NativeString.StringFromUTF8(Imported.Schema_GetPollResponse_GetOptionAt(this.Object, Index));
        }

        public UInt32 ResultCount
        {
            get
            {
                return Imported.Schema_GetPollResponse_GetResultCount(this.Object);
            }
        }

        public Int32 ResultAt(UInt32 Index)
        {
            return Imported.Schema_GetPollResponse_GetResultAt(this.Object, Index);
        }

        public double Mean
        {
            get
            {
                return Imported.Schema_GetPollResponse_GetMean(this.Object);
            }
        }

        public double Sum
        {
            get
            {
                return Imported.Schema_GetPollResponse_GetSum(this.Object);
            }
        }

        public double Count
        {
            get
            {
                return Imported.Schema_GetPollResponse_GetCount(this.Object);
            }
        }

        private Imports.Schema.GetPollResponse Object;
    }

    public class PollUpdateResponse
    {
        public PollUpdateResponse(Imports.Schema.PollUpdateResponse Obj)
        {
            this.Object = Obj;
        }
        /// <summary> Gets First Error for PollUpdateResponse </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        public Error GetFirstError()
        {
            NativeError Err = Imported.Schema_GetFirstError(this.Object.Obj);
            if (!Imported.Error_IsValid(Err))
            {
                return null;
            }

            return new Error(Err);
        }
        public string PollId
        {
            get
            {
                if (CachedPollId == null)
                {
                    CachedPollId = NativeString.StringFromUTF8(Imported.Schema_PollUpdateResponse_GetPollId(this.Object));
                }

                return CachedPollId;
            }
        }
        private string CachedPollId;

        public string Prompt
        {
            get
            {
                if (CachedPrompt == null)
                {
                    CachedPrompt = NativeString.StringFromUTF8(Imported.Schema_PollUpdateResponse_GetPrompt(this.Object));
                }

                return CachedPrompt;
            }
        }
        private string CachedPrompt;

        public UInt32 OptionCount
        {
            get
            {
                return Imported.Schema_PollUpdateResponse_GetOptionCount(this.Object);
            }
        }

        public String OptionAt(UInt32 Index)
        {
            return NativeString.StringFromUTF8(Imported.Schema_PollUpdateResponse_GetOptionAt(this.Object, Index));
        }

        public UInt32 ResultCount
        {
            get
            {
                return Imported.Schema_PollUpdateResponse_GetResultCount(this.Object);
            }
        }

        public Int32 ResultAt(UInt32 Index)
        {
            return Imported.Schema_PollUpdateResponse_GetResultAt(this.Object, Index);
        }

        public double Mean
        {
            get
            {
                return Imported.Schema_PollUpdateResponse_GetMean(this.Object);
            }
        }

        public double Sum
        {
            get
            {
                return Imported.Schema_PollUpdateResponse_GetSum(this.Object);
            }
        }

        public double Count
        {
            get
            {
                return Imported.Schema_PollUpdateResponse_GetCount(this.Object);
            }
        }

        private Imports.Schema.PollUpdateResponse Object;
    }
}