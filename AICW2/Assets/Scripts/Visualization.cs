using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Visualization : MonoBehaviour
{
    GameHandler gameController;
    public bool flipped = false;
    public Text hide;
    void Start()
    {
        gameController = gameObject.GetComponent<GameHandler>();
    }

    void Update()
    {
     //A trun indicator 
     //A look into what move they chose
     //A look into what moves they didnt chose
     //A look into what they consider to be the worst play the Player can do / the best move for the player (button to request this)
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
}
