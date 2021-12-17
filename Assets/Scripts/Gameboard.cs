using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Mark
{
    NONE,
    X,
    O
}

public class Gameboard : MonoBehaviour
{
    //Gameboard Singleton!!
    public static Gameboard gameboardInstance;

    //Networked Client 
    GameObject networkedClient;

    //Node Array 
    [SerializeField] private Node[] nodes;

    //Memeber Variables 
    public Mark thisPlayersMark;

    public Mark Player1Mark = Mark.X;
    public Mark Player2Mark = Mark.O;

    public Sprite xSprite;
    public Sprite oSprite;

    public bool IsThisPlayersTurn = false;
    public bool IsObersever = false;

    private int numOfNodesFull = 0;

    private void Awake()
    {
        //Game Singleton Setup
        if (gameboardInstance != null)
        {
            return;
        }
        gameboardInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetNetworkedClient();
    }

    public void SetPlayerNumber(int playerNumber)
    {
        if (playerNumber == 1)
        {
            thisPlayersMark = Player1Mark;
            IsThisPlayersTurn = true;
        }
        else if (playerNumber == 2)
        {
            thisPlayersMark = Player2Mark;
        }
    }

    public void SelectPlayerNode(int nodeID)
    {
        //Send Selected Node and PlayerMark to the Server
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TurnTaken + "," + (int)thisPlayersMark + "," + nodeID);
        IsThisPlayersTurn = false;
    }


    public void UpdateGameBoard(Mark nodeMark, int nodeIndex)
    {
        nodes[nodeIndex].UpdateNode(nodeMark);

        //After updating the gameboard, check for win
        bool hasWon = CheckForWin();

        bool hasTied = CheckForTie();

        if (hasWon)
        {
            //This Player has won
            Debug.Log("Player Win!");

            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.PlayerWin + "");
        }

        if (hasTied)
        {
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TieGame + "");
        }
    }


    private void GetNetworkedClient()
    {
        GameObject []
        allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name == "NetworkedClient")
                networkedClient = go;
        }
    }

    private void ClearGameBoard()
    {
        numOfNodesFull = 0;

        foreach (Node node in nodes)
        {
            node.ResetNode();
        }
    }


    public void Rematch()
    {
        ClearGameBoard();
    }


    public bool CheckForTie()
    {
        numOfNodesFull += 1;

        Debug.Log("Number of Tiles Full: " + numOfNodesFull);

        if (numOfNodesFull == 9)
        {
            Debug.Log("Tie");

            return true;
        }
        return false;
    }

    //
    // Game Win Condition Check
    //

    //Check for all possbile win conditions 
    public bool CheckForWin()
    {
        return
        AreNodesMatched(0, 1, 2) || AreNodesMatched(3, 4, 5) || AreNodesMatched(6, 7, 8) ||
        AreNodesMatched(0, 3, 6) || AreNodesMatched(1, 4, 7) || AreNodesMatched(2, 5, 8) ||
        AreNodesMatched(0, 4, 8) || AreNodesMatched(2, 4, 6);
    }

    
    //Loop through node array testing nodeMarks
    private bool AreNodesMatched(int i, int j, int k)
    {
        Mark m = thisPlayersMark;

        bool isMatched = (nodes[i].NodeMark == m && nodes[j].NodeMark == m && nodes[k].NodeMark == m);
        return isMatched;
    }
    
}
