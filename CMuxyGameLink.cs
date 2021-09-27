using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using System.Runtime.InteropServices;

using UnityEngine.UI;

using MuxyGameLink;

public class CMuxyGameLink : MonoBehaviour
{
    public String GAMELINK_CLIENT_ID = "";

    public List<GameObject> Groups;

    public InputField PINInput;
    public InputField RefreshTokenInput;
    public InputField PollIdInput;
    public InputField PromptInput;
    public InputField SKUInput;
    public InputField BroadcastInput;
    public InputField StateInput;
    public Dropdown BroadcastTargetDropdown;
    public Dropdown StateTargetDropdown;
    public Dropdown TransactionsDropdown;
    public Text ResultText;

    private String GAMELINK_PLAYERPREF_RT = "MuxyGameLinkRefreshToken";

    private SDK.AuthenticationCallback AuthCB;

    private MuxyGameLink.SDK GameLink;
    private WebSocket WS;

    private Color ColWhite = new Color(255, 255, 255);
    private Color ColSuccess = new Color(0, 255, 0);
    private Color ColFailure = new Color(255, 0, 0);

    void LogResult(String Res, Color? Col = null, bool AddToConsole = false)
    {
        ResultText.text = Res;
        ResultText.color = Col ?? ColWhite;
        if (AddToConsole) Debug.Log(Res);
    }

    void UISetup()
    {
        String PrefRefreshToken = PlayerPrefs.GetString(GAMELINK_PLAYERPREF_RT, "");
        if (PrefRefreshToken != "")
        {
            RefreshTokenInput.text = PrefRefreshToken;
        }
    }

    void GameLinkSetup()
    {
        GameLink = new SDK(GAMELINK_CLIENT_ID);

        AuthCB = (AuthResp) =>
        {
            Error Err = AuthResp.GetFirstError();
            if (Err != null)
            {
                LogResult("Authentication Failed! | " + Err.Title + ":" + Err.Detail, ColFailure, true);
                ResultText.color = ColFailure;
                PlayerPrefs.SetString(GAMELINK_PLAYERPREF_RT, "");
            }
            else
            {
                if (GameLink.IsAuthenticated())
                {
                    String RefreshToken = GameLink.User?.RefreshToken;
                    if (RefreshToken != null)
                    {
                        LogResult("Authentication Successful! | Saving RefreshToken for later Authentication", ColSuccess, true);
                        if (PlayerPrefs.GetString(GAMELINK_PLAYERPREF_RT, "") == "") PlayerPrefs.SetString(GAMELINK_PLAYERPREF_RT, RefreshToken);
                        RefreshTokenInput.text = RefreshToken;
                    }
                }
            }
        };

        GameLink.OnDebugMessage(
        (Message) =>
        {
            //LogResult("GAMELINK_DEBUG: " + Message, ColWhite, true);
        });
    }

