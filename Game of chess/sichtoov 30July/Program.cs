using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sichtoov_30July
{
    class Program
    {
        static void Main(string[] args)
        {
            new Game().play();
        }
    }
    class Location
    {
        public int column;
        public int row;
        public Location (int column, int row)
        {
            this.column = column;
            this.row = row;
        }
    }
    class Move
    {
        public Location from;
        public Location to;
        public Move(int columnFrom,  int rowFrom, int columnTo, int rowTo)
        {
            this.from = new Location(columnFrom, rowFrom);
            this.to = new Location(columnTo, rowTo);
        }
    }
    class Game
    {
        string allWhiteBoards;
        string allBlackBoards;
        string lastWhiteBoard;
        string lastBlackBoard;
        string saveAllMoves;
        int countNonChangableMoves;
        public void play()
        {
            bool gameEnd = false;
            int countMoves = 0;
            ChessPiece[,] board;
            board = setBoard();
            print(board); 
            do
            {
                bool isWhiteTurn = countMoves % 2 == 0;
                string gameState = (this.checkStalemateAndCheckmate(board, isWhiteTurn));
                if (gameState == "Checkmate")
                {
                    Console.WriteLine((isWhiteTurn ? "Black" : "White") + " player win");
                    break;
                }
                if (gameState == "draw")
                {
                    Console.WriteLine("No move available- it's a draw");
                    break;
                }
                string theMoveString = getPlayerMove(isWhiteTurn, board);
                if (theMoveString == "draw" || theMoveString == "resign")
                {
                    Console.WriteLine(theMoveString == "draw" ? "It's a draw" : "player " + (isWhiteTurn ? "white resign, black player win" : "black resign, white player win"));
                    break;
                }
                Move theMove = convertMove(theMoveString);
                if (this.isMoveLegal(board, theMove, isWhiteTurn) && this.isKingSafeAfterMove(board, theMove, isWhiteTurn))
                {
                    if (countMoves == 0) { this.SaveBoardStats(board, isWhiteTurn); }
                    this.moveTool(board, theMove);
                    this.setEnPassantStatus(board, isWhiteTurn, theMove);
                    countMoves++;
                    this.SaveBoardStats(board, !isWhiteTurn);
                    if (this.isDraw(board, isWhiteTurn))
                    {
                        this.print(board);
                        Console.WriteLine("its a draw");
                        break;
                    }
                    isWhiteTurn = !isWhiteTurn;
                    this.print(board);
                }
                else
                {
                    this.print(board);
                    Console.WriteLine();
                    Console.WriteLine(theMoveString == "draw" ? "draw not accepted" : "Invalid move");
                }
                Console.WriteLine();
            } while (!gameEnd);
        }
        public ChessPiece[,] setBoard()
        {
            ChessPiece[,] board;
            board = new ChessPiece[8, 8];
            for (int i = 0; i < 8; i++)
            {
                board[i, 7] = new Rook(i == 7, true);
                board[i, 0] = new Rook(i == 7, true);
                board[i, 6] = new Knight(i == 7);
                board[i, 1] = new Knight(i == 7);
                board[i, 5] = new Bishop(i == 7);
                board[i, 2] = new Bishop(i == 7);
                board[i, 3] = new Queen(i == 7);
                board[i, 4] = new King(i == 7, true);
                if (i == 0) { i = 6; }
            }
            for (int column = 0; column < 8; column++)
                board[1, column] = new Pawn(false, false);
            for (int column = 0; column < 8; column++)
                board[6, column] = new Pawn(false, true);
           return board;
        }
        public void SaveBoardStats(ChessPiece[,] board, bool isWhiteTurn)
        {
            for (int column = 0; column < 8; column++)
                for (int row = 0; row < 8; row++)
                {
                    if (isWhiteTurn)
                    {
                        allWhiteBoards += board[column, row];
                        if (board[column, row] == null)
                            allWhiteBoards += "-";
                    }
                    else
                    {
                        allBlackBoards += board[column, row];
                        if (board[column, row] == null)
                            allBlackBoards += "-";
                    }
                }
            if (isWhiteTurn)
                allWhiteBoards += "|";
            else allBlackBoards += "|";
        }        
        public void print(ChessPiece[,] board)
        {
            Console.WriteLine("  A  B  C  D  E  F  G  H  ");
            for (int column = 0; column < 8; column++)
            {
                Console.Write((8 - column) + " ");
                for (int row = 0; row < 8; row++)
                    Console.Write(board[column, row] == null ? "   " : board[column, row] + " ");
                Console.WriteLine();
            }
        }
        public string getPlayerMove(bool isWhiteTurn, ChessPiece[,] board)
        {
            string input = "";
            bool isValid = true, firstTime = true;
            do
            {
                if (!firstTime) { this.print(board); }
                Console.WriteLine();
                Console.WriteLine((!firstTime && input != "DRAW" ? "Invalid move. " : "") + (isWhiteTurn ? "White turn- " : "Black turn- ") + "please enter your move, draw or resign and press ENTER");
                firstTime = false;
                input = Console.ReadLine().Trim().ToUpper();
                if (input == "DRAW")
                    if (isOtherPlayerAgreeToDraw(isWhiteTurn))
                        return "draw";
                if (input == "RESIGN")
                    return "resign";
                isValid = true;
                if ((input.Length != 4) || (input[0] == input[2] && input[1] == input[3]))
                    isValid = false;
                for (int i = 0; input.Length == 4 && i < 4 && isValid; i++)
                {
                    if (i % 2 == 0)
                        isValid = ("ABCDEFGHabcdefgh".Contains(input[i]));
                    else isValid = ("12345678".Contains(input[i]));
                }

            } while (!isValid);
            return input;
        }
        public static Move convertMove(string theMove)
        {
            string letters = "ABCDEFGHabcdefgh";
            string numbers = "0123456701234567";
            int insexOfFirstLetter = int.Parse(numbers[letters.IndexOf(theMove[0])] + "");
            int insexOfsecondLetter = int.Parse(numbers[letters.IndexOf(theMove[2])] + "");
            int[] indexNumbersOnBoard;
            indexNumbersOnBoard = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                    indexNumbersOnBoard[i] = (int.Parse(numbers[insexOfFirstLetter] + ""));
                if (i == 2)
                    indexNumbersOnBoard[i] = (int.Parse(numbers[insexOfsecondLetter] + ""));
                if (i % 2 != 0)
                    indexNumbersOnBoard[i] = (8 - int.Parse(theMove[i] + ""));
            }
            int x = indexNumbersOnBoard[1];
            indexNumbersOnBoard[1] = indexNumbersOnBoard[0];
            indexNumbersOnBoard[0] = x;
            x = indexNumbersOnBoard[3];
            indexNumbersOnBoard[3] = indexNumbersOnBoard[2];
            indexNumbersOnBoard[2] = x;
            Move move;
            move = new Move(indexNumbersOnBoard[0], indexNumbersOnBoard[1], indexNumbersOnBoard[2], indexNumbersOnBoard[3]);
            return move;
        }
        public bool isOtherPlayerAgreeToDraw(bool isWhitePlayerTurn)
        {
            bool isValid = true, firstTime = true;
            string input = "";
            do
            {
                isValid = true;
                Console.WriteLine((firstTime ? "" : "Invalid! ") + (isWhitePlayerTurn ? "Black player," : "White player,") + " do you accept a draw? write yes or no and hit ENTER");
                firstTime = false;
                input = Console.ReadLine();
                input = input.Trim();
                if (!(input == "yes" || input == "YES" || input == "no" || input == "NO"))
                    isValid = false;
            } while (!isValid);
            if (input == "yes" || input == "YES")
                return true;
            if (input == "no" || input == "NO")
                return false;
            return false;
        }
        public bool isMoveLegal(ChessPiece[,] board, Move move, bool isWhitePlayerTurn)
        {
            bool isValid;
            if (board[move.from.column, move.from.row] == null)
                return false;
            isValid = IsPieceInPlayerColor(board, move.from.column, move.from.row, isWhitePlayerTurn); 
            if (!isValid)
                return false;
            isValid = board[move.from.column, move.from.row].IsMoveLegal(move, isWhitePlayerTurn, board);
            if (!isValid)
                return false;
            isValid = this.isMoveToLegalPlace(board, move, isWhitePlayerTurn);
            if (!isValid)
                return false;
            isValid = board[move.from.column, move.from.row].isWayClear(board, move);
            if (!isValid)
                return false;
            if (this.isCastling(board, move))
            {
                isValid = this.isCastlingLegal(move, isWhitePlayerTurn, board);
                if (!isValid)
                    return false;
            }
            return true;
        }
        public bool IsPieceInPlayerColor(ChessPiece[,] board, int rowFrom, int columnfrom, bool isWriteTurn)
        {
            return board[rowFrom, columnfrom].getIsWhite() == isWriteTurn;
        }
        public virtual bool isMoveToLegalPlace(ChessPiece[,] board, Move move, bool isWhiteTurn)
        {
            return (board[move.to.column, move.to.row] == null) || (board[move.to.column, move.to.row].getIsWhite() != isWhiteTurn);
        }
        public virtual bool isKingSafeAfterMove(ChessPiece[,] board, Move move, bool isWhiteTurn)
        {
            ChessPiece SaveMoveToContent;
            SaveMoveToContent = new ChessPiece(true);
            if (!(move.from.column == move.to.column && move.from.row == move.to.row))
            {
               
                SaveMoveToContent = board[move.to.column, move.to.row];// שומר את מה שהיה במשבצת לפני שהכלי זז לשם
                board[move.to.column, move.to.row] = board[move.from.column, move.from.row];
                board[move.from.column, move.from.row] = null;
            }
            string theKingPlace = "";
            bool find = false;
            for (int column = 0; !find && column < 8; column++)
            {
                for (int row = 0; !find && row < 8; row++)
                    if (board[column, row] is King)
                        if (board[column, row].getIsWhite() == isWhiteTurn)
                        {
                            find = true;
                            theKingPlace = (column + "");
                            theKingPlace += (row + "");
                        }
            }
            find = false;

            Move hypotheticalMove;
            hypotheticalMove = new Move(0, 0, 0, 0);
            hypotheticalMove.to.column = int.Parse(theKingPlace[0] + "");
            hypotheticalMove.to.row = int.Parse(theKingPlace[1] + "");
            for (int column = 0; !find && column < 8; column++)
            {
                for (int row = 0; !find && row < 8; row++)
                {
                    hypotheticalMove.from.column = column;
                    hypotheticalMove.from.row = row;
                    if (this.isMoveLegal(board, hypotheticalMove, !isWhiteTurn))
                    {
                        if (!(move.from.column == move.to.column && move.from.row == move.to.row))
                        {
                            board[move.from.column, move.from.row] = board[move.to.column, move.to.row];
                            board[move.to.column, move.to.row] = SaveMoveToContent;
                        }
                        return false;
                    }
                }
            }
            if (!(move.from.column == move.to.column && move.from.row == move.to.row))
            {
                board[move.from.column, move.from.row] = board[move.to.column, move.to.row];
                board[move.to.column, move.to.row] = SaveMoveToContent;
            }
            return true;
        }
        public string checkStalemateAndCheckmate(ChessPiece[,] board, bool isWhiteTurn)
        {
            bool findAmove = false, isCheckmate = false;
            Move theMove;
            theMove = new Move(0, 0, 0, 0);
            for (int i = 0; !findAmove && i < 8; i++)
                for (int j = 0; !findAmove && j < 8; j++)
                    for (int column = 0; !findAmove && column < 8; column++)
                        for (int row = 0; !findAmove && row < 8; row++)
                        {
                            theMove.from.column = i;
                            theMove.from.row = j;
                            theMove.to.column = column;
                            theMove.to.row = row;
                            if (isMoveLegal(board, theMove, isWhiteTurn))
                                if (!(column == i && row == j))
                                    findAmove = (isKingSafeAfterMove(board, theMove, isWhiteTurn));
                            if ((column == i && row == j) && (board[i, j] is King) && (board[i, j].getIsWhite() == isWhiteTurn))
                                isCheckmate = (!(isKingSafeAfterMove(board, theMove, isWhiteTurn)));
                        }
            if (isCheckmate && !findAmove)
                return "Checkmate";
            return findAmove ? "continue" : "draw";
        }
        public bool isCastling(ChessPiece[,] board, Move move) 
        {
            return (Math.Abs(move.from.row - move.to.row) == 2 && Math.Abs(move.from.column - move.to.column) == 0)&&(board[move.from.column, move.from.row] is King);
        }
        public bool isCastlingLegal(Move theMove, bool isWriteTurn, ChessPiece[,] board) 
        {
            bool isAllConditionsMet = true, isShortCasteling = theMove.from.row < theMove.to.row ? true : false;
            if (isShortCasteling)// הצרחה קצרה
            {
                if ((saveAllMoves == null) || (saveAllMoves.Contains((isWriteTurn ? "74" : "04")))) { return false; }
                if ((saveAllMoves == null) || (saveAllMoves.Contains((isWriteTurn ? "77" : "07")))) { return false; }
                if (!(board[theMove.from.column, theMove.from.row + 1] == null && board[theMove.from.column, theMove.from.row + 2] == null)) { return false; }
                Move TheMoveToCheck;
                TheMoveToCheck = new Move(0, 0, 0, 0);
                TheMoveToCheck.from.column = TheMoveToCheck.to.column = theMove.from.column;
                TheMoveToCheck.from.row = theMove.from.row;
                for (int row = theMove.from.row; row < 7 && isAllConditionsMet; row++)
                {
                    TheMoveToCheck.to.row = row;
                    isAllConditionsMet = this.isKingSafeAfterMove(board, TheMoveToCheck, isWriteTurn);
                }
            }
            if (!isShortCasteling)// הצרחה ארוכה
            {
                if ((saveAllMoves == null) || saveAllMoves.Contains((isWriteTurn ? "74" : "04"))) { return false; }
                if ((saveAllMoves == null) || saveAllMoves.Contains((isWriteTurn ? "70" : "00"))) { return false; }
                if (!(board[theMove.from.column, theMove.from.row - 1] == null && board[theMove.from.column, theMove.from.row - 2] == null && board[theMove.from.column, theMove.from.row - 2] == null)) { return false; }
                Move TheMoveToCheck;
                TheMoveToCheck = new Move(0, 0, 0, 0);
                TheMoveToCheck.from.column = TheMoveToCheck.to.column = theMove.from.column;
                TheMoveToCheck.from.row = theMove.from.row;
                for (int row = theMove.from.row; row > 1 && isAllConditionsMet; row--)
                {
                    TheMoveToCheck.to.row = row;
                    this.isKingSafeAfterMove(board, TheMoveToCheck, isWriteTurn);
                }
            }
            return isAllConditionsMet;
        }
        public bool moveTool(ChessPiece[,] board, Move move)
        {
            if (board[move.from.column, move.from.row].isEnPassant(board, move)) // en passant
            {
                board[move.from.column, move.to.row] = null;
                allWhiteBoards = allBlackBoards = "";
            }
            if (this.isCastling(board, move)) // castling
            {
                if (move.from.row < move.to.row)
                {
                    board[move.to.column, move.to.row - 1] = board[move.from.column, 7];
                    board[move.from.column, 7] = null;
                }
                else
                {
                    board[move.to.column, move.to.row + 1] = board[move.from.column, 0];
                    board[move.from.column, 0] = null;
                }
                allWhiteBoards = allBlackBoards = "";
            }
            if ((board[move.to.column, move.to.row] != null || board[move.from.column, move.from.row] is Pawn))
            {
                countNonChangableMoves = 0;
                allBlackBoards = allWhiteBoards = "";
            }
            else countNonChangableMoves++;
            if (board[move.from.column, move.from.row] is Rook)
                if (((Rook)board[move.from.column, move.from.row]).getCanDoCastling())
                {
                    if (board[move.from.column, move.from.row].getIsWhite())
                        if (board[7, 4] is King)
                            if (((King)board[7, 4]).getCanDoCastling())
                            {
                                ((Rook)board[move.from.column, move.from.row]).setCanDoCastling(false) ;
                                allBlackBoards = allWhiteBoards = "";
                            }
                    if (!(board[move.from.column, move.from.row].getIsWhite()))
                        if (board[0, 4] is King)
                            if (((King)board[0, 4]).getCanDoCastling())
                            {
                                ((Rook)board[move.from.column, move.from.row]).setCanDoCastling(false) ;
                                allBlackBoards = allWhiteBoards = "";
                            }
                }
            if (board[move.from.column, move.from.row] is King)
                if (((King)board[move.from.column, move.from.row]).getCanDoCastling())
                {
                    ((King)board[move.from.column, move.from.row]).setCanDoCastling(false);
                    allBlackBoards = allWhiteBoards = "";
                }
            board[move.to.column, move.to.row] = board[move.from.column, move.from.row];
            board[move.from.column, move.from.row] = null;
            if (board[move.to.column, move.to.row].isPromotion(move))
                board[move.to.column, move.to.row].setPromotion(move, board);
            saveAllMoves += move.from.column + "" + move.from.row + "" + move.to.column + "" + move.to.row + "" + " ";
            return true;
        }
        public void setEnPassantStatus(ChessPiece[,] board, bool isWhiteTurn, Move move) // אחרי התזוזה
        {
            if (board[move.to.column, move.to.row] is Pawn && Math.Abs(move.from.column - move.to.column) == 2)
                ((Pawn)board[move.to.column, move.to.row]).setCanBeEatenInEnPasant(true);
            for(int row = 0; row<8; row++)
                for(int column=0; column<8; column++)
                {
                    if (board[row, column] is Pawn)
                        if(((Pawn)board[row, column]).getIsWhite()!=isWhiteTurn)
                            ((Pawn)board[row, column]).setCanBeEatenInEnPasant(false);
                }
        }
        public bool isDraw(ChessPiece[,] board, bool isWhiteTurn)
        {
            if (isDeadPosition(board))
                return true;
            if (isWhiteTurn)
                lastBlackBoard =this.getBoardState(board);
            else lastWhiteBoard = this.getBoardState(board);
            if (this.isThreefoldRepetition(!isWhiteTurn))
                return true;
            if (countNonChangableMoves == 100)
                return true;
            return false;
        }
        public string getBoardState(ChessPiece[,] board)
        {
            string output = "";
            for (int column = 0; column < 8; column++)
                for (int row = 0; row < 8; row++)
                {
                    output += board[column, row];
                    if (board[column, row] == null)
                        output += "-";
                }
            return output;
        }
        public bool isDeadPosition(ChessPiece[,] board)
        {
            string ChessPices = "WR WN WB WQ WP BR BN BB BQ BP";
            bool someoneCanWin = false;
            for (int i = 0; i < ChessPices.Length; i += 3)
            {
                someoneCanWin = getBoardState(board).Contains((ChessPices[i] + "") + (ChessPices[i + 1] + ""));
            }
            return !someoneCanWin;
        }
        public bool isThreefoldRepetition(bool isWhiteTurn)
        {
            int countSameBoard = 0;
            if (!isWhiteTurn)
            {
                for (int indexToLookFor = 0; indexToLookFor < allBlackBoards.Length; indexToLookFor += lastBlackBoard.Length)
                {
                    if (allBlackBoards.IndexOf(lastBlackBoard, indexToLookFor) == -1)
                        return false;
                    indexToLookFor = (allBlackBoards.IndexOf(lastBlackBoard, indexToLookFor));
                    countSameBoard++;
                    if (countSameBoard == 3)
                        return true;
                }
            }
            if (isWhiteTurn)
            {
                for (int indexToLookFor = 0; indexToLookFor < allWhiteBoards.Length; indexToLookFor += lastWhiteBoard.Length)
                {
                    if (allWhiteBoards.IndexOf(lastWhiteBoard, indexToLookFor) == -1)
                        return false;
                    indexToLookFor = (allWhiteBoards.IndexOf(lastWhiteBoard, indexToLookFor));
                    countSameBoard++;
                    if (countSameBoard == 3)
                        return true;
                }
            }
            return false;
        }
    }
    class ChessPiece
    {
        protected bool isWhite;
        public void setIsWhite(bool isWhite)
        {
            this.isWhite = isWhite;
        }
        public bool getIsWhite()
        {
            return this.isWhite;
        }
        public ChessPiece(bool isWhite) { this.isWhite = isWhite; } 
        public override string ToString() { return isWhite ? "W" : "B"; }
        public virtual bool IsMoveLegal(Move move, bool isWriteTurn, ChessPiece[,] board)
        {
            return true;
        }
        public virtual bool isWayClear(ChessPiece[,] board, Move move)
        {
            return true;
        }
        public virtual bool isEnPassant(ChessPiece[,] board, Move move)
        {
            return false;
        }
        public virtual bool isPromotion(Move move)
        {
            return false;
        }
        public virtual bool setPromotion(Move move, ChessPiece[,] board)
        {
            return false;
        }  
    }
    class Rook : ChessPiece
    {
        bool canDoCastling;
        public void setCanDoCastling(bool canDoCastling)
        {
            this.canDoCastling = canDoCastling;
        }
        public bool getCanDoCastling()
        {
            return this.canDoCastling;
        }
        public Rook(bool isWhite, bool canDoCastling) : base(isWhite)
        {
            this.setCanDoCastling(canDoCastling);
        }
        public override string ToString()
        {
            return base.ToString() + "R";
        }
        public override bool IsMoveLegal(Move move, bool isWriteTurn, ChessPiece[,] board)
        {
            return move.from.column == move.to.column || move.from.row == move.to.row;
        }
        public override bool isWayClear(ChessPiece[,] board, Move move)
        {
            bool isTheWayClear = true;
            if (move.from.row == move.to.row)
            {
                int bigerIndexNumber = (move.from.column > move.to.column ? move.from.column : move.to.column);
                int smallerIndexNumber = (move.from.column < move.to.column ? move.from.column : move.to.column);
                for (int i = smallerIndexNumber + 1; i < bigerIndexNumber && isTheWayClear; i++)
                    isTheWayClear = board[i, move.from.row] == null;
            }
            if (move.from.column == move.to.column)
            {
                int bigerIndexNumber = (move.from.row > move.to.row ? move.from.row : move.to.row);
                int smallerIndexNumber = (move.from.row < move.to.row ? move.from.row : move.to.row);
                for (int i = smallerIndexNumber + 1; i < bigerIndexNumber && isTheWayClear; i++)
                    isTheWayClear = board[move.from.column, i] == null;
            }
            return isTheWayClear;
        }
    }
    class Knight : ChessPiece
    {
        public Knight(bool isWhite) : base(isWhite) { }
        public override string ToString()
        {
            return base.ToString() + "N";
        }
        public override bool IsMoveLegal(Move move, bool isWhiteTurn, ChessPiece[,] board)
        {
            return (Math.Abs(move.from.column - move.to.column) == 2) && (Math.Abs(move.from.row - move.to.row) == 1) ||
                (Math.Abs(move.from.column - move.to.column) == 1) && (Math.Abs(move.from.row - move.to.row) == 2);
        }
    }
    class Bishop : ChessPiece
    {
        public Bishop(bool isWhite) : base(isWhite) { }
        public override string ToString()
        {
            return base.ToString() + "B";
        }
        public override bool IsMoveLegal(Move move, bool isWhiteTurn, ChessPiece[,] board)
        {
            return Math.Abs(move.from.column - move.to.column) == Math.Abs(move.from.row - move.to.row);
        }
        public override bool isWayClear(ChessPiece[,] board, Move move)
        {
            bool isWayClear = true;
            bool isCorentColumnBigerThenNewColumn = move.from.column > move.to.column, isCorentRowBigerThenNewColumn = move.from.row > move.to.row;
            if (isCorentColumnBigerThenNewColumn && isCorentRowBigerThenNewColumn)
                for (int i = move.from.column - 1, j = move.from.row - 1; i > move.to.column && j > move.to.row && isWayClear; i--, j--)
                    isWayClear = board[i, j] == null;
            if (!isCorentColumnBigerThenNewColumn && !isCorentRowBigerThenNewColumn)
                for (int i = move.from.column + 1, j = move.from.row + 1; i < move.to.column && j < move.to.row && isWayClear; i++, j++)
                    isWayClear = board[i, j] == null;
            if (isCorentColumnBigerThenNewColumn && !isCorentRowBigerThenNewColumn)
                for (int i = move.from.column - 1, j = move.from.row + 1; i > move.to.column && j < move.to.row && isWayClear; i--, j++)
                    isWayClear = board[i, j] == null;
            if (!isCorentColumnBigerThenNewColumn && isCorentRowBigerThenNewColumn)
                for (int i = move.from.column + 1, j = move.from.row - 1; i < move.to.column && j > move.to.row && isWayClear; i++, j--)
                    isWayClear = board[i, j] == null;
            return isWayClear;
        }
    }
    class Queen : ChessPiece
    {
        public Queen(bool isWhite) : base(isWhite) { }
        public override string ToString()
        {
            return base.ToString() + "Q";
        }
        public override bool IsMoveLegal(Move move, bool isWhiteTurn, ChessPiece[,] board)
        {
            return (new Rook(this.isWhite, false).IsMoveLegal(move, isWhiteTurn, board) && new Rook(this.isWhite, false).isWayClear(board, move))
                || (new Bishop(this.isWhite).IsMoveLegal(move, isWhiteTurn, board) && new Bishop(this.isWhite).isWayClear(board, move));
        }
    }
    class King : ChessPiece
    {
        bool canDoCastling;
        public void setCanDoCastling(bool canDoCastling)
        {
            this.canDoCastling = canDoCastling;
        }
        public bool getCanDoCastling()
        {
            return this.canDoCastling;
        }
        public King(bool isWhite, bool canDoCastling) : base(isWhite)
        {
            this.setCanDoCastling(canDoCastling);
        }
        public override string ToString()
        {
            return base.ToString() + "K";
        }
        public override bool IsMoveLegal(Move move, bool isWriteTurn, ChessPiece[,] board)
        {
            return ((Math.Abs(move.from.column - move.to.column) == 1 || Math.Abs(move.from.column - move.to.column) == 0) &&
                (Math.Abs(move.from.row - move.to.row) == 1 || Math.Abs(move.from.row - move.to.row) == 0)
                || (Math.Abs(move.from.row - move.to.row) == 2 && (move.from.column == move.to.column)));
        }
    }
    class Pawn : ChessPiece
    {
        bool canBeEatenInEnPasant;
        public void setCanBeEatenInEnPasant(bool canBeEatenInEnPasant)
        {
            this.canBeEatenInEnPasant = canBeEatenInEnPasant;
        }
        public bool getCanBeEatenInEnPasant()
        {
            return this.canBeEatenInEnPasant;
        }
        public Pawn(bool canBeEatenInEnPasant, bool isWhite) : base(isWhite) { }
        public override string ToString()
        {
            return base.ToString() + "P";
        }
        public override bool IsMoveLegal(Move move, bool isWriteTurn, ChessPiece[,] board)
        {
            if ((isWriteTurn && move.from.column <= move.to.column) || (!isWriteTurn && move.from.column >= move.to.column)) 
                return false;
            if (move.from.row == move.to.row && board[move.to.column, move.to.row] != null)
                return false;
            if (isWriteTurn)
                if (move.from.column == 6 && move.from.row == move.to.row) // לבן- גם נמצא בשורה הראשונה וגם לא הולך לצד
                    return ((Math.Abs(move.from.column - move.to.column) == 1 || (Math.Abs(move.from.column - move.to.column) == 2) && board[move.to.column, move.to.row] == null));
            if (!isWriteTurn)
                if (move.from.column == 1 && move.from.row == move.to.row) // שחור- גם נמצא בשורה הראשונה וגם לא הולך לצד
                    return ((Math.Abs(move.from.column - move.to.column) == 1 || (Math.Abs(move.from.column - move.to.column) == 2) && board[move.to.column, move.to.row] == null));
            if (move.from.row == move.to.row) // הולך ישר
                return Math.Abs(move.from.column - move.to.column) == 1 && board[move.to.column, move.to.row] == null;
            if (move.from.row != move.to.row && board[move.to.column, move.to.row] != null) // הולך לצד ולא למקום ריק
                return ((Math.Abs(move.from.column - move.to.column) == 1 && Math.Abs(move.from.row - move.to.row) == 1));
            if (Math.Abs(move.from.row - move.to.row) == 1 && board[move.to.column, move.to.row] == null && Math.Abs(move.from.column - move.to.column)==1) // הולך לצד מקום ריק en passant
                return board[move.from.column, move.to.row] is Pawn && ((Pawn)board[move.from.column, move.to.row]).canBeEatenInEnPasant;
            return false;
        }
        public override bool isEnPassant(ChessPiece[,] board, Move move)
        {
            return move.from.row != move.to.row && board[move.to.column, move.to.row] == null;
        }
        public override bool isWayClear(ChessPiece[,] board, Move move)
        {
            bool isValid = true;
            if (Math.Abs(move.from.column - move.to.column) == 2)
                isValid = board[(move.from.column > move.to.column ? move.from.column : move.to.column) - 1, move.from.row] == null;
            return isValid;
        }
        public override bool isPromotion(Move move)
        {
            return move.to.column == 0 || move.to.column == 7;
        }
        public override bool setPromotion(Move move, ChessPiece[,] board)
        {
            string input;
            bool isValid = true, firstTime = true;
            do
            {
                Console.WriteLine((firstTime ? "" : "Invalid input, ") + "Write the letter you want- the R for Rook, N for Knight, B for Bishop, Q for Queen and press ENTER");
                firstTime = false;
                input = Console.ReadLine().Trim().ToUpper();
                isValid = true;
                isValid = input.Length != 1;
                if (isValid)
                    isValid = ("RNBQ".Contains(input));
            } while (!isValid);
            if (input == "R")
                board[move.to.column, move.to.row] = new Rook(board[move.to.column, move.to.row].getIsWhite(), false);
            if (input == "N")
                board[move.to.column, move.to.row] = new Knight(board[move.to.column, move.to.row].getIsWhite());
            if (input == "B")
                board[move.to.column, move.to.row] = new Bishop(board[move.to.column, move.to.row].getIsWhite());
            if (input == "Q")
                board[move.to.column, move.to.row] = new Queen(board[move.to.column, move.to.row].getIsWhite());
            return true;
        }
    }
}
