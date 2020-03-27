using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pile : MonoBehaviour
{
    public PlayerController selected;
    public GameHandler gameController;
    public GameObject Text;
    public List<GameHandler.Cards> aiSelected = new List<GameHandler.Cards>();
    public List<int> SequenceSpecial = new List<int>();
    public class Board
    {
        public Board(GameObject space, GameObject text)
        {
            Text = text;
            board = space;
            board.GetComponent<HorizontalLayoutGroup>().spacing = spacing;
        }
        public void adjust()
        {
            this.board.GetComponent<HorizontalLayoutGroup>().spacing = this.spacing;
            this.board.GetComponent<HorizontalLayoutGroup>().spacing = this.spacing - (this.cardsOnTheBoard.Count);
        }
        public void changeSize()
        {
            Text.GetComponent<Text>().text = cardsOnTheBoard.Count.ToString();
        }

        public Board clone(List<GameHandler.Cards> boardCards)
        {
            Board temp = new Board(board, Text);
            for (int i= 0; i < boardCards.Count; i++)
            {
                temp.cardsOnTheBoard.Add(boardCards[i]);
            }
            return temp;
        }

        public GameObject board;
        public bool higherThan = true;
        public GameObject Text;
        public int spacing = -100;
        public List<GameHandler.Cards> cardsOnTheBoard = new List<GameHandler.Cards>();
    }
    public Board gameBoard;
    void Start()
    { 
        gameBoard = new Board(gameObject, Text);
        selected = GameObject.FindGameObjectWithTag("Controller").GetComponent<PlayerController>();
        gameController = GameObject.FindGameObjectWithTag("Controller").GetComponent<GameHandler>();
    }
    public void changeSize()
    {
        Text.GetComponent<Text>().text = gameBoard.cardsOnTheBoard.Count.ToString();
    }
    public void pickUpPile()
    {
        print("I cant go, I will pick up");
        for (int i = 0; i < gameBoard.cardsOnTheBoard.Count; i++)
        {
            gameBoard.cardsOnTheBoard[i].value = gameBoard.cardsOnTheBoard[i].card.GetComponent<CardFlip>().value;
            gameController.DrawToHand(gameBoard.cardsOnTheBoard[i], gameController.Locations[(int)PlayerController.HandLocations.eHand], (int)PlayerController.HandLocations.eHand);
        }
        for(int i =0; i < gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Count; i++)
        {
            gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[i].card.GetComponent<CardFlip>().locked = true;
        }
        gameBoard.cardsOnTheBoard.Clear();
        changeSize();
        if(gameController.pTurn == true)
        {
            gameController.endTurn();
        } else
        {
            //Ai end turn
        }
    }
    public void pickUpPileButton()
    {
        //check if its the players turn
        if (gameController.pTurn == true)
        {
            for(int i = 0; i < gameBoard.cardsOnTheBoard.Count; i++)
            { 
                gameBoard.cardsOnTheBoard[i].value = gameBoard.cardsOnTheBoard[i].card.GetComponent<CardFlip>().value;
                gameBoard.cardsOnTheBoard[i].card.GetComponent<BoxCollider2D>().enabled = true;
                gameController.DrawToHand(gameBoard.cardsOnTheBoard[i], gameController.Locations[(int)PlayerController.HandLocations.pHand], (int)PlayerController.HandLocations.pHand); ;
            }
            gameBoard.cardsOnTheBoard.Clear();
            changeSize();
            selected.Selected.Clear();
            gameController.endTurn();
        }
        else
        {
            print("Please Wait Until your turn");
        }
    }
    void placeSubFunct(GameHandler.Cards card)
    {
        //Flip the card if it needs to be flipped 
        if (card.bottomCard)
        {
            card.card.GetComponent<CardFlip>().flipped = false;
        }
        //Add the card to the board and change the location
        gameBoard.cardsOnTheBoard.Add(card);
        card.location = (int)PlayerController.HandLocations.pile;
        card.card.transform.SetParent(gameBoard.board.transform);
        //Change the Z values to stop Z fighting
        card.card.transform.position = gameBoard.board.transform.position - new Vector3(0, 0, gameBoard.cardsOnTheBoard.Count + 0.2f);
        //Adjust the spacing
        gameBoard.adjust();
        //Remove the card from where ever it came from
        gameController.Locations[(int)PlayerController.HandLocations.pHand].cardsInHand.Remove(card);
        gameController.Locations[(int)PlayerController.HandLocations.pTop].cardsInHand.Remove(card);
        gameController.Locations[(int)PlayerController.HandLocations.pBot].cardsInHand.Remove(card);
        gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Remove(card);
        gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand.Remove(card);
        gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand.Remove(card);
        //Visually show the ammount of cards on the board
        gameBoard.changeSize();
        //End the turn if selected only has one card in it otherwise there is more to come
        if (selected.Selected.Count != 0 && selected.Selected[selected.Selected.Count-1] == card || aiSelected.Count != 0 && aiSelected[aiSelected.Count-1] == card)
        {
            //before ending the trun check to see if the previous 4 cards are the same value, if so burn
            if(gameBoard.cardsOnTheBoard.Count >= 4)
            {
                if (gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value == gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 2].value 
                    && gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 3].value == gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 4].value
                    && gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value == gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 3].value
                    && gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value == gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 4].value
                    && gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 2].value == gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 3].value
                    && gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 2].value == gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 4].value)
                {
                    int howManyThrees = 0;
                    List<GameHandler.Cards> maybeBurn = new List<GameHandler.Cards>();
                    //check if they are originaly there values
                    for (int i = 1; i < 5; i++)
                    {
                       if(gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - i].card.GetComponent<CardFlip>().value == 3)
                       {
                            howManyThrees++;
                       } else
                       {
                            maybeBurn.Add(gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - i]);
                       }
                    }
                    if (howManyThrees == 0)
                    {
                        gameController.burn(gameBoard, false);
                    } else if (howManyThrees == 4 && maybeBurn.Count == 0)
                    {
                        //it is just four threes
                        gameController.burn(gameBoard, false);
                    }
                    else
                    {
                        //Check can we go back far enough
                        if(gameBoard.cardsOnTheBoard.Count >= 4 + howManyThrees)
                        {
                            // make a new list containing all the original ones, then go howManyTrees deeper to try and find mathing ones
                            //If one of them matches, and is not a three, add it to the list, if it is a three increase howManyThrees
                            //If one doesnt, break out
                            bool burn = false;
                            int checker = gameBoard.cardsOnTheBoard.Count - 5;
                            for (int i = 0; i < howManyThrees; i++)
                            {
                                if(gameBoard.cardsOnTheBoard[checker - i].value == maybeBurn[0].value)
                                {
                                    if (gameBoard.cardsOnTheBoard[checker - i].card.GetComponent<CardFlip>().value != 3)
                                    {
                                        maybeBurn.Add(gameBoard.cardsOnTheBoard[checker - i]);
                                        if(maybeBurn.Count == 4)
                                        {
                                            burn = true;
                                            break;
                                        }
                                    } else
                                    {
                                        howManyThrees++;
                                    }
                                } else
                                {
                                    break;
                                } 
                            }
                            if (burn)
                            {
                                for(int i = 0; i < maybeBurn.Count; i++)
                                {
                                    print(maybeBurn[i].value);
                                }
                                print("Special Burn");
                                gameController.burn(gameBoard, false);
                            }
                        } else
                        {
                            //There is not enough room in the board to have 4 of a kind 
                        }
                    }
                }
            }
            gameController.endTurn();
        }
    }
    void specialCardPlay(GameHandler.Cards card)
    {
        GameHandler.SpecialCards temp = new GameHandler.SpecialCards(card.value, card.special);
        temp.Effect(gameBoard,0);
    }
    public void bottomCardPlay(GameHandler.Cards card)
    {
        card.card.GetComponent<CardFlip>().flipped = false;
        card.bottomCard = false;
        if(card.location == 5 || card.location == 4)
        {
            gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand.Remove(card);
            gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand.Remove(card);
            gameController.DrawToHand(card, gameController.Locations[(int)PlayerController.HandLocations.eHand], (int)PlayerController.HandLocations.eHand);
            pickUpPile();
            
        } else
        {
            gameController.Locations[(int)PlayerController.HandLocations.pTop].cardsInHand.Remove(card);
            gameController.Locations[(int)PlayerController.HandLocations.pBot].cardsInHand.Remove(card);
            gameController.DrawToHand(card, gameController.Locations[(int)PlayerController.HandLocations.pHand], (int)PlayerController.HandLocations.pHand);
            pickUpPileButton();
        }
    }
    void SequenceCheck(GameHandler.Cards card)
    {
        if (gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value != 7)
        { 
            gameBoard.higherThan = true;
        }
        if (gameBoard.higherThan)
        {
            if (gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value <= card.value) // if the card on the boards value is lower than the card thats being added
            {
                placeSubFunct(card); // can be played 
                if (card.special)
                { 
                    specialCardPlay(card);
                }
            }
            else
            {
                if (card.special)
                {
                    print("This card has to be played in sequence"); // cant be played
                }
                else
                {
                    print("This card has a lower value");
                }
                if (card.bottomCard) //the card was on the bottom
                {
                    bottomCardPlay(card);
                }
            }
        } else // flipped checks for a seven
        {
            if (gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value >= card.value) // if the card on the boards value is lower than the card thats being added
            {
                placeSubFunct(card); // can be played 
                if (card.special)
                {
                    specialCardPlay(card);
                }
            }
            else
            {
                if (card.special)
                {
                    print("This card has to be played in sequence"); // cant be played
                }
                else
                {
                    print("This card has a higher value, needs to be lower than a seven");
                }
                if (card.bottomCard) //the card was on the bottom
                {
                    bottomCardPlay(card);
                }
            }
        }
       
    }
    public void placeToPile(GameHandler.Cards card)
    {
        if (gameBoard.cardsOnTheBoard.Count == 0) { // if there are no cards on the board simply add the card
            placeSubFunct(card);
            if (card.value == 10)
            {
                if (selected.Selected.Count != 0 && selected.Selected[selected.Selected.Count - 1] == card || aiSelected.Count != 0 && aiSelected[aiSelected.Count - 1] == card) // if its the last one
                {
                    specialCardPlay(card);
                }
            }
            else
            {
                specialCardPlay(card);
            }
        } 
        else
        {
            if (card.special == true)// check to see if the card is a special card, and if so then do the effect and add the card, unless it has to be played in sequence
            {
                bool Seq = false;
                for (int i = 0; i < SequenceSpecial.Count; i++)
                {
                    if (SequenceSpecial[i] == card.value)
                    {
                        Seq = true;
                    }
                }
                if(Seq == false)
                {
                    // can play it any where and trigger the effect
                    placeSubFunct(card);
                    if (card.value == 10)
                    {
                        if (selected.Selected.Count != 0 && selected.Selected[selected.Selected.Count - 1] == card || aiSelected.Count != 0 && aiSelected[aiSelected.Count - 1] == card) // if its the last one
                        {
                            specialCardPlay(card);
                        }
                    }
                    else
                    {
                        specialCardPlay(card);
                    }
                }
                else
                {
                    // it is special but still has to be played in sequence
                    // So check if you can actually play it, and if you can trigger the effect
                    //Also check if a seven has been played last, if so swap the order of play
                   
                    SequenceCheck(card);
                }
            } else
            {
                SequenceCheck(card);
            }
        }
    }
    public void AIChoice(List<GameHandler.Cards> Selected)
    {
        aiSelected = Selected;
        if (Selected.Count != 0)
        {
            for (int i = 0; i < Selected.Count; i++)
            {
                placeToPile(Selected[i]);
            }
            Selected.Clear();
        }
    }

    void OnMouseDown()
    {
       if(gameController.PlayerReady != false)
       {
            if (selected.Selected.Count != 0)
            {
                for (int i = 0; i < selected.Selected.Count; i++)
                {
                    placeToPile(selected.Selected[i]);
                    selected.Selected[i].card.GetComponent<SpriteRenderer>().material.shader = gameController.shader1;
                }
                selected.Selected.Clear();
            }
            else
            {
                Debug.Log("Please select a card first");
            }
       }
    }
}
