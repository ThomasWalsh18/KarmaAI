using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Visualization : MonoBehaviour
{
    GameHandler gameController;
    public bool flipped = false;
    public Text hide;

    public Image turnIndicator;
    public Text textTurn;

    public Text thoughts;

    void Start()
    {
        gameController = gameObject.GetComponent<GameHandler>();
    }

    void Update()
    {

        if (gameController.pTurn)
        {
            if(turnIndicator.color != Color.yellow)
            {
                turnIndicator.color = Color.yellow;
                textTurn.text = "Player's Turn";
            }
        } else
        {
            if (turnIndicator.color != Color.red)
            {
                turnIndicator.color = Color.red;
                textTurn.text = "AI's Turn";
            }
        }
    }

    public void revealAIHand()
    {
        flipped = !flipped;
        for(int i = 0; i < gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Count; i++)
        {
            gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[i].card.GetComponent<CardFlip>().flipped = !gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[i].card.GetComponent<CardFlip>().flipped;
        }
        if (flipped)
        {
            hide.text = "Reveal";
        } else
        {
            hide.text = "Hide";
        }
    }

    public void changeAIText(string toChange)
    {
        thoughts.text = toChange;
    }

    public void MoveThoughts(AI.tree tree)
    {
        int lowestValue = int.MaxValue;
        int j = 0;
        for (int i = 0; i < tree.Root.children.Count; i++)
        {
            if (tree.Root.children[i].value < lowestValue)
            {
                lowestValue = tree.Root.children[i].value;
                j = i;
            }
        }
        if (tree.Root.value == tree.Root.children[j].value)
        {
            thoughts.text = "I played a " + tree.Root.mostValuableMove.ToString() + ". This gets the highest value of " + tree.Root.value.ToString() + ". This was the only move I could have done!";
        } else
        {
            thoughts.text = "I played a " + tree.Root.mostValuableMove.ToString() + ". This gets the highest value of " + tree.Root.value.ToString() + " the worst move was a " + tree.Root.children[j].moveChoice.ToString() + ", which gets a score of " +  tree.Root.children[j].value.ToString()+ "."; 
        }
    }
}
