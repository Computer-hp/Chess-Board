using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public class CMatrixBoard
    {
        private const int BOARD_SIZE = 8;
        private const int NUMBER_OF_DIRECTIONS = 8;

        private CPiece[,] mBoard;

        public CPiece[,] Board { get { return mBoard; } set { mBoard = value; } }

        public List<CSquare> ValidMoves { get; set; } = new();

        public List<CSquare> copyMoves { get; set; } = new();


        public Dictionary<Tuple<int, int>, List<CSquare>> stopCheckWithPiece { get; set; } = new();


        private static readonly (string, CSquare)[] linearDirections =
        {
            ( "Up", new CSquare(0, 1) ),
            ( "Down", new CSquare(0, -1) ),
            ( "Right", new CSquare(1, 0) ),
            ( "Left", new CSquare(-1, 0) ),
            ( "RightUp", new CSquare(1, 1) ),
            ( "RightDown", new CSquare(1, -1) ),
            ( "LeftUp", new CSquare(-1, 1) ),
            ( "LeftDown", new CSquare(-1, -1) )
        };


        private static readonly (int x, int y)[] knightMoves = 
        {
            (2, 1), (2, -1), (-2, 1), (-2, -1),
            (1, 2), (1, -2), (-1, 2), (-1, -2)
        };


        public int MovePawnTowardsBlackOrWhite { get; set; } = -1;



        public CMatrixBoard()
        {
            Board = new CPiece[BOARD_SIZE, BOARD_SIZE];
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
                Board[secondRank, x] = new CPiece(x, secondRank, "P", PieceColor.White);
                Board[firstRank, x] = new CPiece(x, firstRank, pieces[x], PieceColor.White);

                Board[seventhRank, x] = new CPiece(x, seventhRank, "P", PieceColor.Black);
                Board[eightRank, x] = new CPiece(x, eightRank, pieces[x], PieceColor.Black);
            }
        }


        public void CalculateMoves(CPiece piece, string direction)
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

        private void PawnMoves(CPiece pawn)
        {
            int movePawnTowardsBlackOrWhite = (pawn.pieceType == PieceColor.White) ? -1 : 1;

            int destinationRank = pawn.y + movePawnTowardsBlackOrWhite;

            if (IsSquareOutsideTheBoard(pawn.x, destinationRank))
                return;

            if (IsSquareNull(pawn.x, destinationRank))
                ValidMoves.Add(new CSquare(pawn.x, destinationRank));

            for (int x = pawn.x - 1; x < pawn.x + 2; x += 2)
            {
                if (IsSquareOutsideTheBoard(x, destinationRank))
                    continue;
                
                ValidMoves.Add(new CSquare(x, destinationRank));
            }

            int destinationRankOfFirstPawnMove = pawn.y + movePawnTowardsBlackOrWhite * 2;

            if (!IsPawnBeingMovedForTheFirstTime(pawn) ||
                IsSquareOutsideTheBoard(pawn.x, destinationRankOfFirstPawnMove) ||

                !IsSquareNull(pawn.x, destinationRankOfFirstPawnMove))
                return;

            ValidMoves.Add(new CSquare(pawn.x, destinationRankOfFirstPawnMove));
        }


        private bool IsPawnBeingMovedForTheFirstTime(CPiece piece)
        {
            return ((piece.pieceType == PieceColor.White && piece.y == (int)Ranks.SecondRank) ||
                    (piece.pieceType == PieceColor.Black && piece.y == (int)Ranks.SeventhRank));
        }



        private void ManageLinearDirections(CPiece piece, string direction)
        {
            int times = (piece.pieceName == "K") ? 1 : BOARD_SIZE;

            if (!string.IsNullOrEmpty(direction))
            {
                CSquare incrementForNextSquare = linearDirections.First(storedDirection => storedDirection.Item1 == direction).Item2;
                CalculateLinearDirections(piece, incrementForNextSquare, times);
                return;
            }

            int startingIndexForDirections = (piece.pieceName == "R") ? 0 
                                                                         : (piece.pieceName == "B") ? 4 : 0;

            int endingIndexForDirections = (piece.pieceName == "R") ? NUMBER_OF_DIRECTIONS / 2 : NUMBER_OF_DIRECTIONS;

            for (int i = startingIndexForDirections; i < endingIndexForDirections; i++)
                CalculateLinearDirections(piece, linearDirections[i].Item2, times);
        }


        private void CalculateLinearDirections(CPiece piece, CSquare incrementForNextSquare, int times)
        {
            int destinationX = piece.x, destinationY = piece.y;

            for (int i = 0; i < times; i++)
            {
                destinationX += incrementForNextSquare.x;
                destinationY += incrementForNextSquare.y;

                if (IsSquareOutsideTheBoard(destinationX, destinationY))
                    return;

                if (!IsSquareNull(destinationX, destinationY))
                {
                    ValidMoves.Add(new CSquare(destinationX, destinationY));
                    return;
                }

                ValidMoves.Add(new CSquare(destinationX, destinationY));
            }
        }


        // knight is able to move when check
        private void KnightMoves(CPiece piece)
        {
            foreach (var move in knightMoves)
            {
                int newX = piece.x + move.x;
                int newY = piece.y + move.y;

                if (!IsSquareOutsideTheBoard(newX, newY))
                    ValidMoves.Add(new CSquare(newX, newY));
            }                
        }


        public bool IsSquareOutsideTheBoard(int destinationX, int destinationY)
        {
            return (destinationX < 0 || destinationX >= BOARD_SIZE ||
                    destinationY < 0 || destinationY >= BOARD_SIZE);
        }


        public bool IsSquareNull(int destinationX, int destinationY)
        {
            return (Board[destinationY, destinationX] == null);
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
