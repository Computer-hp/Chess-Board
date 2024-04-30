using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public class ChessBoard : IEnumerable<Piece>
    {
        private const int BOARD_SIZE = 8;
        private const int NUMBER_OF_DIRECTIONS = 8;

        private Piece[,] board;
           
        private static readonly (string, Square)[] linearDirections =
        {
            ( "Up", new Square(0, 1) ),
            ( "Down", new Square(0, -1) ),
            ( "Right", new Square(1, 0) ),
            ( "Left", new Square(-1, 0) ),
            ( "RightUp", new Square(1, 1) ),
            ( "RightDown", new Square(1, -1) ),
            ( "LeftUp", new Square(-1, 1) ),
            ( "LeftDown", new Square(-1, -1) )
        };

        private static readonly (int x, int y)[] knightMoves = 
        {
            (2, 1), (2, -1), (-2, 1), (-2, -1),
            (1, 2), (1, -2), (-1, 2), (-1, -2)
        };


        public List<Square> ValidMoves { get; set; } = new();

        public List<Square> CopyMoves { get; set; } = new();

        public Dictionary<Tuple<int, int>, List<Square>> StopCheckWithPiece { get; set; } = new();

        public int MovePawnTowardsBlackOrWhite { get; set; } = -1;

        public Piece this[int y, int x]
        {
            get 
            {
                if (IsSquareOutsideTheBoard(x, y)) throw new IndexOutOfRangeException();
                return board[y, x]; 
            }
            set 
            { 
                if (IsSquareOutsideTheBoard(x, y)) throw new IndexOutOfRangeException();
                board[y, x] = value; 
            }
        }

        public IEnumerator<Piece> GetEnumerator()
        {
            foreach (Piece piece in board)
                yield return piece;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ref Piece GetPieceRef(int y, int x)
        {
            if (IsSquareOutsideTheBoard(x, y)) throw new IndexOutOfRangeException();

            return ref board[y, x];
        }
         


        public ChessBoard()
        {
            board = new Piece[BOARD_SIZE, BOARD_SIZE];
        }


        /*
            Adds pieces to the board. 
        */

        public void InitializePieces()
        {
            string[] pieces = { "R", "N", "B", "Q", "K", "B", "N", "R" };

            int firstRank = (int)Ranks.FirstRank;
            int secondRank = (int)Ranks.SecondRank;
            int seventhRank = (int)Ranks.SeventhRank;
            int eightRank = (int)Ranks.EighthRank;

            for (int x = 0; x < BOARD_SIZE; x++)
            {
                board[secondRank, x] = new Piece(x, secondRank, "P", PieceColor.White);
                board[firstRank, x] = new Piece(x, firstRank, pieces[x], PieceColor.White);

                board[seventhRank, x] = new Piece(x, seventhRank, "P", PieceColor.Black);
                board[eightRank, x] = new Piece(x, eightRank, pieces[x], PieceColor.Black);
            }
        }


        public void CalculateMoves(Piece piece, string direction)
        {
            ValidMoves.Clear();

            switch (piece.pieceName)
            {
                case "P":
                    PawnMoves(piece);
                    return;

                case "N":
                    KnightMoves(piece);
                    return;
            }

            ManageLinearDirections(piece, direction);
        }


        /*
            Calculates the legal and illegal pawn moves.
        */

        // TODO   En passant

        private void PawnMoves(Piece pawn)
        {
            int movePawnTowardsBlackOrWhite = (pawn.pieceType == PieceColor.White) ? -1 : 1;
            int destinationRank = pawn.y + movePawnTowardsBlackOrWhite;

            if (IsSquareOutsideTheBoard(pawn.x, destinationRank)) return;

            if (IsSquareNull(pawn.x, destinationRank)) ValidMoves.Add(new Square(pawn.x, destinationRank));

            for (int x = pawn.x - 1; x < pawn.x + 2; x += 2)
            {
                if (IsSquareOutsideTheBoard(x, destinationRank)) continue;
                ValidMoves.Add(new Square(x, destinationRank));
            }

            int destinationRankOfFirstPawnMove = pawn.y + movePawnTowardsBlackOrWhite * 2;

            if (!IsPawnBeingMovedForTheFirstTime(pawn) ||
                IsSquareOutsideTheBoard(pawn.x, destinationRankOfFirstPawnMove) ||
                !IsSquareNull(pawn.x, destinationRankOfFirstPawnMove)) 
                return;

            ValidMoves.Add(new Square(pawn.x, destinationRankOfFirstPawnMove));
        }


        private static bool IsPawnBeingMovedForTheFirstTime(Piece piece)
        {
            return ((piece.pieceType == PieceColor.White && piece.y == (int)Ranks.SecondRank) ||
                    (piece.pieceType == PieceColor.Black && piece.y == (int)Ranks.SeventhRank));
        }



        private void ManageLinearDirections(Piece piece, string direction)
        {
            int times = (piece.pieceName == "K") ? 1 : BOARD_SIZE;

            if (!string.IsNullOrEmpty(direction))
            {
                Square incrementForNextSquare = linearDirections.First(storedDirection => storedDirection.Item1 == direction).Item2;
                CalculateLinearDirections(piece, incrementForNextSquare, times);
                return;
            }

            int startingIndexForDirections = (piece.pieceName == "R") ? 0 
                                             : (piece.pieceName == "B") ? 4 : 0;

            int endingIndexForDirections = (piece.pieceName == "R") ? NUMBER_OF_DIRECTIONS / 2 : NUMBER_OF_DIRECTIONS;

            for (int i = startingIndexForDirections; i < endingIndexForDirections; i++)
                CalculateLinearDirections(piece, linearDirections[i].Item2, times);
        }


        private void CalculateLinearDirections(Piece piece, Square incrementForNextSquare, int times)
        {
            int destinationX = piece.x, destinationY = piece.y;

            for (int i = 0; i < times; i++)
            {
                destinationX += incrementForNextSquare.x;
                destinationY += incrementForNextSquare.y;

                if (IsSquareOutsideTheBoard(destinationX, destinationY)) return;

                if (!IsSquareNull(destinationX, destinationY))
                {
                    ValidMoves.Add(new Square(destinationX, destinationY));
                    return;
                }

                ValidMoves.Add(new Square(destinationX, destinationY));
            }
        }


        // knight is able to move when check
        private void KnightMoves(Piece piece)
        {
            foreach (var move in knightMoves)
            {
                int newX = piece.x + move.x;
                int newY = piece.y + move.y;

                if (!IsSquareOutsideTheBoard(newX, newY))
                    ValidMoves.Add(new Square(newX, newY));
            }                
        }


        public static bool IsSquareOutsideTheBoard(int destinationX, int destinationY)
        {
            return (destinationX < 0 || destinationX >= BOARD_SIZE ||
                    destinationY < 0 || destinationY >= BOARD_SIZE);
        }


        public bool IsSquareNull(int destinationX, int destinationY)
        {
            return (board[destinationY, destinationX] == null);
        }


        public override string ToString()
        {
            string output = "";

            foreach (var element in ValidMoves)
                output += element.x + "," + element.y + " ";

            return output;
        }
    }
}
