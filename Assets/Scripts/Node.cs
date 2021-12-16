using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    //Singleton Ref
    private Gameboard gameboard;

    //Member Variables 
    [SerializeField] private int nodeID;
    public Mark NodeMark;

    public bool isOccupied = false;

    private Collider2D collider;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        //Node Setup
        NodeMark = Mark.NONE;
        collider = gameObject.GetComponent<Collider2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        //Singleton Setup
        gameboard = Gameboard.gameboardInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameboard.IsThisPlayersTurn && gameboard.IsObersever == false)
        {
            CheckForInput();
        }
    }

    private void CheckForInput()
    {
       

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos = new Vector2(mousePosition.x, mousePosition.y);

            if (collider.bounds.Contains(mousePos) && isOccupied == false)
            {
                Debug.Log("Input");
                //Send Selected Node to Gameboard
                gameboard.SelectPlayerNode(nodeID);
                isOccupied = true;
            }
        }
    }

    
    public void UpdateNode(Mark recieversNode)
    {
        switch (recieversNode)
        {
            case Mark.NONE:
                break;
            case Mark.X:
                PlaceXSprite();
                break;
            case Mark.O:
                PlaceOSprite();
                break;
            default:
                break;
        }
    }

    public void PlaceXSprite()
    {
        NodeMark = Mark.X;
        spriteRenderer.sprite = gameboard.xSprite;
        isOccupied = true;
    }

    public void PlaceOSprite()
    {
        NodeMark = Mark.O;
        spriteRenderer.sprite = gameboard.oSprite;
        isOccupied = true;
    }

    public void ResetNode()
    {
        NodeMark = Mark.NONE;
        spriteRenderer.sprite = null;
        isOccupied = false;
    }

}
