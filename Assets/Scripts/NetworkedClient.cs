using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{

    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    byte error;
    bool isConnected = false;
    int ourClientID;

    //Game Object Variables 
    GameObject gameSystemManager;
    [SerializeField] private GameObject gameboard;


    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach(GameObject go in allObjects)
        {
            if (go.name == "SystemManager")
                gameSystemManager = go;     
        }
            Connect();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }

    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "192.168.86.176", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);
            }
        }
    }

    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }

    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

        string[] csv = msg.Split(',');

        int signifier = int.Parse(csv[0]);

        //
        //Account Create and Login Checks 
        //
        if(signifier == ServerToClientSignifiers.AccountCreationComplete)
        {
            gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.MainMenu);
        }
        else if (signifier == ServerToClientSignifiers.LoginComplete)
        {
            gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.MainMenu);
        }

        //
        // Game Start, Turns and Win/Lose 
        //

        else if (signifier == ServerToClientSignifiers.GameStart)
        {
            gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.Game);

            SendMessageToHost(ClientToServerSignifiers.PlayGame + "");
        }

        //Set Player Number Check
        else if (signifier == ServerToClientSignifiers.SetPlayerNumber)
        {
            int playerNum = int.Parse(csv[1]);

            Debug.Log("Player Num: " + playerNum);

            gameboard.GetComponent<Gameboard>().SetPlayerNumber(playerNum);
        }

        //Players Turn Check
        else if (signifier == ServerToClientSignifiers.PlayersTurn)
        {
            gameboard.GetComponent<Gameboard>().IsThisPlayersTurn = true;
        }

        //Update Gameboard Check
        else if (signifier == ServerToClientSignifiers.UpdateGameboard)
        {
            int nodeM = int.Parse(csv[1]);
            int nodeIndex = int.Parse(csv[2]);
            Mark mark;

            switch (nodeM)
            {
                case 1:
                    mark = Mark.X;
                    break;

                case 2:
                    mark = Mark.O;
                    break;

                default:
                    mark = Mark.NONE;
                    break;
            }
            gameboard.GetComponent<Gameboard>().UpdateGameBoard(mark, nodeIndex);
        }

        //Game End Check 
        else if (signifier == ServerToClientSignifiers.EndGame)
        {
            int endGameCondition = int.Parse(csv[1]);

            switch (endGameCondition)
            {
                case 0:
                    //Game Lose
                    Debug.Log("Game Lose");
                    gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.LoseGame);
                    break;

                case 1:
                    //Game Win
                    Debug.Log("Game Win");
                    gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.WinGame);
                    break;

                case 3:
                    //Observer
                    Debug.Log("Game Over for Observer");
                    gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.ObserverEnd);
                    break;

                case 4:
                    //Tie Game 
                    Debug.Log("Tie Game");
                    gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.TieGame);
                    break;

                default:
                    break;
            } 
        }

        //Player 1 Message Check
        else if (signifier == ServerToClientSignifiers.DisplayPlayer1Message)
        {
            string msg1 = csv[1];
            StartCoroutine(gameSystemManager.GetComponent<SystemManager>().DisplayPlayer1Message(msg1));
        }

        //Player 2 Message Check
        else if (signifier == ServerToClientSignifiers.DisplayPlayer2Message)
        {
            string msg2 = csv[1];
            StartCoroutine(gameSystemManager.GetComponent<SystemManager>().DisplayPlayer2Message(msg2));
        }

        else if (signifier == ServerToClientSignifiers.ReplayMove)
        {
            int playerID = int.Parse(csv[1]);
            int nodeID = int.Parse(csv[2]);
            StartCoroutine(gameSystemManager.GetComponent<SystemManager>().DisplayReplayMove(playerID, nodeID));
        }

        else if (signifier == ServerToClientSignifiers.JoinAsObserver)
        {
            gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.Observer);
        }

        else if (signifier == ServerToClientSignifiers.RematchConfirmed)
        {
            gameboard.GetComponent<Gameboard>().Rematch();
            gameSystemManager.GetComponent<SystemManager>().ChangeState(GameStates.Game);
            SendMessageToHost(ClientToServerSignifiers.PlayGame + "");

            Debug.Log("Rematch");
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }
}


public static class ClientToServerSignifiers
{
    public const int CreateAccount = 1;

    public const int Login = 2;

    public const int JoinQueueForGameRoom = 3;

    public const int PlayGame = 4;

    public const int TurnTaken = 5;

    public const int PlayerWin = 6;

    public const int PlayerMessage = 7;

    public const int RequestReplayMove = 8;

    public const int PlayerRequestRematch = 9;

    public const int TieGame = 10;
}

public static class ServerToClientSignifiers
{
    public const int LoginComplete = 1;

    public const int LoginFailed = 2;

    public const int AccountCreationComplete = 3;

    public const int AccountCreationFailed = 4;

    public const int GameStart = 5;

    public const int SetPlayerNumber = 6;

    public const int PlayersTurn = 7;

    public const int UpdateGameboard = 8;

    public const int EndGame = 9;

    public const int DisplayPlayer1Message = 10;

    public const int DisplayPlayer2Message = 11;

    public const int JoinAsObserver = 12;

    public const int ReplayMove = 13;

    public const int RematchConfirmed = 14;
}



