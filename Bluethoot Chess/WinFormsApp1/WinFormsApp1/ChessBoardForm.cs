using System;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Generic;
using System.Collections;
using Microsoft.VisualBasic.ApplicationServices;
using System.Configuration;
using System.Net.NetworkInformation;
using Microsoft.VisualBasic.Devices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics.Metrics;
using Timer = System.Windows.Forms.Timer;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;


// TODO  Pin on pieces

namespace WinFormsApp1
{
    public partial class ChessBoardForm : Form
    {
        private const int boardSize = 8;
        
        public const int squareSize = 70;

        private int turn = 0;       // 0 for white, 1 for black
        private int UpOrDown = 1;

        private int[] secondsElapsed = new int[2];

        private string currentPlayer = "";

        private static readonly string projectPath  = GetProjectPath();

        private bool firstMove = false;

        private bool isCheck = false;

        private bool[] firstKingMove = { false, false };

        private bool[] aRookFirstMove = { false, false }, hRookFirstMove = { false, false };

        private bool[] O_O = { false, false }, O_O_O = { false, false };

        public bool isRestarted { get; set; } = false;
        public bool isClosed { get; set; } = false;

        private CMatrixBoard ChessBoard;

        private CPiece? selectedPiece = null;

        private Label[] timerLabel = new Label[2];

        private Timer[] timer = new Timer[2];



        public ChessBoardForm()
        {
            ChessBoard = new CMatrixBoard();
            InitializeComponent();
            ChessBoard.InitializePieces();
            InitializeChessBoardFormButtons();
            InitializeChessBoardFormTimers();
            InitializeTimers();
        }



        private static string GetProjectPath()
        {
            string appDirectory = Application.StartupPath;
            string imagesFolder = Path.GetFullPath(Path.Combine(appDirectory, "..\\..\\..\\..\\images\\"));
            return imagesFolder;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
        }



        private void InitializeTimers()
        {
            timer[0] = new Timer { Interval = 1000 };
            timer[1] = new Timer { Interval = 1000 };

            timer[0].Tick += Timer_Tick;
            timer[1].Tick += Timer_Tick;
        }



        public static Bitmap SetImageToButton(CPiece P)
        {
            string DIR = P.pieceType;

            string imagePath = DIR + "\\" + P.pieceName + ".png";

            Bitmap originalImage = (Bitmap)Image.FromFile(projectPath + imagePath);
            return originalImage;
        }




        // TODO error when giving check with pawn after capturing diagonally.

        // Button_Click for any button, then create one method when user clicks a button with piece
        // and one other method when user click a button after clicking a piece.

        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            var position = (ValueTuple<int, int>)clickedButton.Tag;

            int destinationX = position.Item1;
            int destinationY = position.Item2;


            currentPlayer = (turn == 0) ? "white" : "black";

            int backRank = (currentPlayer == "white") ? 0 : 7;

            UpOrDown = (currentPlayer == "white") ? 1 : -1;


            if (selectedPiece == null && PieceGotClicked(destinationX, destinationY))
                ManageSelectedPiece(destinationX, destinationY, backRank);

            else
                ManageDestinationSquare(clickedButton, destinationX, destinationY, backRank);
        }


        private void ManageSelectedPiece(int destinationX, int destinationY, int backRank)
        {
            AvaibleSquares(ChessBoard.Board[destinationX, destinationY]);
            selectedPiece = ChessBoard.Board[destinationX, destinationY];


            Debug.WriteLine($"{selectedPiece.pieceName}, {selectedPiece.pieceType}");
            Debug.WriteLine(ChessBoard.ToString() + "\n");


            if (selectedPiece.pieceName == "P")
            { 
                DiagonalMovementPawn(destinationX + 1, destinationY + UpOrDown);
                return;
            }

            else if (selectedPiece.pieceName == "N")
            {
                ChessBoard.CheckKnightMoves(selectedPiece);
                return;
            }
            else if (selectedPiece.pieceName != "K")
            {
                RemoveInvalidMovesOfPiece(selectedPiece);
                return;
            }

            SaveInvalidSquaresOfKing(selectedPiece);

            // Check if  O-O  or  O-O-O  is possible.
            if (!O_O[turn])
                CheckCastle(6, backRank, 5, ref O_O[turn], hRookFirstMove[turn]);

            if (!O_O_O[turn])
                CheckCastle(2, backRank, 3, ref O_O_O[turn], aRookFirstMove[turn]);
        }


