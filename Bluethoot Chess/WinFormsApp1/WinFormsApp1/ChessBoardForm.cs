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
using System.Transactions;


// TODO  Pin on pieces

namespace WinFormsApp1
{
    public enum PieceColor
    {
        White,
        Black
    };


    public enum Ranks
    {
        FirstRank,
        SecondRank,
        ThirdRank,
        FourthRank,
        FifthRank,
        SixthRank,
        SeventhRank,
        EighthRank
    };

    
    public enum Files
    {
        FirstFile,
        SecondFile,
        ThirdFile,
        FourthFile,
        FifthFile,
        SixthFile,
        SeventhFile,
        EighthFile
    };



    public partial class ChessBoardForm : Form
    {
        private const int BOARD_SIZE = 8;

        public const int SQUARE_SIZE = 70;


        private int turn = 0;       // 0 for white, 1 for black
        public static int MoveTowardsBlackOrWhite { get; private set; } = 1;

        private int[] secondsElapsed = new int[2];

        private static readonly string projectPath = GetProjectPath();  // pathToImages

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

        private static readonly PieceColor[] currentPlayer = { PieceColor.White, PieceColor.Black };


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
            string DIR = P.pieceType.ToString().ToLower();

            string imagePath = DIR + "\\" + P.pieceName + ".png";

            Bitmap originalImage = (Bitmap)Image.FromFile(projectPath + imagePath);
            return originalImage;
        }




        // TODO

        // 1) error when giving check with pawn after capturing diagonally.

        // 2) when clicking a piece and then clicking a square with the same piece
        //    it should unselect the first clicked button and select the second button

        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            var position = (ValueTuple<int, int>)clickedButton.Tag;

            int destinationX = position.Item1;
            int destinationY = position.Item2;

            var clickedSquare = ChessBoard.Board[destinationX, destinationY];

            if (!PieceGotClicked(clickedSquare) && 
                selectedPiece == null)

                return;

            int backRank = (int)currentPlayer[turn] * (int)Ranks.EighthRank;

            MoveTowardsBlackOrWhite = -1 * (turn * 2 - 1);

            Debug.Write($"\nMoveTowardsBlackOrWhite = {MoveTowardsBlackOrWhite}\n");

            if (selectedPiece == null)
                ManageSelectedPiece(destinationX, destinationY, backRank);

