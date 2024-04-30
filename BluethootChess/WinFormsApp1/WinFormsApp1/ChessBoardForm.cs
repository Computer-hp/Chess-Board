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
// O_O and O_O_O somehow don't work now

namespace WinFormsApp1
{
    public partial class ChessBoardForm : Form
    {
        private const int BOARD_SIZE = 8;
        private const int N_PLAYERS = 2;

        public const int SQUARE_SIZE = 70;


        private int turn = 0;       // 0 for white, 1 for black

        private int[] secondsElapsed = new int[2];

        private static readonly string projectPath = GetProjectPath();  // pathToImages

        private bool firstMove = false;

        private bool isCheck = false;

        private bool[] firstKingMove = { false, false };

        private bool[] aRookFirstMove = { false, false }, hRookFirstMove = { false, false };

        private bool[] O_O = { false, false }, O_O_O = { false, false };

        private ChessBoard chessBoard;

        private Piece? selectedPiece = null;

        private Label[] timerLabel = new Label[2];

        private Timer[] timer = new Timer[2];

        private static readonly PieceColor[] currentPlayer = { PieceColor.White, PieceColor.Black };

        public bool isRestarted { get; private set; } = false;
        public bool isClosed { get; private set; } = false;


        public ChessBoardForm()
        {
            chessBoard = new ChessBoard();
            InitializeComponent();
            chessBoard.InitializePieces();
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
            for (int i = 0; i < N_PLAYERS; i++)
            {
                timer[i] = new Timer { Interval = 1000 };
                timer[i].Tick += Timer_Tick;
            }
        }



        public static Bitmap SetImageToButton(Piece piece)
        {
            string DIR = piece.pieceType.ToString().ToLower();
            string imagePath = DIR + "\\" + piece.pieceName + ".png";

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

            int destinationX = position.Item1, destinationY = position.Item2;
            Debug.Write($"\nclicked_button = {destinationX}, {destinationY}\n");
            var clickedSquare = chessBoard[destinationY, destinationX];

            if (!PieceGotClicked(clickedSquare) && selectedPiece == null) return;

            int firstRank = (int)Ranks.FirstRank;
            int backRank = Math.Abs((int)currentPlayer[turn] * firstRank - firstRank);

            if (selectedPiece == null) ManageSelectedPiece(destinationX, destinationY, backRank);

            else
            { 
                ManageDestinationSquare(clickedButton, destinationX, destinationY, backRank);
                chessBoard.MovePawnTowardsBlackOrWhite = 1 * (turn * 2 - 1);
            }
        }


        private void ManageSelectedPiece(int destinationX, int destinationY, int backRank)
        {
            selectedPiece = chessBoard[destinationY, destinationX];
            chessBoard.CalculateMoves(selectedPiece, "");

            Debug.WriteLine($"{selectedPiece.pieceName}, {selectedPiece.pieceType}");
            Debug.WriteLine(chessBoard.ToString() + "\n");

            if (selectedPiece.pieceName == "P") 
            { 
                ManageInvalidDiagonalPawnMoves(selectedPiece); 
                return; 
            }

            if (selectedPiece.pieceName != "K") return;

            RemoveInvalidSquaresOfKing(selectedPiece);

            if (!O_O_O[turn])  O_O_O[turn] = IsCastleLegal((int)Files.ThirdFile, (int)Files.FifthFile, backRank, aRookFirstMove[turn]);

            if (!O_O[turn])  O_O[turn] = IsCastleLegal((int)Files.SixthFile, (int)Files.EighthFile, backRank, hRookFirstMove[turn]);

            Debug.WriteLine($"{selectedPiece.pieceName}, {selectedPiece.pieceType}");
            Debug.WriteLine(chessBoard.ToString() + "\n");
        }


        private bool PieceGotClicked(Piece clickedSquare)
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

            // control 'if else' fix later
            // use 2 threads,
            // whiteClockThread and blackClockThread,
            // that have to wait for each other

            else 
            { 
                timer[turn - chessBoard.MovePawnTowardsBlackOrWhite].Stop(); 
                timer[turn].Start(); 
            }

            Piece king = FindKing();