        private bool PieceGotClicked(int destinationX, int destinationY)
        {
            return (ChessBoard.Board[destinationX, destinationY] != null) ? true : false;
        }


        private void ManageDestinationSquare(Button clickedButton, int destinationX, int destinationY, int backRank)
        {
            var destinationSquare = ChessBoard.Board[destinationX, destinationY];

            if (!IsDestinationSquareValid(destinationSquare) ||
                (!IsMoveLegal(destinationX, destinationY)))

                return;

            if (isCheck && !IsMoveLegalWhenCheck(destinationX, destinationY))
                return;

            ManagePieceMovement(selectedPiece.x, selectedPiece.y, destinationX, destinationY, backRank);

            if (!firstMove)
            {
                firstMove = true;
                timer[1].Start();
            }                           
                                        // control 'if else' later
                                        // use 2 threads, whiteClockThread and blackClockThread
                                        //   that wait for each other
            else
            {
                timer[turn + UpOrDown].Stop();
                timer[turn].Start();
            }

            CPiece king = FindKing(currentPlayer);

            if (selectedPiece.pieceName != "K")
                ControlIfPieceHasGivenCheck(king, destinationX, destinationY);

            clickedButton.BackgroundImage = SetImageToButton(ChessBoard.Board[destinationX, destinationY]);

            try
            {
                if (!isCheck)
                    return;

                HandleSituationAfterCheck(king);

                if (!IsCheckmate())
                    return;

                var popUp = new RestartForm();

                popUp.StartPosition = FormStartPosition.CenterParent;

                popUp.ShowDialog(this);

                if (RestartForm.NewGame)
                    isRestarted = true;

                else if (RestartForm.MainMenu)
                    this.Close();
            }
            finally
            {

                ChessBoard.validMoves.Clear();

                turn = (turn + 1) % 2;

                selectedPiece = null;
            }
        }


        private bool IsDestinationSquareValid(CPiece destinationSquare)
        {
            return (destinationSquare == null || destinationSquare.pieceType != currentPlayer)
                ? true : false;
        }


        private bool IsMoveLegal(int destinationX, int destinationY)
        {
            return ChessBoard.validMoves.Exists(item => item.x == destinationX && item.y == destinationY);
        }


        private void ManagePieceMovement(int previousX, int previousY, int destinationX, int destinationY, int backRank)
        {
            if (selectedPiece.pieceName == "P")
                PawnPromotion(selectedPiece, destinationX, destinationY);


            Button originalSquare = GetButtonAtPosition(previousX, previousY);
            originalSquare.BackgroundImage = null;
            ChessBoard.Board[previousX, previousY] = null;
            ChessBoard.Board[destinationX, destinationY] = new CPiece(destinationX, destinationY, 
                                                                            selectedPiece.pieceName, 
                                                                                selectedPiece.pieceType);

            if (selectedPiece.pieceName == "K" && !firstKingMove[turn])
                FirstKingMove(destinationX, destinationY, backRank);

            else if (selectedPiece.pieceName == "R")
                FirstRookMove(selectedPiece, destinationX, destinationY, backRank);

            ref CPiece movedPiece = ref ChessBoard.Board[destinationX, destinationY];

            movedPiece.x = destinationX;
            movedPiece.y = destinationY;
        }


