using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
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
        public Error(NativeError Obj)
        {
            this.Title = NativeString.StringFromUTF8(Imported.Error_GetTitle(Obj));
            this.Detail = NativeString.StringFromUTF8(Imported.Error_GetDetail(Obj));
        }

        public String Title { get; private set; }
        public string Detail { get; private set; }

        public override string ToString()
        {
            return String.Format("Muxy GameLink Error: {0} ({1})", Title, Detail);
        }
    }

    public class User
    {
        public User(Imports.Schema.User User)
        {
            this.RefreshToken = NativeString.StringFromUTF8(Imported.Schema_User_GetRefreshToken(User));
        }

        public String RefreshToken { get; private set; }
    }

    public class HasError
    {
        public HasError(System.IntPtr ObjectPointer)
        {
            NativeError Err = Imported.Schema_GetFirstError(ObjectPointer);
            if (Imported.Error_IsValid(Err))
            {
                this._FirstError = new Error(Err);
                return;
            }
        }

        /// <summary> Gets First Error for any response </summary>
        /// <returns> NULL if there is no error, otherwise error information </returns>
        private Error _FirstError = null;
        public Error GetFirstError()
        {
            return _FirstError;
        }
    };

    public class AuthenticationResponse : HasError
    {
        public AuthenticationResponse(Imports.Schema.AuthenticateResponse Obj) 
            : base(Obj.Obj)
        { }
    }

    public class DatastreamUpdate : HasError
    {
        public class Event
        {
            public Event(Imports.Schema.DatastreamEvent Obj)
            {
                this.Json = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_DatastreamEvent_GetJson(Obj));
                this.Timestamp = Imported.Schema_DatastreamEvent_GetTimestamp(Obj);
            }

            public Int64 Timestamp { get; private set; }
            public String Json { get; private set; }
        }

        public DatastreamUpdate(Imports.Schema.DatastreamUpdate Obj)
            : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            Events = new List<Event>();
            for (UInt32 i = 0; i < Imported.Schema_DatastreamUpdate_GetEventCount(Obj); i++)
            {
                Events.Add(new Event(Imported.Schema_DatastreamUpdate_GetEventAt(Obj, i)));
            }
        }

        public List<Event> Events { get; private set; }
    }

    public class Transaction : HasError
    {
        public Transaction(Imports.Schema.TransactionResponse Obj)
            : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            this.Id = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetId(Obj));
            this.SKU = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetSKU(Obj));
            this.DisplayName = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetDisplayName(Obj));
            this.UserId = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetUserId(Obj));
            this.UserName = NativeString.StringFromUTF8(Imported.Schema_Transaction_GetUserName(Obj));
            this.Cost = Imported.Schema_Transaction_GetCost(Obj);
            this.Timestamp = NativeTimestamp.DateTimeFromMilliseconds(Imported.Schema_Transaction_GetTimestamp(Obj));
            this.Json = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_Transaction_GetAdditionalJson(Obj));
        }

        public String Id { get; private set; }
        public String SKU { get; private set; }
        public String DisplayName { get; private set; }
        public String UserId { get; private set; }
        public String UserName { get; private set; }
        public Int32 Cost { get; private set; }
        public DateTime Timestamp { get; private set; }
        public String Json { get; private set; }
    }

    public class OutstandingTransactions : HasError
    {
        public OutstandingTransactions(Imports.Schema.GetOutstandingTransactionsResponse Obj)
            : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }


            Transactions = new List<Transaction>();
            for (UInt32 i = 0; i < Imported.Schema_GetOutstandingTransactionsResponse_GetTransactionCount(Obj); i++)
            {
                Transactions.Add(new Transaction(Imported.Schema_GetOutstandingTransactionsResponse_GetTransactionAt(Obj, i)));
            }

        }

        public List<Transaction> Transactions { get; private set; }
    }

    public class StateResponse : HasError
    {
        public StateResponse(Imports.Schema.StateResponse Obj)
            : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            this.Json = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_StateResponse_GetJson(Obj));
        }
      
        public String Json { get; private set; }
    }

    public class StateUpdate : HasError
    {
        public StateUpdate(Imports.Schema.StateUpdate Obj)
             : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            this.Target = NativeString.StringFromUTF8(Imported.Schema_StateUpdate_GetTarget(Obj));
            this.Json = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_StateUpdate_GetJson(Obj));
        }

        public String Target { get; private set; }
        public String Json { get; private set; }
    }

    public class ConfigResponse : HasError
    {
        public ConfigResponse(Imports.Schema.ConfigResponse Obj)
             : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            this.ConfigId = NativeString.StringFromUTF8(Imported.Schema_ConfigResponse_GetConfigID(Obj));
            this.Json = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_ConfigResponse_GetJson(Obj));
        }

        public String ConfigId { get; private set; }
        public String Json { get; private set; }
    }

    public class ConfigUpdate : HasError
    {
        public ConfigUpdate(Imports.Schema.ConfigUpdate Obj)
            : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            this.ConfigId = NativeString.StringFromUTF8(Imported.Schema_ConfigUpdateResponse_GetConfigID(Obj));
            this.Json = NativeString.StringFromUTF8AndDeallocate(Imported.Schema_ConfigUpdateResponse_GetJson(Obj));
        }
        
        public String ConfigId { get; private set; }
        public String Json { get; private set; }
    }

    public class GetPollResponse : HasError
    {
        public GetPollResponse(Imports.Schema.GetPollResponse Obj)
            : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            this.PollId = NativeString.StringFromUTF8(Imported.Schema_GetPollResponse_GetPollId(Obj));
            this.Prompt = NativeString.StringFromUTF8(Imported.Schema_GetPollResponse_GetPrompt(Obj));
            this.Mean = Imported.Schema_GetPollResponse_GetMean(Obj);
            this.Sum = Imported.Schema_GetPollResponse_GetSum(Obj);
            this.Count = Imported.Schema_GetPollResponse_GetCount(Obj);

            Options = new List<string>();
            Results = new List<Int32>();
            for (UInt32 i = 0; i < Imported.Schema_GetPollResponse_GetOptionCount(Obj); i++)
            {
                Options.Add(NativeString.StringFromUTF8(Imported.Schema_GetPollResponse_GetOptionAt(Obj, i)));
            }
            for (UInt32 i = 0; i < Imported.Schema_GetPollResponse_GetResultCount(Obj); i++)
            {
                Results.Add(Imported.Schema_GetPollResponse_GetResultAt(Obj, i));
            }
        }

        public String PollId { get; private set; }
        public String Prompt { get; private set; }
        public double Mean { get; private set; }
        public double Sum { get; private set; }
        public Int32 Count { get; private set; }
        public List<String> Options { get; private set; }
        public List<Int32> Results { get; private set; }
    }

    public class PollUpdateResponse : HasError
    {
        public PollUpdateResponse(Imports.Schema.PollUpdateResponse Obj)
            : base(Obj.Obj)
        {
            if (GetFirstError() != null)
            {
                return;
            }

            this.PollId = NativeString.StringFromUTF8(Imported.Schema_PollUpdateResponse_GetPollId(Obj));
            this.Prompt = NativeString.StringFromUTF8(Imported.Schema_PollUpdateResponse_GetPrompt(Obj));
            this.Mean = Imported.Schema_PollUpdateResponse_GetMean(Obj);
            this.Sum = Imported.Schema_PollUpdateResponse_GetSum(Obj);
            this.Count = Imported.Schema_PollUpdateResponse_GetCount(Obj);

            Options = new List<string>();
            Results = new List<Int32>();

            for (UInt32 i = 0; i < Imported.Schema_PollUpdateResponse_GetOptionCount(Obj); i++)
            {
                Options.Add(NativeString.StringFromUTF8(Imported.Schema_PollUpdateResponse_GetOptionAt(Obj, i)));
            }
            for (UInt32 i = 0; i < Imported.Schema_PollUpdateResponse_GetResultCount(Obj); i++)
            {
                Results.Add(Imported.Schema_PollUpdateResponse_GetResultAt(Obj, i));
            }
        }

        public String PollId { get; private set; }
        public String Prompt { get; private set; }
        public double Mean { get; private set; }
        public double Sum { get; private set; }
        public Int32 Count { get; private set; }
        public List<String> Options { get; private set; }
        public List<Int32> Results { get; private set; }
    }
}