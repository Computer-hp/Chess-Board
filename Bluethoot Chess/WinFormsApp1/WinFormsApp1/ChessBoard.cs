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
        private const int boardSize = 8;

        private CPiece[,] mBoard;

        private MethodInfo? method = null;

        private int x, y, oppositeX, oppositeY;

        private bool right, left, up, down;

        private bool rightUp, leftUp, rightDown, leftDown;
        
        public CPiece[,] Board { get { return mBoard; } set { mBoard = value; } }

        public List<CSquare> validMoves { get; set; } = new();

        public List<CSquare> copyMoves { get; set; } = new();

        public List<CSquare> invalidSquaresKing { get; set; } = new();

        public Dictionary<Tuple<int, int>, List<CSquare>> stopCheckWithPiece { get; set; } = new();

        public CMatrixBoard()
        {
            Board = new CPiece[boardSize, boardSize];
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
            if (P.pieceType == "white" && P.y < boardSize) 
            {
                if (Board[P.x, P.y + 1] == null)
                    validMoves.Add(new CSquare(P.x, P.y + 1));

                CheckRigth(1, 1, P);
                CheckLeft(-1, 1, P);

                if (P.y == 1 && Board[P.x, P.y + 2] == null)
                    validMoves.Add(new CSquare(P.x, P.y + 2));
            }

            if (P.pieceType == "black" && P.y >= 0)
            {
                if (Board[P.x, P.y - 1] == null)
                    validMoves.Add(new CSquare(P.x, P.y - 1));

                CheckRigth(1, -1, P);
                CheckLeft(-1, -1, P);

                if (P.y == 6 && Board[P.x, P.y - 2] == null)
                    validMoves.Add(new CSquare(P.x, P.y - 2));
            }
        }

        
        private void CheckRigth(int X, int Y, CPiece P)
        {
            int rightX = P.x + X, upORdown = P.y + Y;

            if (rightX >= boardSize || (upORdown >= boardSize || upORdown < 0))
                return;

            validMoves.Add(new CSquare(rightX, upORdown));
        }

        private void CheckLeft(int X, int Y, CPiece P)
        {
            int leftX = P.x + X, upORdown = P.y + Y;

            if (leftX < 0 || (upORdown >= boardSize || upORdown < 0))
                return;

            validMoves.Add(new CSquare(leftX, upORdown));
        }

        public void Straight(CPiece P, int times, string direction)
        {
            x = P.x; y = P.y;

            int counter = 2;

            right = true; left = true; up = true; down = true;

            for (int i = 0; i < times; i++)
            {
                x++; y++;
                oppositeX = x - counter; oppositeY = y - counter;

                if (direction != "")
                {
                    method = typeof(CMatrixBoard).GetMethod(direction);
                    object[] parameters = new object[] { this, P };
                    method.Invoke(this, parameters);
                }
                else
                {
                    Up(this, P);
                    Down(this, P);
                    Left(this, P);
                    Right(this, P);
                }

                counter += 2;
            }
        }

        public void Up(CMatrixBoard B, CPiece P)
        {
            if (y < boardSize && up)
            {
                if (B.Board[P.x, y] == null) {
                    validMoves.Add(new CSquare(P.x, y));
                    return;
                }

                //if (B.Board[P.x, y].pieceType != P.pieceType)
                //if (P.pieceName != "K")
                    validMoves.Add(new CSquare(P.x, y));

                up = false;
            }
        }
        public void Down(CMatrixBoard B, CPiece P)
        {
            if (oppositeY >= 0 && down)
            {
                if (B.Board[P.x, oppositeY] == null)
                {
                    validMoves.Add(new CSquare(P.x, oppositeY));
                    return;
                }

                //if (B.Board[P.x, oppositeY].pieceType != P.pieceType)
              //  if (P.pieceName != "K")
                    validMoves.Add(new CSquare(P.x, oppositeY));

                down = false;
            }

        }
        public void Left(CMatrixBoard B, CPiece P)
        {
            if (oppositeX >= 0 && left)
            {
                if (B.Board[oppositeX, P.y] == null)
                {
                    validMoves.Add(new CSquare(oppositeX, P.y));
                    return;
                }

                //if (B.Board[oppositeX, P.y].pieceType != P.pieceType)
                //if (P.pieceName != "K")
                    validMoves.Add(new CSquare(oppositeX, P.y));
                
                left = false;
            }
        }
        public void Right(CMatrixBoard B, CPiece P)
        {
            if (x < boardSize && right)
            {
                if (B.Board[x, P.y] == null)
                {
                    validMoves.Add(new CSquare(x, P.y));
                    return;
                }

                //if (B.Board[x, P.y].pieceType != P.pieceType)
                //if (P.pieceName != "K")
                    validMoves.Add(new CSquare(x, P.y));
                
                right = false;
            }
        }

        public void Diagonal(CPiece P, int times, string direction)
        {
            x = P.x; y = P.y;

            int counter = 2;

            rightUp = true; leftUp = true; rightDown = true; leftDown = true;

            for (int i = 0; i < times; i++)
            {
                x++; y++;
                oppositeX = x - counter; oppositeY = y - counter;

                if (direction != "")
                {
                    method = typeof(CMatrixBoard).GetMethod(direction);
                    object[] parameters = new object[] { this, P };
                    method.Invoke(this, parameters);
                }
                else
                {
                    if (x < boardSize)
                    {
                        RightUp(this, P);
                        RightDown(this, P);
                    }
                    if (oppositeX >= 0)
                    {
                        LeftUp(this, P);
                        LeftDown(this, P);
                    }
                }
                counter += 2;
            }

        }
        public void RightUp(CMatrixBoard B, CPiece P)
        {
            if (x < boardSize && y < boardSize && rightUp)
            {
                if (B.Board[x, y] == null)
                {
                    validMoves.Add(new CSquare(x, y));
                    return;
                }

                //if (B.Board[x, y] != null && B.Board[x, y].pieceType != P.pieceType)
                //if (P.pieceName != "K")
                    validMoves.Add(new CSquare(x, y));

                rightUp = false;
            }
        }
        public void RightDown(CMatrixBoard B, CPiece P)
        {
            if (x < boardSize && oppositeY >= 0 && rightDown)
            {
                if (B.Board[x, oppositeY] == null)
                {
                    validMoves.Add(new CSquare(x, oppositeY));
                    return;
                }

                //if (B.Board[x, oppositeY] != null && B.Board[x, oppositeY].pieceType != P.pieceType)
                //if (P.pieceName != "K")
                    validMoves.Add(new CSquare(x, oppositeY));

                rightDown = false;
            }
        }
        public void LeftUp(CMatrixBoard B, CPiece P)
        {
            if (oppositeX >= 0 && y < boardSize && leftUp)
            {
                if (B.Board[oppositeX, y] == null)
                {
                    validMoves.Add(new CSquare(oppositeX, y));
                    return;
                }

                //if (B.Board[oppositeX, y] != null && B.Board[oppositeX, y].pieceType != P.pieceType)
               // if (P.pieceName != "K")
                    validMoves.Add(new CSquare(oppositeX, y));

                leftUp = false;
            }
        }
        public void LeftDown(CMatrixBoard B, CPiece P)
        {
            if (oppositeX >= 0 && oppositeY >= 0 && oppositeY < boardSize && leftDown)
            {
                if (B.Board[oppositeX, oppositeY] == null)
                {
                    validMoves.Add(new CSquare(oppositeX, oppositeY));
                    return;
                }

                //if (B.Board[oppositeX, oppositeY] != null && B.Board[oppositeX, oppositeY].pieceType != P.pieceType)
             //   if (P.pieceName != "K")
                    validMoves.Add(new CSquare(oppositeX, oppositeY));
                    

                leftDown = false;
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
            if (provisoryX < boardSize)
            {
                if (provisoryY < boardSize)
                    validMoves.Add(new CSquare(provisoryX, provisoryY));

                if (oppositeY >= 0)
                    validMoves.Add(new CSquare(provisoryX, oppositeY));
            }
        }
        private void JumpLeft(int provisoryY)
        {
            if (oppositeX >= 0)
            {
                if (provisoryY < boardSize)
                    validMoves.Add(new CSquare(oppositeX, provisoryY));
                
                if (oppositeY >= 0)
                    validMoves.Add(new CSquare(oppositeX, oppositeY));

            }
        }
        public void CheckJump(CPiece P)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
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
