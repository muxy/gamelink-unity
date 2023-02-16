using MuxyGameLink.Imports;
using MuxyGameLink.Imports.Schema;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace MuxyGateway
{
    public struct AuthenticationResponse
    {
        public string JWT { get; set; }
        public string RefreshToken { get; set; }
        public string TwitchUsername { get; set; }

        public bool HasError { get; set; }
    }

    public struct Payload
    {
        public byte[] Bytes { get; set; }
    }

    public class SDK
    {
        private GatewaySDK Instance;
        private string GameID;

        public SDK(string GameID) 
        { 
            Instance = Imported.MGW_MakeSDK(GameID);
            this.GameID = GameID;
        }

        #region Network
        public bool ReceiveMessage(String Message)
        {
            if (Message == null)
            { 
                return false; 
            }

            byte[] Bytes = Encoding.UTF8.GetBytes(Message);
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

            Imported.MGW_SDK_ForeachPayload(Instance, WrapperCallback, IntPtr.Zero);
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
        #endregion
    }
}
