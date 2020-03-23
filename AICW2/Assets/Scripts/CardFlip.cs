using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlip : MonoBehaviour
{
    public Sprite MySprite;
    public Sprite Back;
    public bool flipped = false;
    public bool locked = false;
    public int value;
    public bool special;
    public GameHandler.Cards current;

    public PlayerController selected;
    public GameHandler swaper;
    Shader shader2;
    Shader shader1;
    Renderer rend;
    BoxCollider2D col;

    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        shader2 = Shader.Find("Sprites/Outline");
        shader1 = Shader.Find("Sprites/Default");
        col = GetComponent<BoxCollider2D>();

        selected = GameObject.FindGameObjectWithTag("Controller").GetComponent<PlayerController>();
        swaper = GameObject.FindGameObjectWithTag("Controller").GetComponent<GameHandler>();
        //flipped = false;
        //locked = false;
        MySprite = this.GetComponent<SpriteRenderer>().sprite;
        Back = GameObject.FindGameObjectWithTag("Back").GetComponent<SpriteRenderer>().sprite;
        
    }

    void Update()
    {
        if (flipped)
        {
            this.GetComponent<SpriteRenderer>().sprite = Back;
        } else
        {
            this.GetComponent<SpriteRenderer>().sprite = MySprite;
        }
        if (current.location == (int)PlayerController.HandLocations.pile) // we are in the pile
        {
            col.enabled = false;
        }
    }
    void OnMouseDown()
    {
        if (current.location != (int)PlayerController.HandLocations.deckLoc && swaper.pTurn != false) // dont want the player to select cards from the deck or select cards when its not their turn
        {
            if (swaper.PlayerReady != true) // if the player hasnt pressed ready yet
            {
                if (!locked) // instead disable colliders if loc is any other than the players
                {
                    if (selected.Selected.Count == 0)
                    {
                        selected.Selected.Add(current);
                        rend.material.shader = shader2;
                    }
                    else
                    {
                        if (selected.Selected[0].location == current.location)
                        {
                            if (selected.Selected[0].card == gameObject)
                            {
                                rend.material.shader = shader1;
                                selected.Selected.Clear();
                            }
                        }
                        else
                        {
                            selected.Selected.Add(current);
                            rend.material.shader = shader2;
                            swaper.swap(selected.Selected[0], selected.Selected[1], selected.Selected[0].location, selected.Selected[1].location);
                            selected.Selected.Clear();
                        }
                    }
                }
            }
            else // after the player has pressed ready
            {
                if (!locked)
                {
                    if (selected.Selected.Count == 0) // if nothing is selected
                    {
                        selected.Selected.Add(current);
                        rend.material.shader = shader2;
                    }
                    else
                    {
                        if (selected.Selected[0].location == current.location) // if they have selected a card inside the other cards location
                        {
                            if (selected.Selected[0].card == gameObject)
                            {
                                rend.material.shader = shader1;
                                selected.Selected.Clear();
                            } else if (selected.Selected[0].value == current.value) // allowing the player to plaay multiple cards
                            {
                                selected.Selected.Add(current);
                                rend.material.shader = shader2;
                            }
                        }
                    }
                }
            }
        }
    }
}