            if (selectedPiece.pieceName != "K") ControlIfPieceHasGivenCheck(king, destinationX, destinationY);

            clickedButton.BackgroundImage = SetImageToButton(chessBoard[destinationY, destinationX]);

            try
            {
                if (!isCheck)
                    return;

                HandleSituationAfterCheck(king);

                if (!IsCheckmate())
                    return;

                var popUp = new RestartForm { StartPosition = FormStartPosition.CenterParent };

                popUp.ShowDialog(this);

                if (RestartForm.NewGame) isRestarted = true;

                else if (RestartForm.MainMenu) this.Close();
            }
            finally
            {
                chessBoard.ValidMoves.Clear();
                turn = (turn + 1) % 2;
                selectedPiece = null;
            }
        }


        private bool IsMoveLegal(int destinationX, int destinationY)
        {
            return (chessBoard.ValidMoves.Exists(item => item.x == destinationX && item.y == destinationY) && 
                                                 !ChessBoard.IsSquareOutsideTheBoard(destinationX, destinationY) &&
                                                 (chessBoard.IsSquareNull(destinationX, destinationY) ||
                                                 chessBoard[destinationY, destinationX].pieceType != currentPlayer[turn]));
        }


        private void ManagePieceMovement(int previousX, int previousY, int destinationX, int destinationY, int backRank)
        {
            if (selectedPiece.pieceName == "P") ManagePawnPromotion(selectedPiece, destinationX, destinationY);

            Button originalSquare = GetButtonAtPosition(previousX, previousY);
            originalSquare.BackgroundImage = null;
            chessBoard[previousY, previousX] = null;
            chessBoard[destinationY, destinationX] = new Piece(destinationX, destinationY, 
                                                               selectedPiece.pieceName, 
                                                               selectedPiece.pieceType);

            if (selectedPiece.pieceName == "K" && !firstKingMove[turn])
                ManageFirstKingMove(destinationX, destinationY, backRank);

            else if (selectedPiece.pieceName == "R")
                ManageFirstRookMove(selectedPiece, backRank);

            ref Piece movedPiece = ref chessBoard.GetPieceRef(destinationY, destinationX);

            movedPiece.x = destinationX; // null reference i don't know why
            movedPiece.y = destinationY;
        }


        private bool IsMoveLegalWhenCheck(int destinationX, int destinationY)
        {
            chessBoard.CopyMoves.Clear();

            Tuple<int, int> destinationSquare = Tuple.Create(selectedPiece.x, selectedPiece.y);

            if (selectedPiece.pieceName == "K") chessBoard.CopyMoves.AddRange(chessBoard.ValidMoves);

            else if (chessBoard.StopCheckWithPiece.ContainsKey(destinationSquare))
            {
                List<Square> squareList = chessBoard.StopCheckWithPiece[destinationSquare];

                if (squareList.Any(square => square.x == destinationX && square.y == destinationY))
                    chessBoard.CopyMoves.AddRange(chessBoard.StopCheckWithPiece[destinationSquare]);
            }
            

            if (!chessBoard.CopyMoves.Exists(square => IsSquareInList(chessBoard.ValidMoves, square.x, square.y))) return false;

            ClearDictionary(chessBoard.StopCheckWithPiece);
            
            isCheck = false;
            return true;
        }


        private void ControlIfPieceHasGivenCheck(Piece king, int destinationX, int destinationY)
        {
            chessBoard.ValidMoves.Clear();
            var lastPieceMoved = chessBoard[destinationY, destinationX];

            if (selectedPiece.pieceName == "R" || selectedPiece.pieceName == "Q")
                DefineDirectionTowardsKing("Straight", king, destinationX, destinationY);

            else if (selectedPiece.pieceName == "B")
                DefineDirectionTowardsKing("Diagonal", king, destinationX, destinationY);

            else chessBoard.CalculateMoves(lastPieceMoved, ""); // for the pawn or knight


            chessBoard.ValidMoves.Add(new Square(destinationX, destinationY));  // piece that gives check can also be captured
                                                                                 // to stop check (neccessary for Knight and Pawn)

            Debug.Write("\nvalidMoves = ");

            foreach (var e in chessBoard.ValidMoves)
                Debug.WriteLine($"[{e.x}, {e.y}] ");

            Debug.Write('\n');

            isCheck = IsCheck(king);
            Debug.WriteLine("\ncheck = " + isCheck + '\n');
        }


