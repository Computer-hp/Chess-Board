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

        public List<CSquare> validMoves { get; set; } = new();

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

            for (int x = 0; x < 8; x++)
            {
                Board[x, 1] = new CPiece(x, (int)Ranks.SecondRank, "P", PieceColor.White);
                Board[x, 0] = new CPiece(x, (int)Ranks.FirstRank, pieces[x], PieceColor.White);

                Board[x, 6] = new CPiece(x, (int)Ranks.SeventhRank, "P", PieceColor.Black);
                Board[x, 7] = new CPiece(x, (int)Ranks.EighthRank, pieces[x], PieceColor.Black);
            }
        }


        public void CalculateMoves(CPiece piece, string direction)
        {
            validMoves.Clear();

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
            Calculates the legal pawn moves. 
        */

        // TODO   En passant

        private void PawnMoves(CPiece piece)
        {
            int movePawnTowardsBlackOrWhite = ChessBoardForm.MoveTowardsBlackOrWhite;

            if (piece.y + movePawnTowardsBlackOrWhite < 0 || 
                piece.y + movePawnTowardsBlackOrWhite >= BOARD_SIZE)

                return;

            Debug.WriteLine($"\nFirst Rank = {(int)Ranks.FirstRank}\n");

            bool firstMoveIsValid = ((piece.pieceType == PieceColor.White && piece.y == (int)Ranks.SecondRank) ||
                                     (piece.pieceType == PieceColor.Black && piece.y == (int)Ranks.SeventhRank))
                                     ? true : false;

            if (IsSquareNull(piece.x, piece.y + movePawnTowardsBlackOrWhite))

                validMoves.Add(new CSquare(piece.x, piece.y + movePawnTowardsBlackOrWhite));


            if (firstMoveIsValid && 
                IsSquareNull(piece.x, piece.y + (movePawnTowardsBlackOrWhite * 2)))

                validMoves.Add(new CSquare(piece.x, piece.y + (movePawnTowardsBlackOrWhite * 2)));


            validMoves.Add(new CSquare(piece.x + movePawnTowardsBlackOrWhite, piece.y + movePawnTowardsBlackOrWhite));
            validMoves.Add(new CSquare(piece.x - movePawnTowardsBlackOrWhite, piece.y + movePawnTowardsBlackOrWhite));

            validMoves.RemoveAll(square => square.x < 0 || square.x >= BOARD_SIZE);
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

            int startingIndexForDirections = (piece.pieceName == "R") ? 0 : (piece.pieceName == "B") ? 4 : 1;
            int endingIndexForDirections = (piece.pieceName == "R")
                                            ? NUMBER_OF_DIRECTIONS / 2 : NUMBER_OF_DIRECTIONS;

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
                    validMoves.Add(new CSquare(destinationX, destinationY));
                    return;
                }

                validMoves.Add(new CSquare(destinationX, destinationY));
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
                    validMoves.Add(new CSquare(newX, newY));
            }                
        }



        public bool IsSquareNull(int destinationX, int destinationY)
        {
            return (!IsSquareOutsideTheBoard(destinationX, destinationY))

                ? (Board[destinationX, destinationY] == null)
                : false; //throw new IndexOutOfRangeException("\nThrown an exception due to incorrect coordinates\n");
        }


        public bool IsSquareOutsideTheBoard(int destinationX, int destinationY)
        {
            return (destinationX < 0 || destinationX >= BOARD_SIZE ||
                    destinationY < 0 || destinationY >= BOARD_SIZE);
        }



        public override string ToString()
        {
            string output = "";

            foreach (var element in validMoves)
                output += element.x + "," + element.y + " ";

            return output;
        }
    }
}