    async void Start()
    {
        GameLinkSetup();
        UISetup();

        String Addr = "ws://" + GameLink.ConnectionAddress(Stage.Sandbox);
        WS = new WebSocket(Addr);

        WS.OnOpen += () =>
        {
            Debug.Log("WS Connection open [" + Addr + "]!");
        };

        WS.OnError += (e) =>
        {
            Debug.Log("WS Error! " + e);
        };

        WS.OnClose += (e) =>
        {
            Debug.Log("WS Connection closed!");
        };

        WS.OnMessage += (bytes) =>
        {
            String Message = System.Text.Encoding.UTF8.GetString(bytes);
            GameLink.ReceiveMessage(Message);
            Debug.Log("WS OnMessage! " + Message);
        };

        // Keep sending messages at every 0.1s
        InvokeRepeating("SendWebSocketMessage", 0.0f, 0.1f);
        await WS.Connect();
    }

    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        WS.DispatchMessageQueue();
    #endif
    }

    void SendWebSocketMessage()
    {
        if (WS.State == WebSocketState.Open)
        {
            GameLink.ForEachPayload((string Payload) =>
            {
                WS.SendText(Payload);
            });
        }
    }

    private void OnApplicationQuit()
    {
        WS.Close();
    }

    public void SwitchTab(GameObject Visible)
    {
        foreach (GameObject GO in Groups)
        {
            if (GO == Visible) GO.SetActive(true);
            else               GO.SetActive(false);
        }
    }
    public void OnClickAuthWithPIN()
    {
        GameLink.AuthenticateWithPIN(PINInput.text, AuthCB);
    }

    public void OnClickAuthWithRefreshToken()
    {
        GameLink.AuthenticateWithRefreshToken(RefreshTokenInput.text, AuthCB);
    }

    public void OnClickCreatePoll()
    {
        LogResult("Created Poll: " + PollIdInput.text, ColSuccess);
        List<string> Options = new List<string>(); 
        Options.Add("Option1");
        Options.Add("Option2");
        Options.Add("Option3");
        GameLink.CreatePoll(PollIdInput.text, PromptInput.text, Options);
    }

    public void OnClickDeletePoll()
    {
        LogResult("Deleted Poll: " + PollIdInput.text, ColWhite);
        GameLink.DeletePoll(PollIdInput.text);
    }

    public void OnClickSubscribeToPoll()
    {
        LogResult("Subscribed to Poll: " + PollIdInput.text, ColSuccess);
        GameLink.SubscribeToPoll(PollIdInput.text);

    }

    public void OnClickUnsubscribeToPoll()
    {
        LogResult("Unsubscribed from Poll: " + PollIdInput.text, ColWhite);
        GameLink.UnsubscribeFromPoll(PollIdInput.text);
    }

    private UInt32 OnPollUpdateId = 0;
    public void OnClickOnPollUpdate()
    {
        LogResult("Attached OnPollUpdate: " + OnPollUpdateId, ColSuccess);
        OnPollUpdateId = GameLink.OnPollUpdate((Resp) =>
        {
            LogResult("OnPollUpdate | Responses: " + Resp.Count, ColWhite, true);
        });
    }

    public void OnClickDetachOnPollUpdate()
    {
        LogResult("Detached OnPollUpdate: " + OnPollUpdateId, ColWhite);
        GameLink.DetachOnPollUpdate(OnPollUpdateId);
    }

    List<Transaction> TxList = new List<Transaction>(); 
    public void OnClickGetOutstandingTransactions()
    {
        TransactionsDropdown.ClearOptions();
        UInt32 RetId = GameLink.GetOutstandingTransactions(SKUInput.text, (Transactions) =>
        {
            LogResult("Outstanding transactions count: " + Transactions.Count, ColWhite);
            for (UInt32 i = 0; i < Transactions.Count; i++)
            {
                Transaction Tx = Transactions.At(i);
                TxList.Add(Tx);
                List<string> DropOptions = new List<string>();
                DropOptions.Add("ID " + Tx.Id + ": " + Tx.UserName + " bought " + Tx.DisplayName + " [" + Tx.SKU + " ] ($" + Tx.Cost + ")");
                TransactionsDropdown.AddOptions(DropOptions);
            }
        });
        LogResult("Getting outstanding transactions", ColWhite);
    }

    public void OnClickValidateTransaction()
    {
        Transaction Tx = TxList[TransactionsDropdown.value];
        GameLink.ValidateTransaction(Tx.Id, "");
        LogResult("Validated transaction: " + Tx.Id, ColSuccess);
        OnClickGetOutstandingTransactions();
    }

    public void OnClickRefundTransaction()
    {
        Transaction Tx = TxList[TransactionsDropdown.value];
        GameLink.RefundTransactionByID(Tx.Id, Tx.UserId);
        LogResult("Refunded transaction: " + Tx.Id, ColSuccess);
        OnClickGetOutstandingTransactions();
    }

    public void OnClickSubscribeToSKU()
    {
        LogResult("Subscribed to SKU: " + SKUInput.text, ColSuccess);
        GameLink.SubscribeToSKU(SKUInput.text);
    }

    public void OnClickUnsubscribeFromSKU()
    {
        LogResult("Unsubscribed from SKU: " + SKUInput.text, ColWhite);
        GameLink.UnsubscribeFromSKU(SKUInput.text);
    }

    public void OnClickSubscribeToAllPurchases()
    {
        LogResult("Subscribed to all purchases", ColSuccess);
        GameLink.SubscribeToAllPurchases();
    }

    public void OnClickUnsubscribeFromAllPurchases()
    {
        LogResult("Unsubscribed from all purchases", ColWhite);
        GameLink.UnsubscribeFromAllPurchases();
    }

    private UInt32 OnTransactionId = 0;
    public void OnClickOnTransaction()
    {
        LogResult("Attached OnTransaction: " + OnTransactionId, ColSuccess);

        OnTransactionId = GameLink.OnTransaction(
        (Tx) => 
        {
            LogResult("ID " + Tx.Id + ": " + Tx.UserName + " bought " + Tx.DisplayName + " [" + Tx.SKU + " ] ($" + Tx.Cost + ")", ColWhite, true);
        });
    }

    public void OnClickDetachOnTransaction()
    {
        LogResult("Detached OnTransaction: " + OnTransactionId, ColWhite);
        GameLink.DetachOnTransaction(OnTransactionId);
    }

    public void OnClickSendBroadcast()
    {
        LogResult("Sent broadcast", ColSuccess);
        // SDK.STATE_TARGET_CHANNEL / SDK.STATE_TARGET_EXTENSION
        GameLink.SendBroadcast(BroadcastTargetDropdown.options[BroadcastTargetDropdown.value].text, BroadcastInput.text);
    }

    public void OnClickSubscribeToDatastream()
    {
        LogResult("Subscribed To Datastream", ColSuccess);
        GameLink.SubscribeToDatastream();
    }

    public void OnClickUnsubscribeFromDatastream()
    {
        LogResult("Unsubscribed From Datastream", ColWhite);
        GameLink.UnsubscribeFromDatastream();
    }

    private UInt32 OnDatastreamId = 0;
    public void OnClickOnDatastream()
    {
        LogResult("Attached OnDatastream: " + OnDatastreamId, ColSuccess);

        OnDatastreamId = GameLink.OnDatastream(
        (Update) =>
        {
            LogResult("OnDatastream event count: " + Update.Count, ColWhite, true);
            for (UInt32 i = 0; i < Update.Count; i++)
            {
                DatastreamUpdate.Event Event = Update.At(i);
                LogResult("Datastream Event JSON: " + Event.Json, ColWhite, true);
            }
        });
    }

    public void OnClickDetachDatastream()
    {
        LogResult("Detached OnDatastream: " + OnDatastreamId, ColWhite);
        GameLink.DetachOnDatastream(OnDatastreamId);
    }

    public void OnClickSetState()
    {
        String Target = StateTargetDropdown.options[StateTargetDropdown.value].text;
        LogResult("Set State (" + Target + ")", ColSuccess);
        GameLink.SetState(Target, StateInput.text);
    }

    public void OnClickGetState()
    {
        String Target = StateTargetDropdown.options[StateTargetDropdown.value].text;
        LogResult("Get State (" + Target + ")", ColSuccess);

        GameLink.GetState(Target, 
        (Response) =>
        {
            LogResult("GetState JSON (" + Target + "): " + Response.Json, ColWhite, true);
        });
    }

    public void OnClickSubscribeToStateUpdates()
    {
        String Target = StateTargetDropdown.options[StateTargetDropdown.value].text;
        LogResult("Subscribed To State Updates (" + Target + ")", ColSuccess);
        GameLink.SubscribeToStateUpdates(Target);
    }

    public void OnClickUnsubscribeFromStateUpdates()
    {
        String Target = StateTargetDropdown.options[StateTargetDropdown.value].text;
        LogResult("Unsubscribed From State Updates (" + Target + ")", ColWhite);
        GameLink.SubscribeToStateUpdates(Target);
    }

    private UInt32 StateUpdateId = 0;
    public void OnClickOnStateUpdate()
    {
        LogResult("Attached State Update: " + StateUpdateId, ColSuccess);
        StateUpdateId = GameLink.OnStateUpdate(
        (Update) =>
        { 
            LogResult("OnStateUpdate JSON (" + Update.Target + "): " + Update.Json, ColWhite, true);
        });
    }

    public void OnClickDetachStateUpdate()
    {
        LogResult("Detached State Update: " + StateUpdateId, ColSuccess);
        GameLink.DetachOnStateUpdate(StateUpdateId);
    }
}
