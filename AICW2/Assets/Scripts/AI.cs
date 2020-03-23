using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public enum moveList { Two, empty, MultipleCards, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace, PickUpPile }
    public class boardState
    {
        public GameHandler.Hand currentHand;
        public GameHandler.Hand currentTop;
        public GameHandler.Hand currentBot;
        public Pile.Board gameBoard;
        public int playersHand;
        public int deckSize;

        public boardState(GameHandler.Hand eHand, GameHandler.Hand eTop, GameHandler.Hand eBot, int pHandSize, Pile.Board currentBoard, int deck)
        {
            currentTop = eTop;
            currentBot = eBot;
            currentHand = eHand;
            playersHand = pHandSize;
            gameBoard = currentBoard;
            deckSize = deck;
        }
        public boardState clone(GameHandler.Hand Hand, GameHandler.Hand Top, GameHandler.Hand Bot, int HandSize, Pile.Board Board, int Deck)
        {
            boardState temp = new boardState(Hand, Top, Bot, HandSize, Board, Deck);
            return temp;
        }
        public int evaluate()
        {
            //evaluate the current board state to give the node its value
            
            return 0;
        }
    }

    boardState currentBoardState;

    public class node
    {
        public node(bool myOpponentsChoice, node before, boardState state, moveList currentMove)
        {
            playersMove = myOpponentsChoice;
            previous = before;
            currentBoard = state;
            moveChoice = currentMove;
        }

        public void addNode(bool myOpponentsChoice, node previous, boardState predicted, moveList currentMove)
        {
            node child = new node(myOpponentsChoice, previous, predicted, currentMove);
            children.Add(child);
        }

        public node clone(bool myOpponents, node prev, boardState predict, moveList Move)
        {
            node temp = new node(myOpponents, prev, predict, Move);
            return temp;
        }

        public bool playersMove;
        public int value;
        public boardState currentBoard;
        public moveList moveChoice;
        public node previous;
        public List<node> children = new List<node>();
    }
    public class tree
    {
        public tree(node root)
        {
            Root = root;
        }
        public List<node> children = new List<node>();

        public node Root;
    }

    private GameHandler gameController;
    public int maxDepth;
    public Pile pile;

    void Start()
    {
        gameController = GetComponent<GameHandler>();
        pile = GameObject.FindGameObjectWithTag("Board").GetComponent<Pile>();
    }

    void EndTurn()
    {
        gameController.pTurn = true;
        gameController.LockedControl();
        gameController.skipAI = false;
    }
    boardState makeBoardState(PlayerController.HandLocations hand, moveList move, int howMany, boardState current, Pile.Board gameBoard)
    {
        GameHandler.Hand tempHand = current.currentHand.clone(current.currentHand.cardsInHand);
        GameHandler.Hand tempTop = current.currentTop.clone(current.currentTop.cardsInHand);
        GameHandler.Hand tempBot = current.currentBot.clone(current.currentBot.cardsInHand);

        GameHandler.Hand affected = null;

        if (hand == PlayerController.HandLocations.eHand) // using reference types to my advantage
        {
            affected = tempHand;
        }
        else if(hand == PlayerController.HandLocations.eTop)
        {
            affected = tempTop;
        }
        else if(hand == PlayerController.HandLocations.eBot)
        {
            affected = tempBot;
        } else
        {
            print("Wrong Enum");
        }

        int newDeckSize = current.deckSize;
        Pile.Board newBoard = gameBoard.clone(gameBoard.cardsOnTheBoard);
        int checker = 0;
        if (move != moveList.PickUpPile)
        {
            for (int i = 0; i < affected.cardsInHand.Count; i++)
            {
                if(affected.cardsInHand[i] != null) // to check the new cards that we drew
                {
                    if (checker != howMany)
                    {
                        if (affected.cardsInHand[i].value == (int)move)
                        {
                            newBoard.cardsOnTheBoard.Add(affected.cardsInHand[i]);
                            switch (affected.cardsInHand[i].value)
                            {
                                case 3:
                                    if(newBoard.cardsOnTheBoard.Count < 1)
                                    {
                                        newBoard.cardsOnTheBoard[newBoard.cardsOnTheBoard.Count - 1].value = 
                                            newBoard.cardsOnTheBoard[newBoard.cardsOnTheBoard.Count - 2].value;
                                    }
                                    break;
                                case 7:
                                    //just add the seven check into the evaluate
                                    break;
                                case 8:
                                    //some value to say that they are going again 
                                    //node needs this so that i can check to change the evaluation to my turn for that nodes children
                                    checker = howMany; // play only one eight a turn
                                    break;
                                case 10:
                                    //simulate a ten to burn the pile, and set the value to go again
                                    newBoard.cardsOnTheBoard.Clear();
                                    break;
                            }
                            affected.cardsInHand.Remove(affected.cardsInHand[i]);
                            if (newDeckSize != 0) // if there are cards left to draw
                            {
                                newDeckSize--;
                                affected.cardsInHand.Add(null);
                            }
                            checker++;
                        }
                    }
                }
                else
                {
                    // remove the null card? or just dont do anything?
                }
            }
        } else
        {
            //pick up pile 
            for (int i = 0; i < newBoard.cardsOnTheBoard.Count; i++)
            {
                affected.cardsInHand.Add(newBoard.cardsOnTheBoard[0]);
                newBoard.cardsOnTheBoard.Remove(newBoard.cardsOnTheBoard[0]);
            }
        }
       
        GameHandler.Hand newEHand = tempHand.clone(tempHand.cardsInHand);
        GameHandler.Hand newETop = tempTop.clone(tempTop.cardsInHand);
        GameHandler.Hand newEBot = tempBot.clone(tempBot.cardsInHand);

        boardState temp = new boardState(newEHand, newETop, newEBot, current.playersHand, newBoard, newDeckSize);

        return temp;
    }
    void addNodeSub(node currentNode, GameHandler.Hand Hand,int i, bool player)
    {
        if (Hand.cardsInHand[i] != null)
        {
            int howMany = 0;
            //always play as many of the same card as possible, improvements for later
            for (int j = 0; j < Hand.cardsInHand.Count; j++)
            {
                if(Hand.cardsInHand[j]!= null)
                {
                    if (Hand.cardsInHand[i].value == Hand.cardsInHand[j].value)
                    {
                        howMany++;
                    }
                }
            }
            boardState prediction = makeBoardState(PlayerController.HandLocations.eHand, (moveList)Hand.cardsInHand[i].value, howMany, currentNode.currentBoard, currentNode.currentBoard.gameBoard);
            currentNode.addNode(player, currentNode, prediction, (moveList)Hand.cardsInHand[i].value);
        }
       
    }

    void addNodePre(node currentNode, GameHandler.Hand Hand, bool player)
    {
        // there is still cards in hand 
        for (int i = 0; i < Hand.cardsInHand.Count; i++)
        {
            if (Hand.cardsInHand[i] != null) // cant evaluate null cards, these are cards that we have drawn but their value is unknown
            {
                if (currentNode.currentBoard.gameBoard.cardsOnTheBoard.Count == 0)
                {
                    addNodeSub(currentNode,Hand, i, player);
                }
                else if (currentNode.currentBoard.gameBoard.cardsOnTheBoard[currentNode.currentBoard.gameBoard.cardsOnTheBoard.Count - 1].value == 7) // flip the check
                {
                    if (currentNode.currentBoard.gameBoard.cardsOnTheBoard[currentNode.currentBoard.gameBoard.cardsOnTheBoard.Count - 1].value >= Hand.cardsInHand[i].value ||
                        Hand.cardsInHand[i].special && Hand.cardsInHand[i].value != 7 &&
                        Hand.cardsInHand[i].special && Hand.cardsInHand[i].value != 8)
                    {
                        addNodeSub(currentNode, Hand, i, player);
                    }
                }
                else if (currentNode.currentBoard.gameBoard.cardsOnTheBoard[currentNode.currentBoard.gameBoard.cardsOnTheBoard.Count - 1].value <= Hand.cardsInHand[i].value ||
                   Hand.cardsInHand[i].special && Hand.cardsInHand[i].value != 7 &&
                   Hand.cardsInHand[i].special && Hand.cardsInHand[i].value != 8)
                {
                    addNodeSub(currentNode, Hand, i, player);
                }
            }
        }
        boardState pickUpPrediction = makeBoardState(PlayerController.HandLocations.eHand, moveList.PickUpPile, 0, currentNode.currentBoard, currentNode.currentBoard.gameBoard);
        currentNode.addNode(player, currentNode, pickUpPrediction, moveList.PickUpPile);
    }
    bool addMovesSub(node currentNode, bool OppsTurn)
    {
        bool won = false;
        if (currentNode.currentBoard.currentHand.cardsInHand.Count != 0) // still cards in hand
        {
            addNodePre(currentNode, currentNode.currentBoard.currentHand, OppsTurn);
        }
        else if (currentNode.currentBoard.currentTop.cardsInHand.Count != 0)
        {
            //there is still cards on top
            addNodePre(currentNode, currentNode.currentBoard.currentTop, OppsTurn);
        }
        else if (currentNode.currentBoard.currentBot.cardsInHand.Count != 0)
        {
            //chose a random card, from 0 to count and play that card
            //New function

        }
        else
        {
            //no moves avaialbe so win
            //Add a win funct that takes a bool for whether or not the player won
            print("Enemy Wins");
            won = true;
        }
        return won;
    }
    void addAllMoves(int depth, tree gameTree, node currentLevel, bool OppsTurn)
    {
        //Go through the current nodes board state and make all the available moves have nodes
        //Then add one option for picking up the pile

        //Then recurse and now change to add all the options for the player 
        //And again until the depth has hit the max depth 
        if (depth != maxDepth)
        {
            if (currentLevel.children.Count == 0)
            {
                if (currentLevel.previous == null)// then we are the root node
                {
                    if(addMovesSub(currentLevel, OppsTurn))
                    {
                        depth = maxDepth - 1;
                    }
                    //nextLevel = currentLevel.children[0].clone(currentLevel.playersMove, currentLevel, currentLevel.currentBoard, currentLevel.moveChoice);
                }
                else
                {
                    for (int i = 0; i < currentLevel.previous.children.Count; i++)
                    {
                        //currentLevel.previous.children[i].currentBoard = currentLevel.previous.children[i].currentBoard;
                        if (addMovesSub(currentLevel.previous.children[i], OppsTurn))
                        {
                            depth = maxDepth - 1;
                        }
                    }
                }
            }
            depth = depth + 1;
            addAllMoves(depth, gameTree, currentLevel.children[0], !OppsTurn);
        }

    }

    void AITrun()
    {
        print("Thinkning");
        //Save the board state
        currentBoardState = new boardState(
            gameController.Locations[(int)PlayerController.HandLocations.eHand].clone(gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand),
            gameController.Locations[(int)PlayerController.HandLocations.eTop].clone(gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand),
            gameController.Locations[(int)PlayerController.HandLocations.eBot].clone(gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand),
            gameController.Locations[(int)PlayerController.HandLocations.pHand].cardsInHand.Count,
            pile.gameBoard.clone(pile.gameBoard.cardsOnTheBoard),
            gameController.deckStorage.deck.Count
            );
        //Make a new binary tree with all the different moves possible to the player, and enemy for however long the recursive value is
        node root = new node(false, null, currentBoardState, moveList.empty);
        tree gameTree = new tree(root);
        addAllMoves(0, gameTree, gameTree.Root, false);
        //Evaluate all the different moves of the bottom level
        //Then fill out the other moves based on the children
        //Chose a move
        //Do the move
        //End turn
        EndTurn();
    }

    public void updateHandZPos(GameHandler.Hand hand)
    {
        //reset all Z pos
        for (int i = 0; i < hand.cardsInHand.Count; i++)
        {
            hand.cardsInHand[i].card.transform.position = hand.hand.transform.position - new Vector3(0, 0, i + 0.2f);
        }
        hand.adjustHand();
    }

    void swap(GameHandler.Cards cardOne, GameHandler.Cards cardTwo, int start, int finish)
    {
        //Change locations 
        cardOne.location = finish;
        cardTwo.location = start;
        //Update hands

        gameController.Locations[start].cardsInHand.Remove(cardOne);
        gameController.Locations[start].cardsInHand.Add(cardTwo);
        gameController.Locations[finish].cardsInHand.Add(cardOne);
        gameController.Locations[finish].cardsInHand.Remove(cardTwo);
        //Move Cards and adjust z value if adding too the hand
        if (start == (int)PlayerController.HandLocations.pHand)
        {
            cardTwo.card.transform.SetParent(gameController.Locations[start].hand.transform);
            //cardTwo.card.transform.position = playerLocation[start].hand.transform.position - new Vector3(0, 0, playerLocation[0].cardsInHand.Count - 10);
        }
        else
        {
            cardTwo.card.transform.SetParent(gameController.Locations[start].hand.transform);
            cardTwo.card.transform.position = gameController.Locations[start].hand.transform.position;
        }
        if (finish == (int)PlayerController.HandLocations.pHand)
        {
            cardOne.card.transform.SetParent(gameController.Locations[finish].hand.transform);
            //cardOne.card.transform.position = playerLocation[finish].hand.transform.position - new Vector3(0, 0, playerLocation[0].cardsInHand.Count - 10);
        }
        else
        {
            cardOne.card.transform.SetParent(gameController.Locations[finish].hand.transform);
            cardOne.card.transform.position = gameController.Locations[finish].hand.transform.position;
        }

        updateHandZPos(gameController.Locations[start]);
        updateHandZPos(gameController.Locations[finish]);
    }

    public void startGame()
    {
        //check each card in the top section, 
        //then if the card is not special and not a 7 or 8
        //check all the cards in your hand to see if its special and not a seven or eight
        //then if none are specail check the values for the highest card to swap
        int notSwapped = 0;
        for(int i= 0; i < gameController.downAmmount; i++)
        {
            //if its special leave it alone
            if (!gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped].special ||
                gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped].value == 7 ||
                gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped].value == 8 
            )
            {
                int current = 0;
                bool swapped = false;
                for (int j = 0; j < gameController.downAmmount; j++)
                {
                    swapped = false;
                    if (gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[j].special &&
                        gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[j].value != 7 &&
                        gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[j].value != 8
                    )
                    {

                        print("Swap" + gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[j].value + " For" + gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped].value);
                        swap(gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped],
                             gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[j],
                             (int)PlayerController.HandLocations.eTop,
                             (int)PlayerController.HandLocations.eHand
                            );
                        swapped = true;
                        break;
                    }
                    else 
                    {
                        //find the highest card
                        if (gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[j].value > 
                            gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[current].value)
                        {
                            current = j;
                        }
                       
                    }
                }
                if (!swapped)
                {
                    if (gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped].value <
                               gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[current].value)
                    {
                        print("Swap" + gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[current].value + " For" + gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped].value);
                        swap(gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[notSwapped],
                         gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand[current],
                         (int)PlayerController.HandLocations.eTop,
                         (int)PlayerController.HandLocations.eHand
                        );
                    }
                    else
                    {
                        notSwapped++;
                    }
                }
            }
            else
            {
                notSwapped++;
            }
        }
    }


    void Update()
    {
        // if it is the AI's turn
        if (gameController.pTurn == false)
        {
            //Check to see if we need to pass the turn back to the player ( if the player has just burned or played a odd number of 8's)
            if (gameController.skipAI)
            {
                //Pass the turn back
                print("Damn, you skipped me");
                EndTurn();
            }
            else
            {
                AITrun();
            }
        }
    }
}