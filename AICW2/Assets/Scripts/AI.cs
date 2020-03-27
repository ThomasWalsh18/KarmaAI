using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AI : MonoBehaviour
{
    public enum moveList { Two, empty, MultipleCards, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace, PickUpPile }
    public class boardState
    {
        public GameHandler.Hand currentHand;
        public GameHandler.Hand currentTop;
        public GameHandler.Hand currentBot;

        public int playersHand;
        public int playersTop;
        public int playersBottom;
        public Pile.Board gameBoard;
        public int deckSize;
        public moveList Move;
        public bool evaluated = false;

        public boardState(GameHandler.Hand eHand, GameHandler.Hand eTop, GameHandler.Hand eBot, int pHand, int pTop, int pBot, Pile.Board currentBoard, int deck, moveList lastMove)
        {
            currentTop = eTop;
            currentBot = eBot;
            currentHand = eHand;
            playersHand = pHand;
            playersTop = pTop;
            playersBottom = pBot;
            gameBoard = currentBoard;
            deckSize = deck;
            Move = lastMove;
        }
        public boardState clone(GameHandler.Hand Hand, GameHandler.Hand Top, GameHandler.Hand Bot, int pHand, int pTop, int pBot, Pile.Board Board, int Deck, moveList lastMove)
        {
            boardState temp = new boardState(Hand, Top, Bot, pHand, pTop, pBot, Board, Deck, lastMove);
            return temp;
        }

        int defensive(GameHandler.Hand hand, int evalue, moveList actualMove)
        {
            print("defence");
            int lowestVal = int.MaxValue;
            for (int i = 0; i < hand.cardsInHand.Count; i++)
            {
                if (hand.cardsInHand[i] != null) // card we have drawn cant be evaluated
                {
                    if (hand.cardsInHand[i].value < lowestVal)
                    {
                        lowestVal = hand.cardsInHand[i].value;
                    }
                    if (hand.cardsInHand[i].special) // if we have any special cards, we get rewarded
                    {
                        if (hand.cardsInHand[i].value == 7 || hand.cardsInHand[i].value == 8) // but if we have the sequence ones we get punished
                        {
                            evalue -= (14 - hand.cardsInHand[i].value);
                        }
                        else
                        {
                            evalue += 30;
                        }
                    }
                    else if (hand.cardsInHand[i].value < 11)
                    {
                        evalue -= (14 - hand.cardsInHand[i].value); //Punished for having higher cards in hand
                    }
                    else
                    {
                        evalue += (14 + hand.cardsInHand[i].value); // Rewarded for having lower costed cards in hand
                    }
                } else
                {
                    //Random cards get highly rewarded, encouraging the playing of multiple cards
                    evalue += 10;
                }
            }
            if((int)actualMove == lowestVal)
            {
                //Actually playing the lowest card in the hand
                evalue += 15;
            }
            return evalue;
        }
        int aggressive(GameHandler.Hand hand, int evalue, moveList actualMove)
        {
            print("Aggro");
            //How many moves did i cut off
            //The less moves availabe will mean more points
            int movesAvailalbe = 0;
            int starter = 0;
            if (gameBoard.cardsOnTheBoard.Count == 0)
            {
                //Player can do any move
                for (int i = 0; i < 15; i++)
                {
                    if (i == 1 || i == 2)
                    {

                    }
                    else
                    {
                        movesAvailalbe++;
                    }
                }
            }
            else
            {
                starter = gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value;
                if (starter == 7)
                {
                    //flip the checks
                    for (int i = starter; i >= 0; i--)
                    {
                        if (i == 1 || i == 2)
                        {

                        }
                        else
                        {
                            movesAvailalbe++;
                        }
                    }
                }
                else
                {
                    for (int i = starter; i < 15; i++)
                    {
                        if (i == 1 || i == 2)
                        {

                        }
                        else
                        {
                            movesAvailalbe++;
                        }
                    }
                }
            }
            if (starter > 0)
            {
                if (starter != 7)
                {
                    movesAvailalbe++;
                }
            }
            if (starter > 3)
            {
                if (starter != 7)
                {
                    movesAvailalbe++;
                }
            }
            if (starter > 10 || starter == 7)
            {
                movesAvailalbe++;
            }
            //14 possible moves minus all the moves still avialable
            evalue += 14 - movesAvailalbe;
            
            //Uses this to evaluate playing special cards on high costed cards and punishes for low costed cards
            if(gameBoard.cardsOnTheBoard.Count > 1)
            {
                if(gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count-1].special && !gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 2].special)
                {
                    if(gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count-2].value > 11)
                    {
                        evalue += 10;
                    } else
                    {
                        evalue -= 10;
                    }
                    //takes points away for playing a two in aggresive mode 
                    if(gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value == 0){
                        evalue -= 15;
                    }
                }
            }

            return evalue;
        }

        public int evaluate()
        {
            int evaluation = 0;
            //Evaluate the current board state to give the node its value
            //Using a very small finate state machine to decide if the AI should be defensive or aggressive
            //To decide the current state the AI will look at the ammount of cards in the pile
            //And the more cards there are the less likly the AI is to be defensive
            //This is because as the pile gets larger, the more the AI wants the player to pick up
            int rnd = Random.Range(0, 50 + Mathf.RoundToInt(gameBoard.cardsOnTheBoard.Count * 1.5f));
            if (rnd <= 50)
            {
                //Defensive
                //Values getting rid of lower costed cards, so ending the turn with lower costed cards will take points away
                //But ending the turn with higher costed points will earn more points
                //if(gameBoard.cardsOnTheBoard)
                if (currentHand.cardsInHand.Count != 0)
                {
                    evaluation = defensive(currentHand, evaluation, Move);
                }
                else if (currentTop.cardsInHand.Count != 0)
                {
                    evaluation = defensive(currentTop, evaluation, Move);
                }
                else
                {
                    //We are on the bottom and any move is random so cant evaluate
                }
            }
            else
            {
                //Aggressive
                //Values getting rid of higher costed cards
                //Value playing a big number gap between the last two cards for example playing a jack on a 5 will get 6 points
                //However if the AI doesnt have something to back that up with or any null cards (as these can be anything) points will be taken away 

                
                if (currentHand.cardsInHand.Count != 0)
                {
                    evaluation = aggressive(currentHand, evaluation, Move);
                }
                else if (currentTop.cardsInHand.Count != 0)
                {
                    evaluation = aggressive(currentTop, evaluation, Move);
                }
                else
                {
                    //We are on the bottom and any move is random so cant evaluate
                }
            }


            if (currentHand.cardsInHand.Count < playersHand)
            {
                //If we have no cards left but the player does then we lose points
                evaluation -= playersHand - currentHand.cardsInHand.Count;
            } else
            {
                //Add the players hand to the point value, this means we value the player picking up more cards
                evaluation += currentHand.cardsInHand.Count - playersHand;
            }
            evaluated = true;
            return evaluation;
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
        public moveList mostValuableMove;
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
    public bool skipPlayer = false;
    bool started = false;
    bool go = false;

    void Start()
    {
        gameController = GetComponent<GameHandler>();
        pile = GameObject.FindGameObjectWithTag("Board").GetComponent<Pile>();
    }

    void EndTurn()
    {
        if (skipPlayer)
        {
            gameController.pTurn = false;
            skipPlayer = false;
        }
        else
        {
            gameController.pTurn = true;
            gameController.LockedControl();
        }
        if (gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Count == 0 && gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand.Count == 0)
        {
            if (gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand.Count == 0)
            {
                print("AI WON");
                SceneManager.LoadScene("AIWon");
            }
        }
        gameController.skipAI = false;
    }
    boardState makeBoardState(PlayerController.HandLocations hand, moveList move, int howMany, boardState current, Pile.Board gameBoard)
    {
        GameHandler.Hand tempHand = current.currentHand.clone(current.currentHand.cardsInHand);
        GameHandler.Hand tempTop = current.currentTop.clone(current.currentTop.cardsInHand);
        GameHandler.Hand tempBot = current.currentBot.clone(current.currentBot.cardsInHand);

        GameHandler.Hand affected = null;
        int tempPHandSize = current.playersHand;
        int tempPTopSize = current.playersTop;
        int tempPBotSize = current.playersBottom;
        bool PlayersTurn = false;
        if (hand == PlayerController.HandLocations.eHand) // using reference types to my advantage
        {
            affected = tempHand;
        }
        else if (hand == PlayerController.HandLocations.eTop)
        {
            affected = tempTop;
        }
        else if (hand == PlayerController.HandLocations.eBot)
        {
            affected = tempBot;
        }
        else if(hand == PlayerController.HandLocations.pHand)
        {
            PlayersTurn = true;
            if(move != moveList.PickUpPile)
            {
                GameHandler.Hand playersOptons = playersTurn(gameBoard.clone(gameBoard.cardsOnTheBoard), current.playersHand, current.playersTop);
                affected = playersOptons.clone(playersOptons.cardsInHand);
            }
            if(current.playersHand != 0)
            {
                tempPHandSize--;
            } else if(current.playersTop != 0)
            {
                tempPTopSize--;
            } else
            {
                tempPBotSize--;
            }
        }

        int newDeckSize = current.deckSize;
        Pile.Board newBoard = gameBoard.clone(gameBoard.cardsOnTheBoard);
        int checker = 0;
        if (move != moveList.PickUpPile)
        {
            for (int i = 0; i < affected.cardsInHand.Count; i++)
            {
                if (affected.cardsInHand[i] != null) // to check the new cards that we drew
                {
                    if (checker != howMany)
                    {
                        if (affected.cardsInHand[i].value == (int)move)
                        {
                            newBoard.cardsOnTheBoard.Add(affected.cardsInHand[i]);
                            switch (affected.cardsInHand[i].value)
                            {
                                case 3:
                                    //GameHandler.SpecialCards three = new GameHandler.SpecialCards(3, true);
                                    //three.Effect(gameBoard, 0);
                                    //if (PlayersTurn) // the threes for the opponents turn are done elsewhere
                                    //{
                                    //    if (newBoard.cardsOnTheBoard.Count > 1)
                                    //    {
                                    //        newBoard.cardsOnTheBoard[newBoard.cardsOnTheBoard.Count - 1].value =
                                    //            newBoard.cardsOnTheBoard[newBoard.cardsOnTheBoard.Count - 2].value;
                                    //    }
                                    //}
                                    break;
                                case 7:
                                    //just add the seven check into the evaluate
                                    break;
                                case 8:
                                    //some value to say that they are going again 
                                    //node needs this so that i can check to change the evaluation to my turn for that nodes children
                                    checker = howMany-1; // play only one eight a turn
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
        }
        else
        {
            //pick up pile 
            if (!PlayersTurn)
            {
                tempPHandSize += newBoard.cardsOnTheBoard.Count;
            }
            for (int i = 0; i < newBoard.cardsOnTheBoard.Count; i++)
            {
                affected.cardsInHand.Add(newBoard.cardsOnTheBoard[0]);
                newBoard.cardsOnTheBoard.Remove(newBoard.cardsOnTheBoard[0]);
            }
        }

        GameHandler.Hand newEHand = tempHand.clone(tempHand.cardsInHand);
        GameHandler.Hand newETop = tempTop.clone(tempTop.cardsInHand);
        GameHandler.Hand newEBot = tempBot.clone(tempBot.cardsInHand);
        boardState temp = new boardState(newEHand, newETop, newEBot, tempPHandSize, tempPTopSize, tempPBotSize, newBoard, newDeckSize, move);

        return temp;
    }
    void addNodeSub(node currentNode, GameHandler.Hand Hand, int i, bool player)
    {
        if (!player)
        {
            boardState prediction = makeBoardState(PlayerController.HandLocations.pHand, (moveList)Hand.cardsInHand[i].value, 1, currentNode.currentBoard, currentNode.currentBoard.gameBoard);
            currentNode.addNode(player, currentNode, prediction, (moveList)Hand.cardsInHand[i].value);

        } else
        {
            if (Hand.cardsInHand[i] != null)
            {
                int howMany = 0;
                //always play as many of the same card as possible
                for (int j = 0; j < Hand.cardsInHand.Count; j++)
                {
                    if (Hand.cardsInHand[j] != null)
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
    }
    GameHandler.Hand playersTurn(Pile.Board gameBoard, int pHand, int pTop)
    {
        GameHandler.Hand playersOptons = new GameHandler.Hand(null);
        if(pHand != 0 || pTop == 0)
        {// its in their hand so they can play anything
            int starter = 0;
            if (gameBoard.cardsOnTheBoard.Count == 0)
            {
                //Player can do any move
                for (int i = 0; i < 15; i++)
                {
                    if (i == 1 || i == 2)
                    {

                    }
                    else
                    {
                        GameHandler.Cards tempChoice = new GameHandler.Cards();
                        tempChoice.value = i;
                        tempChoice.special = false;
                        tempChoice.card = null;
                        tempChoice.bottomCard = false;
                        tempChoice.handler = null;
                        tempChoice.Startvalue = i;
                        if (i == 2 || i == 3 || i == 7 || i == 8 || i == 10)
                        {
                            tempChoice.special = true;
                        }
                        playersOptons.cardsInHand.Add(tempChoice);
                    }
                }
            }
            else
            {
                starter = gameBoard.cardsOnTheBoard[gameBoard.cardsOnTheBoard.Count - 1].value;
                if(starter == 7)
                {
                    //flip the checks
                    for (int i = starter; i >= 0; i--)
                    {
                        if (i == 1 || i == 2)
                        {

                        }
                        else
                        {
                            GameHandler.Cards tempChoice = new GameHandler.Cards();
                            tempChoice.value = i;
                            tempChoice.special = false;
                            tempChoice.card = null;
                            tempChoice.bottomCard = false;
                            tempChoice.handler = null;
                            tempChoice.Startvalue = i;
                            if (i == 2 || i == 3 || i == 7 || i == 8 || i == 10)
                            {
                                tempChoice.special = true;
                            }
                            playersOptons.cardsInHand.Add(tempChoice);
                        }
                    }
                } else
                {
                    for (int i = starter; i < 15; i++)
                    {
                        if (i == 1 || i == 2)
                        {

                        }
                        else
                        {
                            GameHandler.Cards tempChoice = new GameHandler.Cards();
                            tempChoice.value = i;
                            tempChoice.special = false;
                            tempChoice.card = null;
                            tempChoice.bottomCard = false;
                            tempChoice.handler = null;
                            tempChoice.Startvalue = i;
                            if (i == 2 || i == 3 || i == 7 || i == 8 || i == 10)
                            {
                                tempChoice.special = true;
                            }
                            playersOptons.cardsInHand.Add(tempChoice);
                        }
                    }
                }
            }
            //can always play the special cards
            if (starter > 0)
            {
                if (starter != 7)
                {
                    GameHandler.Cards tempChoice = new GameHandler.Cards();
                    tempChoice.value = 0;
                    tempChoice.special = true;
                    tempChoice.card = null;
                    tempChoice.bottomCard = false;
                    tempChoice.handler = null;
                    tempChoice.Startvalue = 0;
                    playersOptons.cardsInHand.Add(tempChoice);
                }
            }
            if (starter > 3)
            {
                if (starter != 7)
                {
                    GameHandler.Cards tempChoice = new GameHandler.Cards();
                    tempChoice.value = 3;
                    tempChoice.special = true;
                    tempChoice.card = null;
                    tempChoice.bottomCard = false;
                    tempChoice.handler = null;
                    tempChoice.Startvalue = 3;
                    playersOptons.cardsInHand.Add(tempChoice);
                }
            }
            if (starter > 10 || starter == 7)
            {
                GameHandler.Cards tempChoice = new GameHandler.Cards();
                tempChoice.value = 10;
                tempChoice.special = true;
                tempChoice.card = null;
                tempChoice.bottomCard = false;
                tempChoice.handler = null;
                tempChoice.Startvalue = 10;
                playersOptons.cardsInHand.Add(tempChoice);
            }
        }
        else if (pTop != 0)
        { //We can see the cards that they are going to play
            for(int i = 0; i < gameController.Locations[(int)PlayerController.HandLocations.pTop].cardsInHand.Count; i++)
            {
                playersOptons.cardsInHand.Add(gameController.Locations[(int)PlayerController.HandLocations.pTop].cardsInHand[i]);
            }
        }

        return playersOptons;
    }
    void addNodePre(node currentNode, GameHandler.Hand Hand, bool player)
    {
        if (!player)
        {
            //assume the player can play anything within the rules
            //Make a hand with all the moves in
            GameHandler.Hand playersOptons = playersTurn(currentNode.currentBoard.gameBoard.clone(currentNode.currentBoard.gameBoard.cardsOnTheBoard), 
                gameController.Locations[(int)PlayerController.HandLocations.pHand].cardsInHand.Count, 
                gameController.Locations[(int)PlayerController.HandLocations.pTop].cardsInHand.Count);
            for (int j= 0; j < playersOptons.cardsInHand.Count; j++)
            {
                addNodeSub(currentNode, playersOptons, j, false);
            }
            //No pick up for player as the AI will have to beat the best of the player
        } else
        {
            // there is still cards in hand 
            for (int i = 0; i < Hand.cardsInHand.Count; i++)
            {
                if (Hand.cardsInHand[i] != null) // cant evaluate null cards, these are cards that we have drawn but their value is unknown
                {
                    if (currentNode.currentBoard.gameBoard.cardsOnTheBoard.Count == 0)
                    {
                        addNodeSub(currentNode, Hand, i, player);
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
            //Add pick up pile 
            boardState pickUpPrediction = makeBoardState(PlayerController.HandLocations.eHand, moveList.PickUpPile, 0, currentNode.currentBoard, currentNode.currentBoard.gameBoard);
            currentNode.addNode(player, currentNode, pickUpPrediction, moveList.PickUpPile);
        }
    }
    void addMovesSub(node currentNode, bool OppsTurn)
    {
        if (!OppsTurn)
        {
            addNodePre(currentNode, null, OppsTurn);
        } else
        {
            if (currentNode.currentBoard.currentHand.cardsInHand.Count != 0) // still cards in hand
            {
                addNodePre(currentNode, currentNode.currentBoard.currentHand, OppsTurn);
            }
            else if (currentNode.currentBoard.currentTop.cardsInHand.Count != 0)
            {
                //there is still cards on top
                addNodePre(currentNode, currentNode.currentBoard.currentTop, OppsTurn);
            }
        }
    }

    void recursiveTree(node currentNode, bool OppsTurn)
    {
        if(currentNode.children.Count != 0)
        {
            for (int i = 0; i < currentNode.children.Count; i++)
            {
                recursiveTree(currentNode.children[i], !OppsTurn);
            }
        } else
        {
            addMovesSub(currentNode, OppsTurn);
        } 
    }
    void addAllMoves(int depth, tree gameTree, node currentLevel, bool OppsTurn)
    {
        //Go through the current nodes board state and make all the available moves have nodes
        //Then add one option for picking up the pile

        //Then recurse and now change to add all the options for the player 
        //And again until the depth has hit the max depth 
        OppsTurn = !OppsTurn;
        if (depth != maxDepth)
        {
            recursiveTree(gameTree.Root, OppsTurn);

            depth = depth + 1;
            if (currentLevel.children.Count != 0) // ran out of moves
            { 
                addAllMoves(depth, gameTree, currentLevel.children[0], !OppsTurn);
            }
        }

    }

    void addEvals(node node)
    {
        for (int i = 0; i < node.children.Count; i++)
        {
            if (node.children[i].children.Count != 0)
            {
                if (node.moveChoice != moveList.PickUpPile)
                {
                    addEvals(node.children[i]);
                }
            }
            else
            {
                if(node.moveChoice != moveList.PickUpPile)
                {
                    node.children[i].value = node.children[i].currentBoard.evaluate();
                }
            }
        }
    }

    void findHighestOrLowest(node node)
    {
        if(node.children.Count == 2)
        {
            //One of them must be pick up, and we dont want to evaluate that one
            
        }

        int highestOrLowest = 0;
        for (int i = 0; i < node.children.Count; i++)
        {
            if(node.children[i].moveChoice != moveList.PickUpPile)
            {
                if (node.moveChoice != moveList.PickUpPile)
                {
                    if (node.playersMove)
                    {
                        if (node.children[highestOrLowest].value > node.children[i].value)
                        {
                            highestOrLowest = i;
                        }

                    }
                    else
                    {
                        if (node.children[highestOrLowest].value < node.children[i].value)
                        {
                            highestOrLowest = i;
                        }
                    }

                    node.value = node.children[highestOrLowest].value;
                    node.currentBoard.evaluated = true;
                    node.mostValuableMove = node.children[highestOrLowest].moveChoice;
                }
            }
        }
    }

    void fillInTheRestOfTheTree(node node)
    {
        for (int i = 0; i < node.children.Count; i++)
        {
            if (!node.children[i].currentBoard.evaluated)
            {
                fillInTheRestOfTheTree(node.children[i]);
            }
            else
            {
                //go back up a node, and call another function
                if(node.moveChoice != moveList.PickUpPile)
                {
                    findHighestOrLowest(node);
                }
            }
        }
    }

    void doMove(moveList bestMove)
    {
        List<GameHandler.Cards> Selected = new List<GameHandler.Cards>();
        int moveValue = 0;
        bool pickUp = false;
        switch (bestMove)
        {
            case moveList.Ace:
                moveValue = 14;
                break;
            case moveList.Two:
                moveValue = 0;
                break;
            case moveList.Three:
                moveValue = 3;
                break;
            case moveList.Four:
                moveValue = 4;
                break;
            case moveList.Five:
                moveValue = 5;
                break;
            case moveList.Six:
                moveValue = 6;
                break;
            case moveList.Seven:
                moveValue = 7;
                break;
            case moveList.Eight:
                moveValue = 8;
                break;
            case moveList.Nine:
                moveValue = 9;
                break;
            case moveList.Ten:
                moveValue = 10;
                break;
            case moveList.Jack:
                moveValue = 11;
                break;
            case moveList.Queen:
                moveValue = 12;
                break;
            case moveList.King:
                moveValue = 13;
                break;
            case moveList.PickUpPile:
                if (gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Count == 0)
                {
                    if (gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand.Count != 0)
                    {
                        pile.bottomCardPlay(gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand[0]);
                    }
                }
                pile.pickUpPile();
                pickUp = true;
                EndTurn();
                break;
        }
        if (!pickUp)
        {
            int selected = 0;
            if (gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Count != 0) // still cards in hand
            {
                selected = (int)PlayerController.HandLocations.eHand;
            }
            else if (gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand.Count != 0)
            {
                //there is still cards on top
                selected = (int)PlayerController.HandLocations.eTop;
            }

            for (int i = 0; i < gameController.Locations[selected].cardsInHand.Count; i++)
            {
                if (gameController.Locations[selected].cardsInHand[i].value == moveValue)
                {
                    Selected.Add(gameController.Locations[selected].cardsInHand[i]);
                    if (gameController.Locations[selected].cardsInHand[i].value == 8)
                    {
                        break; // make it play one eight at a time
                    }
                }
            }
            pile.AIChoice(Selected);
        }
    }
    IEnumerator Wait()
    {
        if (!started)
        {
            print("Thinkning");
            yield return new WaitForSeconds(0.5f);
            go = true;
            started = false;
        }
    }
    void AITrun()
    {
        if (!go)
        {
            StartCoroutine(Wait());// Add thinking time
            started = true;
        }
        if (go)
        {
            go = false;
            if (gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand.Count == 0 && gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand.Count == 0)
            {
                if (gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand.Count == 0)
                {
                    print("AI WON");
                    SceneManager.LoadScene("AIWon");
                }
                else
                {
                    //chose a random card, from 0 to count and play that card
                    //New function
                    int rnd = Random.Range(0, gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand.Count);
                    List<GameHandler.Cards> Rand = new List<GameHandler.Cards>();
                    Rand.Add(gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand[rnd]);
                    pile.AIChoice(Rand);
                    EndTurn();
                }
            }
            else
            {
                //Save the board state
                currentBoardState = new boardState(
                    gameController.Locations[(int)PlayerController.HandLocations.eHand].clone(gameController.Locations[(int)PlayerController.HandLocations.eHand].cardsInHand),
                    gameController.Locations[(int)PlayerController.HandLocations.eTop].clone(gameController.Locations[(int)PlayerController.HandLocations.eTop].cardsInHand),
                    gameController.Locations[(int)PlayerController.HandLocations.eBot].clone(gameController.Locations[(int)PlayerController.HandLocations.eBot].cardsInHand),
                    gameController.Locations[(int)PlayerController.HandLocations.pHand].cardsInHand.Count,
                    gameController.Locations[(int)PlayerController.HandLocations.pTop].cardsInHand.Count,
                    gameController.Locations[(int)PlayerController.HandLocations.pBot].cardsInHand.Count,
                    pile.gameBoard.clone(pile.gameBoard.cardsOnTheBoard),
                    gameController.deckStorage.deck.Count,
                    moveList.empty
                    );
                //Make a new binary tree with all the different moves possible to the player, and enemy for however long the recursive value is
                node root = new node(false, null, currentBoardState, moveList.empty);
                tree gameTree = new tree(root);
                addAllMoves(0, gameTree, gameTree.Root, false);
                if(gameTree.Root.children.Count == 1)
                {
                    //No moves
                    doMove(moveList.PickUpPile);
                } else
                {
                    //Evaluate all the different moves of the bottom level
                    addEvals(gameTree.Root);
                    //Then fill out the other moves based on the children
                    for (int i = 0; i < maxDepth; i++)
                    {
                        fillInTheRestOfTheTree(gameTree.Root);
                    }
                    //Chose a move, Do the move
                    doMove(gameTree.Root.mostValuableMove);
                }
                //End turn
                EndTurn();
            }
        }
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
        for (int i = 0; i < gameController.downAmmount; i++)
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