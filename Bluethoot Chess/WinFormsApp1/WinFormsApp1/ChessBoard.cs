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
        private const int N_DIRECTIONS = 4;

        private int x, y, oppositeX, oppositeY;

        private bool right, left, up, down;

        private bool rightUp, leftUp, rightDown, leftDown;

        private CPiece[,] mBoard;

        public CPiece[,] Board { get { return mBoard; } set { mBoard = value; } }

        public List<CSquare> validMoves { get; set; } = new();

        public List<CSquare> copyMoves { get; set; } = new();

        public List<CSquare> invalidSquaresKing { get; set; } = new();

        public Dictionary<Tuple<int, int>, List<CSquare>> stopCheckWithPiece { get; set; } = new();



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
            /*x = P.x; y = P.y;

//            int counter = 2;

            right = true; left = true; up = true; down = true;

            for (int i = 0; i < times; i++)
            {
    //            x++; y++;
  //              oppositeX = x - counter; oppositeY = y - counter;

                switch (direction)
                {
                    case "":
                        Up(this, P, i + 1);
                        Down(this, P);
                        Left(this, P);
                        Right(this, P);
                        break;

                    case "Up":
                        Up(this, P, i + 1);
                        break;

                    case "Down":
                        Down(this, P);
                        break;

                    case "Left":
                        Left(this, P);
                        break;

                    case "Right":
                        Right(this, P);
                        break;
                }

//                counter += 2;
            }
            */

            CSquare? incrementForNextSquare = null;


            for (int i = 0; i < N_DIRECTIONS; i++)
            {
                switch (direction)
                {
                    case "Up":
                        incrementForNextSquare = new(0, 1);
                        break;


                    case "Down":
                        incrementForNextSquare = new(0, -1);
                        break;


                    case "Right":
                        incrementForNextSquare = new(1, 0);
                        break;


                    case "Left":
                        incrementForNextSquare = new(-1, 0);
                        break;
                }

                CalculateStraightDirections(P, incrementForNextSquare);
            }
        }
        
                                                       // using switch case determine the value; if direction == Up --> incrementForNextSquare.x = 0 .y = 1
                                                                                                              // Down ---> .x = 0 .y = -1
        public void CalculateStraightDirections(CPiece P, CSquare? incrementForNextSquare)
        {
            int destinationX = P.x, destinationY = P.y;

            for (int i = 1; i < BOARD_SIZE; i++)
            {
                destinationX += incrementForNextSquare.x;
                destinationY += incrementForNextSquare.y;

                CPiece tmpPiece = Board[destinationX, destinationY];

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

/*
        public void Up(CMatrixBoard B, CPiece P)
        {
            if (y >= boardSize || !up)
                return;

            validMoves.Add(new CSquare(P.x, y));

            if (B.Board[P.x, y] != null)
                up = false;
        }*/

        /*
        public void Up(CMatrixBoard B, CPiece P, int incrementForNextSquare)
        {
            int nextSquare = P.y + incrementForNextSquare;
            
            if (nextSquare > boardSize)
                return;

            validMoves.Add(new CSquare(P.x, nextSquare));

            if (B.Board[P.x, y] != null)
                up = false;
        }


        public void Down(CMatrixBoard B, CPiece P)
        {
            if (oppositeY < 0 || !down)
                return;

            validMoves.Add(new CSquare(P.x, oppositeY));

            if (B.Board[P.x, oppositeY] != null)
                down = false;
        }


        public void Left(CMatrixBoard B, CPiece P)
        {
            if (oppositeX < 0 || !left)
                return;

            validMoves.Add(new CSquare(oppositeX, P.y));

            if (B.Board[oppositeX, P.y] != null)
                left = false;
        }


        public void Right(CMatrixBoard B, CPiece P)
        {
            if (x >= boardSize || !right)
                return;

            validMoves.Add(new CSquare(x, P.y));

            if (B.Board[x, P.y] != null)
                right = false;
        }
        */


        public void Diagonal(CPiece P, int times, string direction)
        {
            /*
            x = P.x; y = P.y;

            int counter = 2;

            rightUp = true; leftUp = true; rightDown = true; leftDown = true;

            for (int i = 0; i < times; i++)
            {
                x++; y++;
                oppositeX = x - counter; oppositeY = y - counter;

                switch (direction)
                {
                    case "":
                        RightUp(this, P);
                        RightDown(this, P);
                        LeftUp(this, P);
                        LeftDown(this, P);
                        break;

                    case "RightUp":
                        RightUp(this, P);
                        break;

                    case "RightDown":
                        RightDown(this, P);
                        break;

                    case "LeftUp":
                        LeftUp(this, P);
                        break;

                    case "LeftDown":
                        LeftDown(this, P);
                        break;
                }

                counter += 2;
            }
            */



        }

/*
        public void RightUp(CMatrixBoard B, CPiece P)
        {
            if (x >= boardSize || y >= boardSize || !rightUp)
                return;

            validMoves.Add(new CSquare(x, y));

            if (B.Board[x, y] != null)
                rightUp = false;
        }


        public void RightDown(CMatrixBoard B, CPiece P)
        {
            if (x >= boardSize || oppositeY < 0 || !rightDown)
                return;

            validMoves.Add(new CSquare(x, oppositeY));

            if (B.Board[x, oppositeY] != null)
                rightDown = false;
        }


        public void LeftUp(CMatrixBoard B, CPiece P)
        {
            if (oppositeX < 0 || y >= boardSize || !leftUp)
                return;

            validMoves.Add(new CSquare(oppositeX, y));

            if (B.Board[oppositeX, y] != null)
                leftUp = false;
        }


        public void LeftDown(CMatrixBoard B, CPiece P)
        {
            if (oppositeX < 0 || oppositeY < 0 || oppositeY >= boardSize || !leftDown)
                return;

            validMoves.Add(new CSquare(oppositeX, oppositeY));

            if (B.Board[oppositeX, oppositeY] != null)
                leftDown = false;
        }
*/

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
