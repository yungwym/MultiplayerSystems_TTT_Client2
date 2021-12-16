using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemManager : MonoBehaviour
{
    //Base UI
    GameObject basePanel;

    //Login Panel Variables 
    GameObject loginPanel;

    // UI Variables 
    GameObject submitButton;
    GameObject usernameText;
    GameObject passwordText;
    GameObject usernameInput;
    GameObject passwordInput;
    GameObject loginToggle;
    GameObject createToggle;

    //Join Game Room Panel
    GameObject joinGameRoomPanel;
    GameObject joinGameRoomButton;

    //Waiting Panel
    GameObject waitingPanel;

    //Gameboard
    GameObject gameboard;

    //End Game Panel
    GameObject endGamePanel;
    GameObject gameResultText;
    GameObject replayButton;
    GameObject quitButton;

    GameObject replayBlock;
    GameObject replayText;

    //HUD Panel
    GameObject hudPanel;
    GameObject player1Panel;
    GameObject player2Panel;
    GameObject msgLogPanel;

    GameObject prefixedMsg1;
    GameObject prefixedMsg2;
    GameObject prefixedMsg3;
    GameObject prefixedMsg4;

    GameObject customMsgInputField;
    GameObject customMsgSendButton;

    GameObject player1MsgBlock;
    GameObject player2MsgBlock;

    GameObject player1MsgText;
    GameObject player2MsgText;


    
    //Member Variables 
    GameObject networkedClient;


    // Start is called before the first frame update
    void Start()
    {
        SetupUserInterface();
    }

    // Update is called once per frame
    void Update()
    {

    }



   
    //Login and Create Account Functions 
    public void LoginTogglePressed(bool newValue)
    {
        createToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    public void CreateTogglePressed(bool newValue)
    {
        loginToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }
    public void SubmitButtonPressed()
    {
        //Send Login Info to Server
        Debug.Log("Submit Clicked");

        string user = usernameInput.GetComponent<InputField>().text;
        string pass = passwordInput.GetComponent<InputField>().text;

        string msg;

        if (createToggle.GetComponent<Toggle>().isOn)

            msg = ClientToServerSignifiers.CreateAccount + "," + user + "," + pass;
        else
            msg = ClientToServerSignifiers.Login + "," + user + "," + pass;


        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(msg);

        Debug.Log(msg);
    }

    public void JoinGameRoomButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinQueueForGameRoom + "");
        ChangeState(GameStates.WaitingInQueueForOtherPlayers);
    }

    public void ReplayButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.RequestReplayMove + "");
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }


    //Messaging Functions 
    public void SendPrefixed1()
    {
        string buttonTxt = "Hello";
        SendMessageToOpponent(buttonTxt);
    }

    public void SendPrefixed2()
    {
        string buttonTxt = "Nice Move!";
        SendMessageToOpponent(buttonTxt);
    }

    public void SendPrefixed3()
    {
        string buttonTxt = "Good Game";
        SendMessageToOpponent(buttonTxt);
    }

    public void SendPrefixed4()
    {
        string buttonTxt = "No Way!";
        SendMessageToOpponent(buttonTxt);
    }

    public void SendCustomMsg()
    {
        string playerMsg = customMsgInputField.GetComponent<InputField>().text;

        SendMessageToOpponent(playerMsg);
    }

    public void SendMessageToOpponent(string msg)
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.PlayerMessage + "," + msg);
        Debug.Log(msg);
    }

    public IEnumerator DisplayPlayer1Message(string msg)
    {
        player1MsgBlock.SetActive(true);
        player1MsgText.SetActive(true);
        player1MsgText.GetComponent<Text>().text = msg;
        yield return new WaitForSeconds(5.0f);
        player1MsgBlock.SetActive(false);
        player1MsgText.SetActive(false);
    }

    public IEnumerator DisplayPlayer2Message(string msg)
    {
        player2MsgBlock.SetActive(true);
        player2MsgText.SetActive(true);
        player2MsgText.GetComponent<Text>().text = msg;
        yield return new WaitForSeconds(5.0f);
        player2MsgBlock.SetActive(false);
        player2MsgText.SetActive(false);
    }

    public IEnumerator DisplayReplayMove(int playerID, int nodeID)
    {
        replayBlock.SetActive(true);
        replayText.SetActive(true);
        replayText.GetComponent<Text>().text = "Player: " + playerID + " placed tile at: " + nodeID;
        yield return new WaitForSeconds(5.0f);
        replayBlock.SetActive(false);
        replayText.SetActive(false);
    }

    public void DisplayEndScreen(int endCondition)
    {
        endGamePanel.SetActive(true);

        switch (endCondition)
        {
            //Player Lose
            case 0:
                endGamePanel.GetComponent<Image>().color = Color.red;
                gameResultText.GetComponent<Text>().text = "You Have Lost!";
                break;

            //Player Win
            case 1:
                endGamePanel.GetComponent<Image>().color = Color.blue;
                gameResultText.GetComponent<Text>().text = "You Have Won!";
                break;
            //Observer End
            case 3:
                endGamePanel.GetComponent<Image>().color = Color.yellow;
                gameResultText.GetComponent<Text>().text = "Game Over!";
                break;

            default:
                break;
        }
    }


    //Game States and Game UI
    public void ChangeState(int newState)
    {
        basePanel.SetActive(false);
        loginPanel.SetActive(false);
        joinGameRoomPanel.SetActive(false);
        waitingPanel.SetActive(false);
        gameboard.SetActive(false);
        endGamePanel.SetActive(false);
        hudPanel.SetActive(false);

        player1MsgBlock.SetActive(false);
        player1MsgText.SetActive(false);

        player2MsgBlock.SetActive(false);
        player2MsgText.SetActive(false);

        if (newState == GameStates.LoginMenu)
        {
            basePanel.SetActive(true);
            loginPanel.SetActive(true);
        }
        else if (newState == GameStates.MainMenu)
        {
            basePanel.SetActive(true);
            joinGameRoomPanel.SetActive(true);
        }
        else if (newState == GameStates.WaitingInQueueForOtherPlayers)
        {
            basePanel.SetActive(true);
            waitingPanel.SetActive(true);
        }
        else if (newState == GameStates.Game)
        {
            gameboard.SetActive(true);
            hudPanel.SetActive(true);
        }
        else if (newState == GameStates.Observer)
        {
            gameboard.SetActive(true);
            gameboard.GetComponent<Gameboard>().IsObersever = true;
        }
        else if (newState == GameStates.WinGame)
        {
            DisplayEndScreen(1);
        }
        else if (newState == GameStates.LoseGame)
        {
            DisplayEndScreen(0);
        }
        else if(newState == GameStates.ObserverEnd)
        {
            DisplayEndScreen(3);
        }
    }

    private void SetupUserInterface()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            //Get Base Ui Elemts 
            if (go.name == "BaseUIPanel")
                basePanel = go;

            //Get Login Panel Elements
            else if (go.name == "LoginPanel")
                loginPanel = go;
            else if (go.name == "UserField")
                usernameInput = go;
            else if (go.name == "PassField")
                passwordInput = go;
            else if (go.name == "UsernameText")
                usernameText = go;
            else if (go.name == "PasswordText")
                passwordText = go;
            else if (go.name == "SubmitButton")
                submitButton = go;
            else if (go.name == "LoginToggle")
                loginToggle = go;
            else if (go.name == "CreateToggle")
                createToggle = go;

            //Get joinGameRoom Panel Elements
            else if (go.name == "JoinGameRoomPanel")
                joinGameRoomPanel = go;
            else if (go.name == "JoinGameRoomButton")
                joinGameRoomButton = go;

            //Get waiting panel elements
            else if (go.name == "WaitingPanel")
                waitingPanel = go;

            //Get Gameboard Elements 
            else if (go.name == "GameBoard")
                gameboard = go;

            //Get EndGame Panel Elements 
            else if (go.name == "EndGamePanel")
                endGamePanel = go;
            else if (go.name == "ResultText")
                gameResultText = go;
            else if (go.name == "ReplayButton")
                replayButton = go;
            else if (go.name == "QuitButton")
                quitButton = go;

            else if (go.name == "ReplayBlock")
                replayBlock = go;
            else if (go.name == "ReplayText")
                replayText = go;

            //Get HUD Panel
            else if (go.name == "HUDPanel")
                hudPanel = go;
            else if (go.name == "PlayerPanel")
                player1Panel = go;
            else if (go.name == "Player2Panel")
                player2Panel = go;

            else if (go.name == "Prefixed1")
                prefixedMsg1 = go;

            else if (go.name == "Prefixed2")
                prefixedMsg2 = go;

            else if (go.name == "Prefixed3")
                prefixedMsg3 = go;

            else if (go.name == "Prefixed4")
                prefixedMsg4 = go;

            else if (go.name == "CustomMsgInputField")
                customMsgInputField = go;

            else if (go.name == "SendButton")
                customMsgSendButton = go;

            //Message Log
            else if (go.name == "Player1Block")
                player1MsgBlock = go;
            else if (go.name == "Player2Block")
                player2MsgBlock = go;
            else if (go.name == "Player1Text")
                player1MsgText = go;
            else if (go.name == "Player2Text")
                player2MsgText = go;


            //Opponent Panel



            //Get Networked Client 
            else if (go.name == "NetworkedClient")
                networkedClient = go;
          
          
        }

        //Login Buttons and Toggle Setup
        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        joinGameRoomButton.GetComponent<Button>().onClick.AddListener(JoinGameRoomButtonPressed);

        loginToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginTogglePressed);
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateTogglePressed);

        //Prefixed and Custom Message Setup
        prefixedMsg1.GetComponent<Button>().onClick.AddListener(SendPrefixed1);
        prefixedMsg2.GetComponent<Button>().onClick.AddListener(SendPrefixed2);
        prefixedMsg3.GetComponent<Button>().onClick.AddListener(SendPrefixed3);
        prefixedMsg4.GetComponent<Button>().onClick.AddListener(SendPrefixed4);
        customMsgSendButton.GetComponent<Button>().onClick.AddListener(SendCustomMsg);

        replayButton.GetComponent<Button>().onClick.AddListener(ReplayButtonPressed);
        quitButton.GetComponent<Button>().onClick.AddListener(QuitButtonPressed);

        //Set beginning Game State
        ChangeState(GameStates.LoginMenu);
    }
}



//Game States Class 
public static class GameStates
{
    public const int LoginMenu = 1;

    public const int MainMenu = 2;

    public const int WaitingInQueueForOtherPlayers = 3;

    public const int Game = 4;

    public const int WinGame = 5;

    public const int LoseGame = 6;

    public const int EndGameObserver = 7;

    public const int Observer = 8;

    public const int ObserverEnd = 9;
}
