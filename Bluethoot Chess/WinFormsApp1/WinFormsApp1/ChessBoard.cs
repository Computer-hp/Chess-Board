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

        private int x, y, oppositeX, oppositeY;

        private CPiece[,] mBoard;

        public CPiece[,] Board { get { return mBoard; } set { mBoard = value; } }

        public List<CSquare> validMoves { get; set; } = new();

        public List<CSquare> copyMoves { get; set; } = new();

        public List<CSquare> invalidSquaresKing { get; set; } = new();

        public Dictionary<Tuple<int, int>, List<CSquare>> stopCheckWithPiece { get; set; } = new();

        private static readonly Dictionary<string, CSquare> straightDirections = new()
        {
            { "Up", new CSquare(0, 1) },
            { "Down", new CSquare(0, -1) },
            { "Right", new CSquare(1, 0) },
            { "Left", new CSquare(-1, 0) }
        };

        private static readonly Dictionary<string, CSquare> diagonalDirections = new()
        {
            { "RightUp", new CSquare(1, 1) },
            { "RightDown", new CSquare(1, -1) },
            { "LeftUp", new CSquare(-1, 1) },
            { "LeftDown", new CSquare(-1, -1) }
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



        /*
            Calculates the legal pawn moves. 
        */

        public void Pawns(CPiece P)
        {
            int squareUpOrDown = (P.pieceType == "white") ? 1 : -1;

            bool firstMove = ((P.pieceType == "white" && P.y == 1) || 
                              (P.pieceType == "black" && P.y == 6))
                                ? true : false;

            if (P.y + squareUpOrDown < 0 || P.y + squareUpOrDown >= BOARD_SIZE)
                return;

            if (Board[P.x, P.y + squareUpOrDown] == null)
                validMoves.Add(new CSquare(P.x, P.y + squareUpOrDown));

            if (firstMove && Board[P.x, P.y + (squareUpOrDown * 2)] == null)
                validMoves.Add(new CSquare(P.x, P.y + (squareUpOrDown * 2)));

            validMoves.Add(new CSquare(P.x + squareUpOrDown, P.y + squareUpOrDown));
            validMoves.Add(new CSquare(P.x - squareUpOrDown, P.y + squareUpOrDown));

            validMoves.RemoveAll(square => square.x < 0 || square.x >= BOARD_SIZE);
        }



        public void Straight(CPiece P, int times, string direction)
        {
            if (!string.IsNullOrEmpty(direction))
            {
                CalculateDirections(P, straightDirections[direction], times);
                return;
            }

            foreach (var kvp in straightDirections)
                CalculateDirections(P, kvp.Value, times);
        }


        public void Diagonal(CPiece P, int times, string direction)
        {
            if (!string.IsNullOrEmpty(direction))
            {
                CalculateDirections(P, diagonalDirections[direction], times);
                return;
            }

            foreach (var kvp in diagonalDirections)
                CalculateDirections(P, kvp.Value, times);
        }


        public void CalculateDirections(CPiece P, CSquare incrementForNextSquare, int times)
        {
            int destinationX = P.x, destinationY = P.y;

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
                    if (tmpPiece.pieceType == P.pieceType)
                        return;

                    validMoves.Add(new CSquare(destinationX, destinationY));
                    return;
                }

                validMoves.Add(new CSquare(destinationX, destinationY));
            }
        }


        public void Jump(CPiece P)
        {
            x = P.x; y = P.y;

            int counterX = 2;
            int counterY = 4;

            for (int i = 0; i < 2; i++)
            {
                int provisoryX = x + 1;
                int provisoryY = y + 2;

                oppositeX = provisoryX - counterX; 
                oppositeY = provisoryY - counterY;

                JumpRight(provisoryX, provisoryY);
                JumpLeft(provisoryY);


                x++;
                y--;
                counterY -= 2;
                counterX += 2;
            }
        }


        private void JumpRight(int provisoryX, int provisoryY)
        {
            if (provisoryX >= BOARD_SIZE)
                return;

            if (provisoryY < BOARD_SIZE)
                validMoves.Add(new CSquare(provisoryX, provisoryY));

            if (oppositeY >= 0)
                validMoves.Add(new CSquare(provisoryX, oppositeY));
        }


        private void JumpLeft(int provisoryY)
        {
            if (oppositeX < 0)
                return;

            if (provisoryY < BOARD_SIZE)
                validMoves.Add(new CSquare(oppositeX, provisoryY));

            if (oppositeY >= 0)
                validMoves.Add(new CSquare(oppositeX, oppositeY));
        }


        public void CheckKnightMoves(CPiece P)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (Board[x, y] != null && Board[x, y].pieceType == P.pieceType)
                        if (validMoves.Exists(item => item.x == x && item.y == y) == true)
                            validMoves.RemoveAll(item => item.x == x && item.y == y);
                }
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