        private bool IsCheck(Piece king)
        {
            if (IsSquareInList(chessBoard.ValidMoves, king.x, king.y)) return true;
            
            return false;
        }


        private void HandleSituationAfterCheck(Piece king)
        {
            Debug.WriteLine("CHECK\n");

            chessBoard.CopyMoves.Clear();
            chessBoard.CopyMoves.AddRange(chessBoard.ValidMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null ||
                    piece.pieceName == "K" ||
                    piece.pieceType == selectedPiece.pieceType)
                    continue;
                
                chessBoard.CalculateMoves(piece, "");

                if (piece.pieceName == "P")
                {
                    // should also remove the 2 squares in front of the pawn. --> create a method to do that because it's done several times
                    // still works because when moving the king something controls the 2 squares!
                    ManageInvalidDiagonalPawnMoves(piece);
                }

                RemoveSquaresFromList(chessBoard.ValidMoves, king.x, king.y);
                StopCheck(piece);
            }

            chessBoard.CalculateMoves(king, "");
            RemoveInvalidSquaresOfKing(king);
        }


        private bool IsCheckmate()
        {
            bool noValidMoves = !chessBoard.ValidMoves.Any();
            bool noBlockingPieces = !chessBoard.StopCheckWithPiece.Any(); // maybe incorrect

            return noValidMoves && noBlockingPieces;
        }


        // TODO improve this method
        private void DefineDirectionTowardsKing(string moveTo, Piece king, int x, int y)
        {
            chessBoard.ValidMoves.Clear();

            string direction = (moveTo == "Straight") ? FindStraightDirection(king, x, y) 
                                                      : FindDiagonalDirection(king, x, y);

            Debug.Write($"\ndirection = {direction}\n");

            switch (moveTo)
            {
                case "Straight":
                    chessBoard.CalculateMoves(chessBoard[y, x], direction);
                    break;

                case "Diagonal":
                    chessBoard.CalculateMoves(chessBoard[y, x], direction);
                    break;
            }

            if (selectedPiece.pieceName == "Q" && !isCheck && moveTo != "Diagonal")
                DefineDirectionTowardsKing("Diagonal", king, x, y);
        }


        private bool IsCastleLegal(int startingFile, int endingFile, int backRank, bool firstRookMove)
        {
            if (firstKingMove[turn] || firstRookMove) return false;

            // controls if [0, 1] is null for O_O_O
            if (startingFile == (int)Files.ThirdFile &&
                !chessBoard.IsSquareNull(startingFile - 1, endingFile))
                return false;

            var king = selectedPiece;

            List<Square> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            for (; startingFile < endingFile; startingFile++)
            {
                if (!chessBoard.IsSquareNull(startingFile, backRank)) return false; // squares are never outside the board

                                // control if squares from starting to ending File are into opponents piece moves
                foreach (var piece in chessBoard)
                {
                    if (piece == null ||
                        piece.pieceName == "K" ||
                        piece.pieceType == king.pieceType)
                        continue;

                    chessBoard.CalculateMoves(piece, "");

                    if (piece.pieceName == "P")
                        chessBoard.ValidMoves.RemoveAll(square => square.x == piece.x && 
                                                        (square.y == piece.y + chessBoard.MovePawnTowardsBlackOrWhite * (-1) ||
                                                        square.y == piece.y + chessBoard.MovePawnTowardsBlackOrWhite * (-2)));

                    if (IsSquareInList(chessBoard.ValidMoves, startingFile, backRank)) return false;
                }
            }

            tmpKingMoves.Add(new Square(endingFile - 1, backRank)); // add the square to enable O_O or O_O_O
            chessBoard.ValidMoves.Clear();
            chessBoard.ValidMoves.AddRange(tmpKingMoves);
            return true;
        }


        private string FindStraightDirection(Piece king, int targetX, int targetY)
        {
            if (king == null || (targetX != king.x && targetY != king.y))  // not sure that works
                return "";

            return (targetY == king.y) ? ((targetX > king.x) ? "Left" : "Right")
                                       : ((targetY > king.y) ? "Down" : "Up");
        }


        private string FindDiagonalDirection(Piece king, int targetX, int targetY)
        {
            if (king == null || targetX == king.x || targetY == king.y) return "";

            return (targetY > king.y) ? ((targetX > king.x) ? "LeftDown" : "RightDown")
                                      : ((targetX < king.x) ? "RightUp" : "LeftUp");
        }



        // TODO remove also c1 or g1 square in case of O_O or O_O_O
        private void RemoveInvalidSquaresOfKing(Piece king)
        {
            List<Square> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null || 
                    piece.pieceType == king.pieceType ||
                    piece.pieceName == king.pieceName)
                    continue;


                chessBoard.CalculateMoves(piece, "");

                // to remove straight moves of the PAWN, by one square and/or two squares in case the pawn hasn't been moved yet
                if (piece.pieceName == "P")
                {
                    RemoveSquaresFromList(chessBoard.ValidMoves, piece.x, piece.y + chessBoard.MovePawnTowardsBlackOrWhite * (-1));
                    RemoveSquaresFromList(chessBoard.ValidMoves, piece.x, piece.y + chessBoard.MovePawnTowardsBlackOrWhite * (-2));
                }

                tmpKingMoves.RemoveAll(square => IsSquareInList(chessBoard.ValidMoves, square.x, square.y)); // strange but ok
            }
            
            chessBoard.ValidMoves.Clear();  // if this is removed than ValidMoves will contain the moves of the piece that protects the one that gave check
            chessBoard.ValidMoves.AddRange(tmpKingMoves);

            CheckPiecesNearKing(king);
        }


        /*
            Checks each square next to the king, 8 squares in total,
            and removes squares where king is not able to move.
        */

        private void CheckPiecesNearKing(Piece king)
        {
            for (int x = king.x - 1; x < king.x + 2; x++)
                for (int y = king.y - 1; y < king.y + 2; y++)

                    if (!ChessBoard.IsSquareOutsideTheBoard(x, y) &&
                        !chessBoard.IsSquareNull(x, y))
                        FindInvalidCapturesKing(king, chessBoard[y, x]);
        }


        

        private void FindInvalidCapturesKing(Piece king, Piece pieceNearKing)
        {
            if (king.Equals(pieceNearKing)) return;

            if (king.pieceType == pieceNearKing.pieceType) // if a piece with the same color is placed near the king
            {
                RemoveSquaresFromList(chessBoard.ValidMoves, pieceNearKing.x, pieceNearKing.y);
                return;
            }


            List<Square> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            // controls whether the piece near king is protected by another piece,
            // so the king can't capture it
            foreach (var piece in chessBoard)
            {
                if (piece == null || 
                    (piece.x == pieceNearKing.x && piece.y == pieceNearKing.y) ||
                    piece.pieceType != pieceNearKing.pieceType)
                    continue;
                

                chessBoard.CalculateMoves(piece, "");

                if (IsSquareInList(chessBoard.ValidMoves, pieceNearKing.x, pieceNearKing.y))
                {
                    RemoveSquaresFromList(tmpKingMoves, pieceNearKing.x, pieceNearKing.y);
                    break;
                }
                
                if (piece.x == 2 && piece.y == 4)
                    Console.Write("\nTarget\n");
            }

            chessBoard.ValidMoves.Clear();
            chessBoard.ValidMoves.AddRange(tmpKingMoves);
        }



        private void StopCheck(Piece piece)
        {
            List<Square> tmpMoves = new();

            Tuple<int, int> key;

            foreach (var square in chessBoard.ValidMoves)
                if (IsSquareInList(chessBoard.CopyMoves, square.x, square.y))
                    tmpMoves.Add(square);

            if (tmpMoves.Any())
            {
                key = Tuple.Create(piece.x, piece.y);
                chessBoard.StopCheckWithPiece[key] = tmpMoves;
            }
        }



        private void ManageShortOrLongCastle(int previousRookX, int backRank)
        {
            var tmpRook = chessBoard[backRank, previousRookX];  // copies the rook

            ManageFirstRookMove(tmpRook, backRank);

            chessBoard[backRank, previousRookX] = null;

            Button rookSquare = GetButtonAtPosition(previousRookX, backRank);
            rookSquare.BackgroundImage = null;

            //transpose the rook
            tmpRook.x = (previousRookX == (int)Files.FirstFile) ? (int)Files.FourthFile : (int)Files.SixthFile;
            chessBoard[backRank, tmpRook.x] = tmpRook;

            Bitmap rookImage = SetImageToButton(tmpRook);

            rookSquare = GetButtonAtPosition(tmpRook.x, backRank);
            rookSquare.BackgroundImage = rookImage;

        }


        private void ManageFirstKingMove(int previusKingX, int previusKingY, int backRank)
        {
            firstKingMove[turn] = true;

            if (O_O[turn] && previusKingX == (int)Files.SeventhFile && previusKingY == backRank)
                ManageShortOrLongCastle((int)Files.SeventhFile, backRank);

            else if (O_O_O[turn] && previusKingX == (int)Files.ThirdFile && previusKingY == backRank)
                ManageShortOrLongCastle((int)Files.FirstFile, backRank);
        }


        private void ManageFirstRookMove(Piece selectedPiece, int backRank)
        {
            if (selectedPiece.x == (int)Files.FirstFile && selectedPiece.y == backRank)
                aRookFirstMove[turn] = true;

            if (selectedPiece.x == (int)Files.EighthFile && selectedPiece.y == backRank)
                hRookFirstMove[turn] = true;
        }

        
        private void ManageInvalidDiagonalPawnMoves(Piece pawn)
        {
            int movePawnTowardBlackOrWhite = (pawn.pieceType == PieceColor.White) ? -1 : 1;

            RemoveInvalidDiagonalPawnMoves(pawn.x + 1, pawn.y + movePawnTowardBlackOrWhite);
            RemoveInvalidDiagonalPawnMoves(pawn.x - 1, pawn.y + movePawnTowardBlackOrWhite);
        }


        private void RemoveInvalidDiagonalPawnMoves(int destinationX, int destinationY)
        {
            if (!ChessBoard.IsSquareOutsideTheBoard(destinationX, destinationY) &&
                (chessBoard.IsSquareNull(destinationX, destinationY) ||
                 chessBoard[destinationY, destinationX].pieceName == "K"))

                RemoveSquaresFromList(chessBoard.ValidMoves, destinationX, destinationY);
        }


        private void ManagePawnPromotion(Piece selectedPawn, int x, int y)
        {
            if (selectedPawn.y - 1 == (int)Ranks.EighthRank 
                && selectedPawn.pieceType == PieceColor.White ||
                selectedPawn.y + 1 == (int)Ranks.FirstRank && 
                selectedPawn.pieceType == PieceColor.Black)
            {

                var promotion = new PromotionForm(turn);
                promotion.ShowDialog();

                this.selectedPiece = new Piece(x, y, promotion.PieceName, selectedPawn.pieceType);

                Debug.WriteLine("Promotion: " + promotion.PieceName);
            }
        }


        // maybe it's better to create two objects, whiteKing and blackKing
        private Piece FindKing()
        {
            foreach (var piece in chessBoard)
                if (piece != null && 
                    piece.pieceType != currentPlayer[turn] && 
                    piece.pieceName == "K")
                    return piece;

            return null;
        }



        private void RemoveSquaresFromList(List<Square> list, int destinationX, int destinationY)
        {
            list.RemoveAll(square => square.x == destinationX && square.y == destinationY);
        }

                                                                                                              // these two in the chessBoard.cs

        private bool IsSquareInList(List<Square> list, int destinationX, int destinationY)
        {
            return list.Exists(square => square.x == destinationX && square.y == destinationY);
        }


        public void ClearDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            dictionary.Clear();
        }


        // Find the button at the specified position
        private Button GetButtonAtPosition(int x, int y)
        {
            foreach (var button in Controls.OfType<Button>())
            {
                var position = (ValueTuple<int, int>)button.Tag;

                if (position.Item1 == x && position.Item2 == y) return button;
            }

            return null;
        }
    }
}