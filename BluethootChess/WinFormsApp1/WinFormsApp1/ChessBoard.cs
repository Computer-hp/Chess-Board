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

        private static readonly (Directions direction, (int x, int y) moveBy)[] linearDirections =
        {
            ( Directions.Left,       (-1, 0)  ),
            ( Directions.Right,      (1, 0)   ),
            ( Directions.Up,         (0, 1)   ),
            ( Directions.Down,       (0, -1)  ),

            ( Directions.LeftDown,   (-1, -1) ),
            ( Directions.LeftUp,     (-1, 1)  ),
            ( Directions.RightUp,    (1, 1)   ),
            ( Directions.RightDown,  (1, -1)  ),
        };

        private static readonly (int x, int y)[] knightMoves = 
        {
            (2, 1), (2, -1), (-2, 1), (-2, -1),
            (1, 2), (1, -2), (-1, 2), (-1, -2)
        };

        // maybe is better to use HashSet then List, for ValidMoves and CopyMoves
        public List<(int x, int y)> ValidMoves { get; set; } = new();

        public List<(int x, int y)> CopyMoves { get; set; } = new();

        public Dictionary<(int x, int y), List<(int validX, int validY)>> StopCheckWithPiece { get; set; } = new();

        public int MovePawnTowardsBlackOrWhite { get; set; } = -1;

        public Piece this[int y, int x]
        {
            get 
            {
                if (IsSquareOutsideTheBoard((x, y))) throw new IndexOutOfRangeException();
                return board[y, x]; 
            }
            set 
            { 
                if (IsSquareOutsideTheBoard((x, y))) throw new IndexOutOfRangeException();
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

        /*
        public ref Piece GetPieceRef(int y, int x)
        {
            if (IsSquareOutsideTheBoard((x, y))) throw new IndexOutOfRangeException();

            return ref board[y, x];
        }
        */ 


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


        public void CalculateMoves(Piece piece, Directions? direction = null)
        {
            ValidMoves.Clear();

            switch (piece.Name)
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
            int movePawnTowardsBlackOrWhite = (pawn.Color == PieceColor.White) ? -1 : 1;
            int destinationRank = pawn.y + movePawnTowardsBlackOrWhite;

            if (IsSquareOutsideTheBoard((pawn.x, destinationRank))) return;

            if (IsSquareNull((pawn.x, destinationRank))) ValidMoves.Add((pawn.x, destinationRank));

            for (int x = pawn.x - 1; x < pawn.x + 2; x += 2)
            {
                if (IsSquareOutsideTheBoard((x, destinationRank))) continue;
                ValidMoves.Add((x, destinationRank));
            }

            int destinationRankOfFirstPawnMove = pawn.y + movePawnTowardsBlackOrWhite * 2;

            if (!IsPawnBeingMovedForTheFirstTime(pawn) ||
                IsSquareOutsideTheBoard((pawn.x, destinationRankOfFirstPawnMove)) ||
                !IsSquareNull((pawn.x, destinationRankOfFirstPawnMove))) 
                return;

            ValidMoves.Add((pawn.x, destinationRankOfFirstPawnMove));
        }


        private static bool IsPawnBeingMovedForTheFirstTime(Piece piece)
        {
            return ((piece.Color == PieceColor.White && piece.y == (int)Ranks.SecondRank) ||
                    (piece.Color == PieceColor.Black && piece.y == (int)Ranks.SeventhRank));
        }



        private void ManageLinearDirections(Piece piece, Directions? direction)
        {
            int times = (piece.Name == "K") ? 1 : BOARD_SIZE;

            if (direction == null)
            {
                ValueTuple<int, int> incrementForNextSquare = linearDirections.First(storedDirection => storedDirection.direction == direction).moveBy;
                CalculateLinearDirections(piece, incrementForNextSquare, times);
                return;
            }

            int startingIndexForDirections = (piece.Name == "R") ? 0 
                                             : (piece.Name == "B") ? 4 : 0;

            int endingIndexForDirections = (piece.Name == "R") ? NUMBER_OF_DIRECTIONS / 2 : NUMBER_OF_DIRECTIONS;

            for (int i = startingIndexForDirections; i < endingIndexForDirections; i++)
                CalculateLinearDirections(piece, linearDirections[i].moveBy, times);
        }


        private void CalculateLinearDirections(Piece piece, (int x, int y) incrementForNextSquare, int times)
        {
            int destinationX = piece.x, destinationY = piece.y;

            for (int i = 0; i < times; i++)
            {
                destinationX += incrementForNextSquare.x;
                destinationY += incrementForNextSquare.y;

                if (IsSquareOutsideTheBoard((destinationX, destinationY))) return;

                if (!IsSquareNull((destinationX, destinationY)))
                {
                    ValidMoves.Add((destinationX, destinationY));
                    return;
                }

                ValidMoves.Add((destinationX, destinationY));
            }
        }


        // knight is able to move when check
        private void KnightMoves(Piece piece)
        {
            foreach (var move in knightMoves)
            {
                int newX = piece.x + move.x;
                int newY = piece.y + move.y;

                if (!IsSquareOutsideTheBoard((newX, newY))) ValidMoves.Add((newX, newY));
            }                
        }


        public static bool IsSquareOutsideTheBoard((int x, int y) destinationSquare)
        {
            return (destinationSquare.x < 0 || destinationSquare.x >= BOARD_SIZE ||
                    destinationSquare.y < 0 || destinationSquare.y >= BOARD_SIZE);
        }


        public bool IsSquareNull((int x, int y) destinationSquare)
        {
            return (board[destinationSquare.y, destinationSquare.x] == null);
        }


        public override string ToString()
        {
            string output = "";

            foreach (var element in ValidMoves)
                output += element.x + "," + element.y + " ";

            return output;
        }


        public void PrintMovesThatCanStopCheck()
        {
            Debug.Write("\nStopCheckWithPiece = \n");
            
            foreach (var kvp in this.StopCheckWithPiece)
            {
                Debug.Write($"Key: ({kvp.Key.Item1}, {kvp.Key.Item2}), Squares: ");

                foreach (var square in kvp.Value)
                    Debug.Write($"{square.validX}, {square.validY} ");

                Debug.Write('\n');
            }

            Debug.Write('\n');
        }
    }
}