        private bool IsMoveLegalWhenCheck(int x, int y)
        {
            ChessBoard.copyMoves.Clear();

            Tuple<int, int> key = Tuple.Create(selectedPiece.x, selectedPiece.y);

            if (selectedPiece.pieceName != "K" && ChessBoard.stopCheckWithPiece.ContainsKey(key))
            {
                List<CSquare> squareList = ChessBoard.stopCheckWithPiece[key];

                if (squareList.Any(square => square.x == x && square.y == y))
                    ChessBoard.copyMoves.AddRange(ChessBoard.stopCheckWithPiece[key]);
            }
            else if (selectedPiece.pieceName == "K")
                ChessBoard.copyMoves.AddRange(ChessBoard.validMoves);

            if (!ChessBoard.copyMoves.Exists(
                    square => ChessBoard.validMoves.Exists(
                    checkSquare => checkSquare.x == square.x && checkSquare.y == square.y)))

                return false;

            foreach (var entry in ChessBoard.stopCheckWithPiece)
                entry.Value.Clear();

            ChessBoard.stopCheckWithPiece.Clear();

            isCheck = false;
            return true;
        }


        private void ControlIfPieceHasGivenCheck(CPiece king, int destinationX, int destinationY)
        {
            ChessBoard.validMoves.Clear();

            if (selectedPiece.pieceName == "R")
                DefineDirectionTowardsKing("Straight", king, destinationX, destinationY);

            else if (selectedPiece.pieceName == "B")
                DefineDirectionTowardsKing("Diagonal", king, destinationX, destinationY);

            else if (selectedPiece.pieceName == "Q")
                DefineDirectionTowardsKing("Straight", king, destinationX, destinationY);


            ChessBoard.validMoves.Add(new CSquare(destinationX, destinationY));  // piece that gives check can also be captured
                                                                                 // to stop check (neccessary for Knight and Pawn)

            Debug.Write("\nvalidMoves = ");

            foreach (var e in ChessBoard.validMoves)
                Debug.WriteLine($"[{e.x}, {e.y}] ");

            Debug.Write('\n');


            isCheck = IsCheck(king);
            Debug.WriteLine("\ncheck = " + isCheck + '\n');
        }


        private bool IsCheck(CPiece king)
        {
            if (ChessBoard.validMoves.Exists(move => move.x == king.x && move.y == king.y))
                return true;
            
            return false;
        }


        private void HandleSituationAfterCheck(CPiece king)
        {
            Debug.WriteLine("CHECK\n");

            ChessBoard.copyMoves.Clear();
            ChessBoard.copyMoves.AddRange(ChessBoard.validMoves);

            foreach (var piece in ChessBoard.Board)
            {
                if (piece == null ||
                    piece.pieceName == "K" ||
                    piece.pieceType == selectedPiece.pieceType)

                    continue;
                
                AvaibleSquares(piece);

                if (piece.pieceName == "P")
                    DiagonalMovementPawn(piece.x + 1, piece.y - UpOrDown);

                ChessBoard.validMoves.RemoveAll(move => move.x == king.x && move.y == king.y);

                StopCheck(piece);
            }

            AvaibleSquares(king);
            SaveInvalidSquaresOfKing(king);
        }


        private bool IsCheckmate()
        {
            bool noValidMoves = !ChessBoard.validMoves.Any();
            bool noBlockingPieces = !ChessBoard.stopCheckWithPiece.Any(kv => kv.Value?.Count > 0);

            return noValidMoves && noBlockingPieces;
        }


        private void DefineDirectionTowardsKing(string moveTo, CPiece king, int x, int y)
        {
            ChessBoard.validMoves.Clear();

            string direction = (moveTo == "Straight") ? FindStraightDirection(king, x, y) 
                                                        : FindDiagonalDirection(king, x, y);

            Debug.Write($"\ndirection = {direction}\n");

            switch (moveTo)
            {
                case "Straight":
                    ChessBoard.Straight(ChessBoard.Board[x, y], 8, direction);
                    break;

                case "Diagonal":
                    ChessBoard.Diagonal(ChessBoard.Board[x, y], 8, direction);
                    break;
            }

            if (selectedPiece.pieceName == "Q" && !isCheck && moveTo != "Diagonal")
                DefineDirectionTowardsKing("Diagonal", king, x, y);
        }


