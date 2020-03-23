using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public List<GameHandler.Cards> Selected = new List<GameHandler.Cards>();
    public enum HandLocations {pHand, pTop, pBot, eHand, eTop, eBot, bPile, pile, deckLoc };
}
