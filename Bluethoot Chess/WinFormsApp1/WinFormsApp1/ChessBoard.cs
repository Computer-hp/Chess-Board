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

        public List<CSquare> invalidSquaresKing { get; set; } = new();

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
                ( "LeftDown", new CSquare(-1, -1) ),
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
                Board[x, 1] = new CPiece(x, 1, "P", "white");
                Board[x, 0] = new CPiece(x, 0, pieces[x], "white");

                Board[x, 6] = new CPiece(x, 6, "P", "black");
                Board[x, 7] = new CPiece(x, 7, pieces[x], "black");
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

            int times = (piece.pieceName == "K") ? 1 : BOARD_SIZE;
            int endingIndexForDirections = (piece.pieceName == "B") ? NUMBER_OF_DIRECTIONS / 2 : NUMBER_OF_DIRECTIONS;

            ManageLinearDirections(piece, times, endingIndexForDirections, direction);
        }


        /*
            Calculates the legal pawn moves. 
        */

        private void PawnMoves(CPiece piece)
        {
            int squareUpOrDown = (piece.pieceType == "white") ? 1 : -1;

            bool firstMove = ((piece.pieceType == "white" && piece.y == 1) ||
                              (piece.pieceType == "black" && piece.y == 6))
                                ? true : false;

            if (piece.y + squareUpOrDown < 0 || piece.y + squareUpOrDown >= BOARD_SIZE)
                return;

            if (Board[piece.x, piece.y + squareUpOrDown] == null)
                validMoves.Add(new CSquare(piece.x, piece.y + squareUpOrDown));

            if (firstMove && Board[piece.x, piece.y + (squareUpOrDown * 2)] == null)
                validMoves.Add(new CSquare(piece.x, piece.y + (squareUpOrDown * 2)));

            validMoves.Add(new CSquare(piece.x + squareUpOrDown, piece.y + squareUpOrDown));
            validMoves.Add(new CSquare(piece.x - squareUpOrDown, piece.y + squareUpOrDown));

            validMoves.RemoveAll(square => square.x < 0 || square.x >= BOARD_SIZE);
        }


        // Some errors when moving diagonaly
        private void ManageLinearDirections(CPiece piece, int times, int endingIndexForDirections, string direction)
        {
            if (!string.IsNullOrEmpty(direction))
            {
                CalculateLinearDirections(piece, linearDirections.First(storedDirection => storedDirection.Item1 == direction).Item2, times);
                return;
            }

            int startingIndexForDirections = (piece.pieceName == "R") ? 0 : (piece.pieceName == "B") ? 4 : 1;

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

                if (destinationX >= BOARD_SIZE ||
                    destinationY >= BOARD_SIZE ||
                    destinationX < 0 ||
                    destinationY < 0)

                    return;

                CPiece? tmpPiece = Board[destinationX, destinationY];

                if (tmpPiece != null)
                {
                    validMoves.Add(new CSquare(destinationX, destinationY));
                    return;
                }

                validMoves.Add(new CSquare(destinationX, destinationY));
            }
        }


        private void KnightMoves(CPiece piece)
        {
            foreach (var move in knightMoves)
            {
                int newX = piece.x + move.x;
                int newY = piece.y + move.y;

                if (newX >= 0 && newX < BOARD_SIZE && newY >= 0 && newY < BOARD_SIZE)
                    validMoves.Add(new CSquare(newX, newY));
            }                
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