        private void CheckCastle(int kingMoveX, int Y, int compareX, ref bool castle, bool firstRookMove)
        {
            if (compareX == 3 && ChessBoard.Board[2, Y] != null)
                return;

            if (!firstKingMove[turn] && !firstRookMove &&
                ChessBoard.Board[kingMoveX, Y] == null && ChessBoard.validMoves.Exists(item => item.x == compareX && item.y == Y))
            {
                ChessBoard.validMoves.Add(new CSquare(kingMoveX, Y));
                castle = true;
                return;
            }

            castle = false;
        }


        private string FindStraightDirection(CPiece king, int targetX, int targetY)
        {
            if (king == null || (targetX != king.x && targetY != king.y))  // no sure that works
                return "";

            return
                (targetY == king.y) ? ((targetX > king.x) ? "Left" : "Right")
                                    : ((targetY > king.y) ? "Down" : "Up");
        }


        private string FindDiagonalDirection(CPiece king, int targetX, int targetY)
        {
            if (king == null || targetX == king.x || targetY == king.y)
                return "";

            return
                (targetY > king.y) ? ((targetX > king.x) ? "LeftDown" : "RightDown")
                                   : ((targetX < king.x) ? "RightUp" : "LeftUp");
        }


        private void RemoveInvalidMovesOfPiece(CPiece P)
        {
            List<CSquare> tmp_list = new();
            tmp_list.AddRange(ChessBoard.validMoves);

            foreach (var move in tmp_list)
            {
                if (ChessBoard.Board[move.x, move.y] == null)
                    continue;

                CPiece piece = ChessBoard.Board[move.x, move.y];

                if (piece.pieceType == P.pieceType)
                    ChessBoard.validMoves.RemoveAll(square => square.x == move.x && square.y == move.y);
            }
        }



        // Calculates the squares of every piece and compares with validMoves, if there are == moves, saves them in InvalidSquares
        private void SaveInvalidSquaresOfKing(CPiece king)
        {
            ChessBoard.invalidSquaresKing.Clear();

            ChessBoard.invalidSquaresKing.AddRange(ChessBoard.validMoves);

            foreach (var piece in ChessBoard.Board)
            {
                if (piece != null && piece.pieceType != king.pieceType && piece.pieceName != king.pieceName)
                {
                    AvaibleSquares(piece);

                    if (piece.pieceName == "P")
                        ChessBoard.validMoves.RemoveAll(square => square.x == piece.x && square.y == piece.y + (-UpOrDown));

                    ChessBoard.invalidSquaresKing.RemoveAll(square => ChessBoard.validMoves.Exists(move => move.x == square.x && move.y == square.y));
                }
            }

            CheckPieceNearKing(king);

            ChessBoard.validMoves.Clear();
            ChessBoard.validMoves.AddRange(ChessBoard.invalidSquaresKing);
        }



        private void CheckPieceNearKing(CPiece king)
        {
            ref CMatrixBoard B = ref ChessBoard;

            for (int x = king.x - 1; x < king.x + 2; x++)
                for (int y = king.y - 1; y < king.y + 2; y++)
                {
                    if (x >= 0 && x < boardSize && 
                        y >= 0 && y < boardSize && 
                        B.Board[x, y] != null)

                        FindInvalidCapturesKing(king, B.Board[x, y]);
                }
        }


        

        private void FindInvalidCapturesKing(CPiece king, CPiece pieceNearKing)
        {
            ref CMatrixBoard B = ref ChessBoard;

            if (king.pieceType == pieceNearKing.pieceType)
            {
                B.invalidSquaresKing.RemoveAll(square => square.x == pieceNearKing.x && square.y == pieceNearKing.y);
                return;
            }

            foreach (var piece in B.Board)
            {
                if (piece == null || piece == pieceNearKing || 
                    piece.pieceType != pieceNearKing.pieceType)
                    
                    continue;
                
                AvaibleSquares(piece);

                if (B.validMoves.Exists(square => square.x == pieceNearKing.x && square.y == pieceNearKing.y))
                {
                    B.invalidSquaresKing.RemoveAll(square => square.x == pieceNearKing.x && square.y == pieceNearKing.y);
                    break;
                }
            }
        }



