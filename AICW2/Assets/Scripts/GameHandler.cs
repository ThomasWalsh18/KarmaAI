using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public Deck deckStorage;

    public int drawAmmount = 3;
    public int downAmmount = 3;

    public bool skipAI = false;

    public Storage store;
    public PlayerController controller;

    public GameObject deckVisual;
    public GameObject howManyLeft;
    public GameObject startButton;
    public GameObject ReadyButton;
    public GameObject PickUpButton;

    public GameObject BurnCanvas;

    public GameObject burnPile;
    public Hand burnPileCode;

    public GameObject[] playerSpace;
    public GameObject[] enemySpace;
    public List<Hand> Locations = new List<Hand>();

    public bool PlayerReady;
    public bool pTurn;

    public Shader shader1;
    public class Cards 
    {
        public Cards()
        {
            handler = GameObject.FindGameObjectWithTag("Controller").GetComponent<GameHandler>();
        }
        public Cards(int Value, bool Special)
        {
            value = Value;
            special = Special;
            handler = GameObject.FindGameObjectWithTag("Controller").GetComponent<GameHandler>();
        }
        public virtual void Effect(Pile.Board gameBoard, int depth) {}
        public int value;
        public bool special;
        public GameObject card;
        public int location;
        public bool bottomCard = false;
        public GameHandler handler;
    }
    public class SpecialCards : Cards
    {
        public SpecialCards(int Value, bool Special)
        {
            special = Special;
            value = Value;
        }
        public override void Effect(Pile.Board gameBoard, int depth)
        {
            switch (value) {
                case 3:
                    if (depth <= gameBoard.cardsOnTheBoard.Count - 1)
                    {
                        //Higher than the last card played
                        if (gameBoard.cardsOnTheBoard.Count != 1)
                        {
                            gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value = gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 2].value;

                            if (gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 2].special)
                            {
                                GameHandler.SpecialCards temp = new GameHandler.SpecialCards(gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 2].value, true);
                                temp.Effect(gameBoard, depth + 1);
                            }
                        }
                    }
                    break;
                case 7:
                    //lower than a seven
                    gameBoard.higherThan = false;
                    break;
                case 8:
                    //Extra turn
                    //two or 4 at the same time will make nothing happen, but three will trigger or one 
                    if (!GameObject.FindGameObjectWithTag("Controller").GetComponent<GameHandler>().pTurn)
                    {
                        handler.skipAI = !handler.skipAI;
                    } else
                    {
                        print("Skip Player");
                        GameObject.FindGameObjectWithTag("Controller").GetComponent<AI>().skipPlayer = true;
                    }
                    break;
                case 10:
                    //Make this a funct for when we call the burn for 4 of a kind
                    //Trigger Extra turn
                    handler.burn(gameBoard);
                    break;
            }
        }
    }

    public class Deck
    {
        public Deck(GameObject left, GameObject visual)
        {
           Left = left;
           Visual = visual;
        }
        public List<Cards> deck = new List<Cards>();
        public GameObject Left;
        public GameObject Visual;
        public void changeAmmount()
        {
            Left.GetComponent<Text>().text = deck.Count.ToString();
            if (deck.Count == 0)
            {
                Visual.transform.position += new Vector3(100, 100, 0);
                //Visual.SetActive(false);
            }
        }
        public void shuffle()
        {
            for (int i = 0; i < deck.Count; i++)
            {
                Cards temp = deck[i];
                int randomIndex = Random.Range(i, deck.Count);
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }
    }

    public class Hand
    {
        public Hand(GameObject space)
        {
            hand = space;
        }
        public void adjustHand()
        {
            this.hand.GetComponent<HorizontalLayoutGroup>().spacing = 0;
            this.hand.GetComponent<HorizontalLayoutGroup>().spacing = this.spacing - (this.cardsInHand.Count);
        }
        public Hand clone(List<Cards> handCards)
        {
            Hand temp = new Hand(hand);
            for (int i= 0; i< handCards.Count; i++)
            {
                temp.cardsInHand.Add(handCards[i]);
            }
            return temp;
        }
        public GameObject hand;
        public int spacing = 0;
        public List<Cards> cardsInHand = new List<Cards>();
    }
    public void burn(Pile.Board gameBoard)
    {
        for (int i = 0; i < gameBoard.cardsOnTheBoard.Count; i++)
        {
            gameBoard.cardsOnTheBoard[i].value = gameBoard.cardsOnTheBoard[i].card.GetComponent<CardFlip>().value;
            gameBoard.cardsOnTheBoard[i].card.SetActive(false);
            DrawToHand(gameBoard.cardsOnTheBoard[i], Locations[(int)PlayerController.HandLocations.bPile], (int)PlayerController.HandLocations.bPile);
        }
        gameBoard.cardsOnTheBoard.Clear();
        gameBoard.changeSize();
        if (!GameObject.FindGameObjectWithTag("Controller").GetComponent<GameHandler>().pTurn)
        {
            skipAI = !skipAI;
        }
        else
        {
            print("Skip Player");
            GameObject.FindGameObjectWithTag("Controller").GetComponent<AI>().skipPlayer = true;
        }
    }
    public void LockedControl()
    {
        if (Locations[(int)PlayerController.HandLocations.pHand].cardsInHand.Count == 0) // if we have no cards in hand
        {
            if (Locations[(int)PlayerController.HandLocations.pTop].cardsInHand.Count == 0) // if we have to cards on top
            {
                if (Locations[(int)PlayerController.HandLocations.pBot].cardsInHand.Count == 0)// no cards left at all
                {
                    print("Win State"); //player has won
                }
                else
                {
                    for (int i = 0; i < Locations[(int)PlayerController.HandLocations.pBot].cardsInHand.Count; i++)
                    {
                        Locations[(int)PlayerController.HandLocations.pBot].cardsInHand[i].card.GetComponent<CardFlip>().locked = !pTurn;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Locations[(int)PlayerController.HandLocations.pTop].cardsInHand.Count; i++)
                {
                    Locations[(int)PlayerController.HandLocations.pTop].cardsInHand[i].card.GetComponent<CardFlip>().locked = !pTurn;
                }
            }
        }
        else
        {
            for (int i = 0; i < Locations[(int)PlayerController.HandLocations.pHand].cardsInHand.Count; i++)
            {
                Locations[(int)PlayerController.HandLocations.pHand].cardsInHand[i].card.GetComponent<CardFlip>().locked = !pTurn;
            }
        }
    }
    public void endTurn()
    {
        //Draw back up if able and elegable
        if (deckStorage.deck.Count != 0) // if there is cards in the deck then draw the hands back up to 3
        {
            while(Locations[(int)PlayerController.HandLocations.pHand].cardsInHand.Count < downAmmount && deckStorage.deck.Count != 0)
            {
                DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.pHand], (int)PlayerController.HandLocations.pHand);
            }
            while (Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Count < downAmmount && deckStorage.deck.Count != 0)
            {
                DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.eHand], (int)PlayerController.HandLocations.eHand);
            }
        } else // the deck is empty
        {
            print("The Deck is empty");
        }
        // swap turns 
        pTurn = !pTurn;
        LockedControl();
    }
    public void SwapBrunPile()
    {
        for (int i = 0; i < burnPileCode.cardsInHand.Count; i++)
        {
            burnPileCode.cardsInHand[i].card.SetActive(!burnPileCode.cardsInHand[i].card.activeSelf);
        }
        BurnCanvas.GetComponent<Canvas>().enabled = !BurnCanvas.GetComponent<Canvas>().enabled;
    }
    public void updateHandZPos(Hand hand)
    {
        //reset all Z pos
        for (int i = 0; i < hand.cardsInHand.Count; i++)
        {
            hand.cardsInHand[i].card.transform.position = hand.hand.transform.position - new Vector3(0, 0,i + 0.2f);
        }
        hand.adjustHand();
    }
    public void swap(Cards cardOne, Cards cardTwo, int start, int finish)
    {
        //Swap shaders back
        cardOne.card.GetComponent<SpriteRenderer>().material.shader = shader1;
        cardTwo.card.GetComponent<SpriteRenderer>().material.shader = shader1;
        //Change locations 
        cardOne.location = finish;
        cardTwo.location = start;
        //Update hands
        Locations[start].cardsInHand.Remove(cardOne);
        Locations[start].cardsInHand.Add(cardTwo);
        Locations[finish].cardsInHand.Add(cardOne);
        Locations[finish].cardsInHand.Remove(cardTwo);
        //Move Cards and adjust z value if adding too the hand
        if (start == (int)PlayerController.HandLocations.pHand)
        {
            cardTwo.card.transform.SetParent(Locations[start].hand.transform);
            //cardTwo.card.transform.position = playerLocation[start].hand.transform.position - new Vector3(0, 0, playerLocation[0].cardsInHand.Count - 10);
        } else
        {
            cardTwo.card.transform.SetParent(Locations[start].hand.transform);
            cardTwo.card.transform.position = Locations[start].hand.transform.position;
        }
        if (finish == (int)PlayerController.HandLocations.pHand)
        {
            cardOne.card.transform.SetParent(Locations[finish].hand.transform);
            //cardOne.card.transform.position = playerLocation[finish].hand.transform.position - new Vector3(0, 0, playerLocation[0].cardsInHand.Count - 10);
        } else
        {
            cardOne.card.transform.SetParent(Locations[finish].hand.transform);
            cardOne.card.transform.position = Locations[finish].hand.transform.position;
        }
        updateHandZPos(Locations[start]);
        updateHandZPos(Locations[finish]);
    }
    void addCardGameObject(Deck deck)
    {
        List<Cards> temp = new List<Cards>();
        for (int i = 0; i < store.cards.Count; i++)
        {
            Cards newCard = new Cards(store.cards[i].GetComponent<CardFlip>().value, store.cards[i].GetComponent<CardFlip>().special);
            GameObject cardClone = Instantiate(store.cards[i], deckVisual.transform.position + new Vector3(0, 0, 10), Quaternion.identity);
            newCard.location = (int)PlayerController.HandLocations.deckLoc; // set location to the deck so the player cant select the card
            cardClone.GetComponent<CardFlip>().current = newCard;
            newCard.card = cardClone;
            newCard.card.transform.SetParent(deckVisual.transform);
            temp.Add(newCard);
        }
        deck.deck = temp;
    }

    public void DrawToHand(Cards card, Hand plane, int loc)
    {
        card.location = loc;
        plane.cardsInHand.Add(card);
        if (loc == (int)PlayerController.HandLocations.pHand)
        {
            //for (int i = 0; i < plane.cardsInHand.Count; i++)
            //{
            //    plane.cardsInHand[i].card.transform.position = plane.hand.transform.position - new Vector3(0, 0, 10);
            //    //plane.cardsInHand[i].card.transform.position -= new Vector3(0, 0, plane.cardsInHand.Count - 10);
            //}
        } else
        {
            if (loc == (int)PlayerController.HandLocations.bPile)
            {
                card.card.transform.position = plane.hand.transform.position - new Vector3(0,0, plane.cardsInHand.Count);
            } else
            {
                card.card.transform.position = plane.hand.transform.position;
            }
        }
        card.card.transform.SetParent(plane.hand.transform);
        if (deckStorage.deck.Count != 0)
        {
            deckStorage.deck.Remove(card);
        }
        deckStorage.changeAmmount();
        updateHandZPos(plane);
        plane.adjustHand();
    }

    public void startGame()
    {
        PlayerReady = false;
        pTurn = true;
        //Draw all the cards
        for (int i= 0; i < downAmmount; i++) 
        {
            DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.pHand], (int)PlayerController.HandLocations.pHand);
            DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.pTop], (int)PlayerController.HandLocations.pTop);
            DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.pBot], (int)PlayerController.HandLocations.pBot);
            DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.eHand], (int)PlayerController.HandLocations.eHand);
            DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.eTop], (int)PlayerController.HandLocations.eTop);
            DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.eBot], (int)PlayerController.HandLocations.eBot);
        }
        //Hide all the hidden cards and lock them
        for (int i = 0; i < downAmmount; i++)
        {
            Locations[(int)PlayerController.HandLocations.pBot].cardsInHand[i].card.GetComponent<CardFlip>().flipped = true;
            Locations[(int)PlayerController.HandLocations.pBot].cardsInHand[i].card.GetComponent<CardFlip>().locked = true;
            Locations[(int)PlayerController.HandLocations.eBot].cardsInHand[i].card.GetComponent<CardFlip>().flipped = true;
            Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[i].bottomCard = false;
            Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[i].bottomCard = true;
            Locations[(int)PlayerController.HandLocations.eBot].cardsInHand[i].bottomCard = true;
            Locations[(int)PlayerController.HandLocations.pTop].cardsInHand[i].bottomCard = true;
            Locations[(int)PlayerController.HandLocations.pBot].cardsInHand[i].bottomCard = true;

        }
        //lock all the untouchable cards
        //...
        for (int i = 0; i < downAmmount; i++)
        {
            Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[i].card.GetComponent<CardFlip>().locked = true;
            Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[i].card.GetComponent<CardFlip>().locked = true;
            Locations[(int)PlayerController.HandLocations.eBot].cardsInHand[i].card.GetComponent<CardFlip>().locked = true;
        }
        startButton.SetActive(false);
        ReadyButton.SetActive(true);
    }
    public void Ready()
    {
        controller.GetComponent<AI>().startGame();
        for (int i = 0; i < 2; i++)
        {
            for(int j = 0; j < downAmmount; j++)
            {
                Locations[i].cardsInHand[j].card.GetComponent<SpriteRenderer>().material.shader = shader1;
                Locations[i].cardsInHand[j].card.GetComponent<SpriteRenderer>().material.shader = shader1;
            }

            PlayerReady = true;
        }
        controller.Selected.Clear();
        for (int i = 0; i < downAmmount; i++)
        {
            Locations[(int)PlayerController.HandLocations.pTop].cardsInHand[i].card.GetComponent<CardFlip>().locked = true;
        }
        ReadyButton.SetActive(false);
        PickUpButton.SetActive(true);

        float rnd = Random.Range(0, 101);
        //Random Who goes first
        if (rnd <= 50)
        {
            pTurn = false;
        }
        else
        {
            pTurn = true;
        }
    }

    public void Start()
    {
        skipAI = false;

        shader1 = Shader.Find("Sprites/Default");

        controller = gameObject.GetComponent<PlayerController>();
        store = GetComponent<Storage>();
        //make the deck
        deckStorage = new Deck(howManyLeft, deckVisual);

        //make all the cards and add them to the deck, then shuffle
        addCardGameObject(deckStorage);
        deckStorage.shuffle();
        deckStorage.changeAmmount();

        //make the hands
        Locations.Add(new Hand(playerSpace[0]));
        Locations.Add(new Hand(playerSpace[1]));
        Locations.Add(new Hand(playerSpace[2]));

        Locations.Add(new Hand(enemySpace[0]));
        Locations.Add(new Hand(enemySpace[1]));
        Locations.Add(new Hand(enemySpace[2]));

        //make the burn pile
        burnPileCode = new Hand(burnPile);
        Locations.Add(burnPileCode);


        while (deckStorage.deck.Count > 52 - drawAmmount)
        {
            //deckStorage.deck[0].card.SetActive(false);
            DrawToHand(deckStorage.deck[0], Locations[(int)PlayerController.HandLocations.pHand], (int)PlayerController.HandLocations.pHand);
        }

    }

    void Update()
    {

    }
}