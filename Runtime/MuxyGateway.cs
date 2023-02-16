using MuxyGameLink.Imports;
using MuxyGameLink.Imports.Schema;
using System.Collections;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace MuxyGateway
{
    public class AuthenticationResponse
    {
        public string JWT { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TwitchUsername { get; set; } = string.Empty;

        public bool HasError { get; set; } = false;
    }

    public class Payload
    {
        public byte[] Bytes { get; set; } = new byte[0];
    }

    public class GameMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
    }

    public class GameText
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public enum ActionCategory
    {
        Neutral = 0,
        Hinder = 1,
        Help = 2
    }

    public enum ActionState
    {
        Unavailable = 0,
        Available = 1,
        Hidden = 2
    }

    public class Action
    {
        public static int InfiniteCount = -1;

        public string ID { get; set; } = string.Empty;

        public ActionCategory Category { get; set; } = ActionCategory.Neutral;
        public ActionState State { get; set; } = ActionState.Unavailable;
        public int Impact { get; set; } = 0;

        public string Name { set; get; } = string.Empty;
        public string Description { set; get; } = string.Empty;
        public string Icon { set; get; } = string.Empty;

        public int Count { get; set; } = InfiniteCount;
    }

    public class PollUpdate
    {
        public int Winner { get; set; } = 0;
        public int WinningVoteCount { get; set; } = 0;

        public List<int> Results { get; set; } = new List<int>();

        public int Count { set; get; } = 0;
        public double Mean { get; set; } = 0;
        public bool IsFinal { get; set; } = false;
    }

    public enum PollLocation
    {
        Default = 0
    }

    public enum PollMode
    {
        Chaos = 0,
        Order = 1
    }

    public class PollConfiguration
    {
        public static int InfiniteDuration = 0;
        public delegate void OnUpdateDelegate(PollUpdate Update);

        public string Prompt { set; get; } = string.Empty;
        public PollLocation Location { set; get; } = PollLocation.Default;
        public PollMode Mode { set; get; } = PollMode.Order;

        public List<string> Options { set; get; } = new List<string>();

        public Int32 DurationInSeconds { set; get; } = InfiniteDuration;

        public OnUpdateDelegate OnPollUpdate { set; get; } = (Update) => { };
    }

    public class SDK
    {
        private GatewaySDK Instance;
        private string GameID;
        private Encoding UTF8WithoutBOM = new UTF8Encoding(false);

        private GCHandle DebugMessage;

        public SDK(string GameID)
        {
            Instance = Imported.MGW_MakeSDK(GameID);
            this.GameID = GameID;

            GatewayDebugMessageCallback Callback = (UserData, Message) =>
            {
                Console.WriteLine(Message);
            };

            DebugMessage = GCHandle.Alloc(Callback, GCHandleType.Normal);
            Imported.MGW_SDK_OnDebugMessage(Instance, Callback, IntPtr.Zero);
        }

        ~SDK()
        {
            Imported.MGW_KillSDK(Instance);
        }

        #region Network
        public bool ReceiveMessage(String Message)
        {
            if (Message == null)
            {
                return false;
            }

            byte[] Bytes = UTF8WithoutBOM.GetBytes(Message);
            bool result = Imported.MGW_SDK_ReceiveMessage(Instance, Bytes, (uint)Bytes.Length);
            return result;
        }

        public bool HasPayloads()
        {
            return Imported.MGW_SDK_HasPayloads(Instance);
        }

        public delegate void ForeachPayloadDelegate(Payload Payload);
        public void ForeachPayload(ForeachPayloadDelegate Delegate)
        {
            GatewayForeachPayloadDelegate WrapperCallback = (UserData, Msg) =>
            {
                Payload p = new Payload();

                MGW_Payload first = Msg[0];
                p.Bytes = new byte[first.Length];
                Marshal.Copy(first.Bytes, p.Bytes, 0, (int)first.Length);

                Delegate(p);
            };

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Normal);
            Imported.MGW_SDK_ForeachPayload(Instance, WrapperCallback, IntPtr.Zero);
            Handle.Free();
        }

        public String GetSandboxURL()
        {
            string s = NativeString.StringFromUTF8AndDeallocate(Imported.MGW_SDK_GetSandboxURL(Instance));
            return s;
        }

        public String GetProductionURL()
        {
            string s = NativeString.StringFromUTF8AndDeallocate(Imported.MGW_SDK_GetProductionURL(Instance));
            return s;
        }
        #endregion

        #region Authentication
        public delegate void OnAuthenticateDelegate(AuthenticationResponse Response);
        public void AuthenticateWithPIN(String PIN, OnAuthenticateDelegate Delegate)
        {
            GatewayAuthenticateResponseDelegate WrapperCallback = (UserData, Msg) =>
            {
                AuthenticationResponse Response = new AuthenticationResponse();
                MGW_AuthenticateResponse resp = Msg[0];

                Response.JWT = NativeString.StringFromUTF8(resp.JWT);
                Response.RefreshToken = NativeString.StringFromUTF8(resp.RefreshToken);
                Response.TwitchUsername = NativeString.StringFromUTF8(resp.TwitchName);
                Response.HasError = resp.HasError;

                Delegate(Response);

                GCHandle Self = GCHandle.FromIntPtr(UserData);
                Self.Free();
            };

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Normal);
            Imported.MGW_SDK_AuthenticateWithPIN(Instance, PIN, WrapperCallback, (IntPtr)Handle);
        }

        public void AuthenticateWithRefreshToken(String Refresh, OnAuthenticateDelegate Delegate)
        {
            GatewayAuthenticateResponseDelegate WrapperCallback = (UserData, Msg) =>
            {
                AuthenticationResponse Response = new AuthenticationResponse();
                MGW_AuthenticateResponse resp = Msg[0];

                Response.JWT = NativeString.StringFromUTF8(resp.JWT);
                Response.RefreshToken = NativeString.StringFromUTF8(resp.RefreshToken);
                Response.TwitchUsername = NativeString.StringFromUTF8(resp.TwitchName);
                Response.HasError = resp.HasError;

                Delegate(Response);

                GCHandle Self = GCHandle.FromIntPtr(UserData);
                Self.Free();
            };

            GCHandle Handle = GCHandle.Alloc(WrapperCallback, GCHandleType.Normal);
            Imported.MGW_SDK_AuthenticateWithRefreshToken(Instance, Refresh, WrapperCallback, (IntPtr)Handle);
        }

        public bool IsAuthenticated
        {
            get
            {
                return Imported.MGW_SDK_IsAuthenticated(Instance);
            }
        }
        #endregion

        #region Game Metadata
        public void SetGameMetadata(GameMetadata InMeta)
        {
            MGW_GameMetadata Meta = new MGW_GameMetadata();
            Meta.GameLogo = InMeta.Logo;
            Meta.GameName = InMeta.Name;
            Meta.Theme = InMeta.Theme;

            Imported.MGW_SDK_SetGameMetadata(Instance, Meta);
        }
        #endregion

        #region Game Texts
        public void SetGameTexts(GameText[] Texts)
        {
            List<MGW_GameText> NativeTexts = new List<MGW_GameText>();

            foreach (GameText Text in Texts)
            {
                MGW_GameText Value = new MGW_GameText();
                Value.Label = Text.Label;
                Value.Value = Text.Value;
                Value.Icon = Text.Icon;

                NativeTexts.Add(Value);
            }

            Imported.MGW_SDK_SetGameTexts(Instance, NativeTexts.ToArray(), (UInt64)NativeTexts.Count);

        }
        #endregion

        #region Polls
        private GCHandle PollDelegateHandle;
        public void StartPoll(PollConfiguration Configuration)
        {
            MGW_PollConfiguration NativeConfig = new MGW_PollConfiguration();

            NativeConfig.Prompt = Configuration.Prompt;
            NativeConfig.Location = (int)Configuration.Location;
            NativeConfig.Mode = (int)Configuration.Mode;

            List<IntPtr> Strings = new List<IntPtr>();
            foreach (string Opt in Configuration.Options)
            {
                int Len = UTF8WithoutBOM.GetByteCount(Opt);
                byte[] buffer = new byte[Len + 1];
                UTF8WithoutBOM.GetBytes(Opt, 0, Opt.Length, buffer, 0);
                buffer[Len] = 0;

                IntPtr NativeStr = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, 0, NativeStr, buffer.Length);

                Strings.Add(NativeStr);
            }

            IntPtr[] StringsArray = Strings.ToArray();
            GCHandle StringsArrayHandle  = GCHandle.Alloc(StringsArray, GCHandleType.Pinned);

            NativeConfig.Options = StringsArrayHandle.AddrOfPinnedObject();
            NativeConfig.OptionsCount = (UInt64)Configuration.Options.Count;
            NativeConfig.Duration = Configuration.DurationInSeconds;

            GatewayPollUpdateDelegate WrapperCallback = (IntPtr User, MGW_PollUpdate[] UpdatePtr) =>
            {
                MGW_PollUpdate NativeUpdate = UpdatePtr[0];

                PollUpdate Update = new PollUpdate();
                Update.Winner = NativeUpdate.Winner;
                Update.WinningVoteCount = NativeUpdate.WinningVoteCount;

                List<int> Results = new List<int>();
                for (UInt64 i = 0; i < NativeUpdate.ResultCount; i++)
                {
                    Results.Add(NativeUpdate.Results[i]);
                }

                Update.Results = Results;
                Update.Count = NativeUpdate.Count;
                Update.Mean = NativeUpdate.Mean;
                Update.IsFinal = NativeUpdate.IsFinal;

                Configuration.OnPollUpdate(Update);
            };

            NativeConfig.OnUpdate = WrapperCallback;
            NativeConfig.User = IntPtr.Zero;

            GCHandle NextPollDelegateHandle = GCHandle.Alloc(WrapperCallback, GCHandleType.Normal);
            Imported.MGW_SDK_StartPoll(Instance, NativeConfig);

            StringsArrayHandle.Free();
            foreach (IntPtr Allocated in Strings)
            {
                Marshal.FreeHGlobal(Allocated);
            }

            if (PollDelegateHandle.IsAllocated)
            {
                PollDelegateHandle.Free();
            }

            PollDelegateHandle = NextPollDelegateHandle;
        }

        public void StopPoll()
        {
            Imported.MGW_SDK_StopPoll(Instance);
        }

        #endregion

        #region Actions
        public void SetActions(Action[] Actions)
        {
            List<MGW_Action> NativeActions = new List<MGW_Action>();

            foreach (Action Action in Actions)
            {
                MGW_Action Value = new MGW_Action();
                Value.ID = Action.ID;
                Value.Category = ((int)Action.Category);
                Value.State = ((int)Action.State);
                Value.Impact = Action.Impact;
                Value.Name = Action.Name;
                Value.Description = Action.Description;
                Value.Icon = Action.Icon;
                Value.Count = Action.Count;

                NativeActions.Add(Value);
            }

            Imported.MGW_SDK_SetActions(Instance, NativeActions.ToArray(), (UInt64)NativeActions.Count);
        }

        public void EnableAction(string ID)
        {
            Imported.MGW_SDK_EnableAction(Instance, ID);
        }

        public void DisableAction(string ID)
        {
            Imported.MGW_SDK_DisableAction(Instance, ID);
        }

        public void SetActionCount(string ID, int Count)
        {
            Imported.MGW_SDK_SetActionCount(Instance, ID, Count);
        }
        #endregion
    }
}