            else
                ManageDestinationSquare(clickedButton, destinationX, destinationY, backRank);
        }


        private void ManageSelectedPiece(int destinationX, int destinationY, int backRank)
        {
            selectedPiece = ChessBoard.Board[destinationX, destinationY];
            ChessBoard.CalculateMoves(selectedPiece, "");


            Debug.WriteLine($"{selectedPiece.pieceName}, {selectedPiece.pieceType}");
            Debug.WriteLine(ChessBoard.ToString() + "\n");


            if (selectedPiece.pieceName == "P")
            { 
                ManageInvalidDiagonalPawnMoves(selectedPiece);
                return;
            }
            else if (selectedPiece.pieceName != "K")
            {
                RemoveInvalidPieceMoves(selectedPiece);
                return;
            }

            RemoveInvalidSquaresOfKing(selectedPiece);


            // the d1 square and f1 square are controlled in RemoveInfalidSquareOfKing();
            //  so i just need to check the b1, c1, and g1 square

            if (!O_O_O[turn])
                O_O_O[turn] = CheckCastle((int)Files.SecondFile, (int)Files.FourthFile, 
                                          backRank, aRookFirstMove[turn]);
            
            if (!O_O[turn])
                O_O[turn] = CheckCastle((int)Files.SixthFile, (int)Files.EighthFile, 
                                        backRank, hRookFirstMove[turn]);
        }


        private bool PieceGotClicked(CPiece clickedSquare)
        {
            return (clickedSquare != null &&
                    clickedSquare.pieceType == currentPlayer[turn]);
        }


        private void ManageDestinationSquare(Button clickedButton, int destinationX, int destinationY, int backRank)
        {
            if (!IsMoveLegal(destinationX, destinationY))
            {
                selectedPiece = null;
                return;
            }

            if (isCheck && !IsMoveLegalWhenCheck(destinationX, destinationY))
            {
                selectedPiece = null;
                return;
            }

            ManagePieceMovement(selectedPiece.x, selectedPiece.y, destinationX, destinationY, backRank);

            if (!firstMove)
            {
                firstMove = true;
                timer[1].Start();
            }                           
                                        // control 'if else' later
                                        // use 2 threads,
                                        // whiteClockThread and blackClockThread
                                        // that wait for each other
            else
            {
                timer[turn + MoveTowardsBlackOrWhite].Stop();
                timer[turn].Start();
            }

            CPiece king = FindKing();

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


        private bool IsMoveLegal(int destinationX, int destinationY)
        {
            return (ChessBoard.validMoves.Exists(
                item => item.x == destinationX && item.y == destinationY) && 

                (ChessBoard.IsSquareNull(destinationX, destinationY) ||
                    ChessBoard.Board[destinationX, destinationY].pieceType != currentPlayer[turn]));
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
                    square => SquareIsInTheList(ChessBoard.validMoves, square.x, square.y)))

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

            if (selectedPiece.pieceName == "R" || selectedPiece.pieceName == "Q")
                DefineDirectionTowardsKing("Straight", king, destinationX, destinationY);

            else if (selectedPiece.pieceName == "B")
                DefineDirectionTowardsKing("Diagonal", king, destinationX, destinationY);

            else
                ChessBoard.CalculateMoves(selectedPiece, "");


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
            if (SquareIsInTheList(ChessBoard.validMoves, king.x, king.y)) // breakpoint
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
                
                ChessBoard.CalculateMoves(piece, "");

                if (piece.pieceName == "P")
                    ManageInvalidDiagonalPawnMoves(piece);

                RemoveSquaresFromList(ChessBoard.validMoves, king.x, king.y);

                StopCheck(piece);
            }

            ChessBoard.CalculateMoves(king, "");
            RemoveInvalidSquaresOfKing(king);
        }


        private bool IsCheckmate()
        {
            bool noValidMoves = !ChessBoard.validMoves.Any();
            bool noBlockingPieces = !ChessBoard.stopCheckWithPiece.Any(kv => kv.Value?.Count > 0);

            return noValidMoves && noBlockingPieces;
        }


        // TODO improve this method
        private void DefineDirectionTowardsKing(string moveTo, CPiece king, int x, int y)
        {
            ChessBoard.validMoves.Clear();

            string direction = (moveTo == "Straight") ? FindStraightDirection(king, x, y) 
                                                        : FindDiagonalDirection(king, x, y);

            Debug.Write($"\ndirection = {direction}\n");

            switch (moveTo)
            {
                case "Straight":
                    ChessBoard.CalculateMoves(ChessBoard.Board[x, y], direction);
                    break;

                case "Diagonal":
                    ChessBoard.CalculateMoves(ChessBoard.Board[x, y], direction);
                    break;
            }

            if (selectedPiece.pieceName == "Q" && !isCheck && moveTo != "Diagonal")
                DefineDirectionTowardsKing("Diagonal", king, x, y);
        }


        private bool CheckCastle(int startingFile, int endingFile, int backRank, bool firstRookMove)
        {
            if (firstKingMove[turn] || firstRookMove)
                return false;

            int destinationKingX = startingFile;

            for (; startingFile < endingFile; startingFile++)
                if (!ChessBoard.IsSquareNull(startingFile, backRank))
                    return false;

            // TODO check if the squares in between the
            //      O_O or O_O_O happens are accessible

            /*

             ... 

            */


            ChessBoard.validMoves.Add(new CSquare(startingFile + 1, backRank));

            return false;
        }


        private string FindStraightDirection(CPiece king, int targetX, int targetY)
        {
            if (king == null || (targetX != king.x && targetY != king.y))  // not sure that works
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


        private void RemoveInvalidPieceMoves(CPiece P)
        {
            List<CSquare> tmp_list = new();
            tmp_list.AddRange(ChessBoard.validMoves);

            foreach (var move in tmp_list)
            {
                if (ChessBoard.IsSquareNull(move.x, move.y))
                    continue;

                CPiece piece = ChessBoard.Board[move.x, move.y];

                if (piece.pieceType == P.pieceType)
                    SquareIsInTheList(ChessBoard.validMoves, move.x, move.y);
            }
        }




        // TODO remove also c1 or g1 square in case of O_O or O_O_O
        private void RemoveInvalidSquaresOfKing(CPiece king)
        {
            List<CSquare> tmpKingMoves = new();
            tmpKingMoves.AddRange(ChessBoard.validMoves);

            foreach (var piece in ChessBoard.Board)
            {
                if (piece == null || 
                    piece.pieceType == king.pieceType ||
                    piece.pieceName == king.pieceName)

                    continue;


                ChessBoard.CalculateMoves(piece, "");

                if (piece.pieceName == "P")
                    ChessBoard.validMoves.RemoveAll(square => square.x == piece.x && 
                                                    square.y == piece.y + (-MoveTowardsBlackOrWhite));

                tmpKingMoves.RemoveAll(square => 
                                        SquareIsInTheList(ChessBoard.validMoves, square.x, square.y)); // strange but ok
            }
            
            ChessBoard.validMoves.Clear();
            ChessBoard.validMoves.AddRange(tmpKingMoves);

            CheckPiecesNearKing(king);
        }


        /*
            Checks each square next to the king, 8 squares in total,
            and removes squares where king is not able to move.
        */

        private void CheckPiecesNearKing(CPiece king)
        {
            for (int x = king.x - 1; x < king.x + 2; x++)

                for (int y = king.y - 1; y < king.y + 2; y++)

                    if (x >= 0 && x < BOARD_SIZE && 
                        y >= 0 && y < BOARD_SIZE && 
                        !ChessBoard.IsSquareNull(x, y))

                        FindInvalidCapturesKing(king, ChessBoard.Board[x, y]);
        }


        

        private void FindInvalidCapturesKing(CPiece king, CPiece pieceNearKing)
        {
            if (king.pieceType == pieceNearKing.pieceType) // if a piece is placed near the king
            {
                RemoveSquaresFromList(ChessBoard.validMoves, pieceNearKing.x, pieceNearKing.y);
                return;
            }


            List<CSquare> tmpKingMoves = new();
            tmpKingMoves.AddRange(ChessBoard.validMoves);


            foreach (var piece in ChessBoard.Board)
            {
                if (piece == null || piece == pieceNearKing || 
                    piece.pieceType != pieceNearKing.pieceType)
                    
                    continue;
                

                ChessBoard.CalculateMoves(piece, "");

                if (SquareIsInTheList(ChessBoard.validMoves, pieceNearKing.x, pieceNearKing.y))
                {
                    RemoveSquaresFromList(tmpKingMoves, pieceNearKing.x, pieceNearKing.y);
                    break;
                }
            }

            ChessBoard.validMoves.AddRange(tmpKingMoves);
        }



        private void StopCheck(CPiece Piece)
        {
            List<CSquare> tmpMoves = new();

            Tuple<int, int> key;


            foreach (var square in ChessBoard.validMoves)
                if (SquareIsInTheList(ChessBoard.copyMoves, square.x, square.y))
                    tmpMoves.Add(square);


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

        
        private void ManageInvalidDiagonalPawnMoves(CPiece piece)
        {
            RemoveInvalidDiagonalPawnMoves(piece.x + 1, piece.y + MoveTowardsBlackOrWhite);
            RemoveInvalidDiagonalPawnMoves(piece.x - 1, piece.y + MoveTowardsBlackOrWhite);
        }


        private void RemoveInvalidDiagonalPawnMoves(int destinationX, int destinationY)
        {
            if (destinationX >= 0 && destinationX < BOARD_SIZE &&
                (ChessBoard.IsSquareNull(destinationX, destinationY) ||
                 ChessBoard.Board[destinationX, destinationY].pieceName == "K"))

                RemoveSquaresFromList(ChessBoard.validMoves, destinationX, destinationY);
        }


        private void PawnPromotion(CPiece selectedPiece, int x, int y)
        {
            if (selectedPiece.y + 1 == 7 && selectedPiece.pieceType == PieceColor.White ||
                selectedPiece.y - 1 == 0 && selectedPiece.pieceType == PieceColor.Black)
            {

                var promotion = new PromotionForm(turn);
                promotion.ShowDialog();

                this.selectedPiece = new CPiece(x, y, promotion.PieceName, selectedPiece.pieceType);

                Debug.WriteLine("Promotion: " + promotion.PieceName);
            }
        }


        private CPiece FindKing()
        {
            foreach (var piece in ChessBoard.Board)
                if (piece != null && 
                    piece.pieceType != currentPlayer[turn] && 
                    piece.pieceName == "K")

                    return piece;

            return null;
        }



        private void RemoveSquaresFromList(List<CSquare> list, int destinationX, int destinationY)
        {
            list.RemoveAll(square => square.x == destinationX && square.y == destinationY);
        }

                                                                                                              // these two in the ChessBoard.cs

        private bool SquareIsInTheList(List<CSquare> list, int destinationX, int destinationY)
        {
            return list.Exists(square => square.x == destinationX && square.y == destinationY);
        }



        // Find the button at the specified position
        private Button GetButtonAtPosition(int x, int y)
        {
            foreach (var button in Controls.OfType<Button>())
            {
                var position = (ValueTuple<int, int>)button.Tag;

                if (position.Item1 == x && position.Item2 == y)
                    return button;
            }

            return null;
        }
    }
}