        private void StopCheck(CPiece Piece)
        {
            List<CSquare> tmpMoves = new();

            Tuple<int, int> key;

            foreach (var square in ChessBoard.validMoves)
            {
                if (ChessBoard.copyMoves.Exists(move => move.x == square.x && move.y == square.y))
                    tmpMoves.Add(square);
            }

            if (tmpMoves.Any())
            {
                key = Tuple.Create(Piece.x, Piece.y);
                ChessBoard.stopCheckWithPiece[key] = tmpMoves;
            }
        }





        private void ShortAndLongCastle(int rookX, int Y)
        {
            var tmpRook = ChessBoard.Board[rookX, Y];  // copies the rook

            FirstRookMove(tmpRook, rookX, Y, Y);

            ChessBoard.Board[rookX, Y] = null;

            Button rookSquare = GetButtonAtPosition(rookX, Y);
            rookSquare.BackgroundImage = null;

            //transpose the rook
            rookX = (rookX == 0) ? 3 : 5;

            tmpRook.x = rookX;
            ChessBoard.Board[rookX, Y] = tmpRook;

            Bitmap rookImage = SetImageToButton(tmpRook);

            rookSquare = GetButtonAtPosition(rookX, Y);
            rookSquare.BackgroundImage = rookImage;
        }


        private void FirstKingMove(int previusKingX, int previusKingY, int Y)
        {
            firstKingMove[turn] = true;

            if (O_O[turn] && previusKingX == 6 && previusKingY == Y)
                ShortAndLongCastle(7, Y);

            else if (O_O_O[turn] && previusKingX == 2 && previusKingY == Y)
                ShortAndLongCastle(0, Y);
        }


        private void FirstRookMove(CPiece selectedPiece, int x, int y, int Y)
        {
            if (selectedPiece.x == 0 && selectedPiece.y == Y)
                aRookFirstMove[turn] = true;

            if (selectedPiece.x == 7 && selectedPiece.y == Y)
                hRookFirstMove[turn] = true;
        }


        private void DiagonalMovementPawn(int x, int y)
        {
            int oppositeX = x - 2;

            if (x < 8 && (ChessBoard.Board[x, y] == null ||
                ChessBoard.Board[x, y].pieceName == "K"))
                ChessBoard.validMoves.RemoveAll(square => square.x == x && square.y == y);

            if (oppositeX >= 0 && (ChessBoard.Board[oppositeX, y] == null || ChessBoard.Board[oppositeX, y].pieceName == "K"))
                ChessBoard.validMoves.RemoveAll(square => square.x == oppositeX && square.y == y);

        }


        private void PawnPromotion(CPiece selectedPiece, int x, int y)
        {
            if (selectedPiece.y + 1 == 7 && selectedPiece.pieceType == "white" ||
                selectedPiece.y - 1 == 0 && selectedPiece.pieceType == "black")
            {

                var promotion = new PromotionForm(turn);
                promotion.ShowDialog();

                this.selectedPiece = new CPiece(x, y, promotion.PieceName, selectedPiece.pieceType);

                Debug.WriteLine("Promotion: " + promotion.PieceName);
            }
        }


        // create a method in ChessBoard.cs  'CalculateMoves(CPiece P, string direction)'

        private void AvaibleSquares(CPiece P)
        {
            ChessBoard.validMoves.Clear();

            switch (P.pieceName)
            {
           }
        }


        private CPiece FindKing(string currentPlayer)
        {
            foreach (var piece in ChessBoard.Board)
            {
                if (piece != null && piece.pieceType != currentPlayer && piece.pieceName == "K")
                    return piece;
            }
            return null;
        }


        // Find the button at the specified position
        private Button GetButtonAtPosition(int x, int y)
        {
            foreach (var button in Controls.OfType<Button>())
            {
                var position = (ValueTuple<int, int>)button.Tag;

                if (position.Item1 == x && position.Item2 == y)
                {
                    return button;
                }
            }
            return null;
        }
    }
}