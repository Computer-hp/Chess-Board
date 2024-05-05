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

        private bool isFirstMovePlayed = false;
        private bool isCheck = false;
        private bool[] firstKingMove = { false, false };
        private bool[] aRookFirstMove = { false, false };
        private bool[] hRookFirstMove = { false, false };
        private bool[] O_O = { false, false };
        private bool[] O_O_O = { false, false };

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


        public static Bitmap GetImageForButton(Piece piece)
        {
            string DIR = piece.Color.ToString().ToLower();
            string imagePath = DIR + "\\" + piece.Name + ".png";

            Bitmap originalImage = (Bitmap)Image.FromFile(projectPath + imagePath);
            return originalImage;
        }


        /* TODO

            1) when clicking a piece and then clicking a square with the same piece
                it should unselect the first clicked button and select the second button.

            2) change color of clicked button which contains a piece.

            3) in Dict StopCheckWithPiece pawns can't block check by moving straight.
                when pawn gives check, one of the two diagonal moves doesn't get cancelled, so for StopCheckWithPiece is also considered that
        */

        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            var destinationSquare = (ValueTuple<int, int>)clickedButton.Tag;

            var clickedSquare = chessBoard[destinationSquare.Item2, destinationSquare.Item1];

            if (!PieceGotClicked(clickedSquare) && selectedPiece == null) return;

            int firstRank = (int)Ranks.FirstRank;
            int backRank = Math.Abs((int)currentPlayer[turn] * firstRank - firstRank);

            if (selectedPiece == null)
            {
                ManageSelectedPiece(destinationSquare, backRank);
                return;
            }

            //  when an illegal move is played the stuff under 'condition' is not executed !!!
            if (!IsMoveLegal(destinationSquare) ||
                (isCheck && !IsMoveLegalWhenCheck(destinationSquare)))
            { 
                selectedPiece = null; 
                return; 
            }

            ValueTuple<int, int> originSquare = (selectedPiece.x, selectedPiece.y);

                                // selectedPiece --> Tuple called originSquare
            ManagePieceMovement(originSquare, destinationSquare, backRank);
            ManageClockTick();
            clickedButton.BackgroundImage = GetImageForButton(chessBoard[destinationSquare.Item2, destinationSquare.Item1]);
            ManageSituationAfterPieceMovement(destinationSquare);
           
            // final things to do
            chessBoard.MovePawnTowardsBlackOrWhite = -1 * (turn * 2 - 1);  
            chessBoard.ValidMoves.Clear();
            turn = (turn + 1) % 2;
            selectedPiece = null;

            chessBoard.PrintMovesThatCanStopCheck();
        }


        private void ManageSelectedPiece(ValueTuple<int, int> destinationSquare, int backRank)
        {
            selectedPiece = chessBoard[destinationSquare.Item2, destinationSquare.Item1];
            chessBoard.CalculateMoves(selectedPiece, "");

            Debug.WriteLine($"{selectedPiece.Name}, {selectedPiece.Color}");
            Debug.WriteLine(chessBoard.ToString() + "\n");

            if (selectedPiece.Name == "P") 
            { 
                ManageInvalidDiagonalPawnMoves(selectedPiece); 
                return; 
            }

            if (selectedPiece.Name != "K") return;

            RemoveInvalidSquaresOfKing(selectedPiece);

            if (!O_O_O[turn])  O_O_O[turn] = IsCastleLegal((int)Files.ThirdFile, (int)Files.FifthFile, backRank, aRookFirstMove[turn]);

            if (!O_O[turn])  O_O[turn] = IsCastleLegal((int)Files.SixthFile, (int)Files.EighthFile, backRank, hRookFirstMove[turn]);
        }


        private bool PieceGotClicked(Piece clickedSquare)
        {
            return (clickedSquare != null &&
                    clickedSquare.Color == currentPlayer[turn]);
        }



        private void ManageClockTick()
        {
            // after white moves, black timer starts
            if (!isFirstMovePlayed)
            {
                isFirstMovePlayed = true;
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
        }


        private bool IsMoveLegal(ValueTuple<int, int> destinationSquare)
        {
            return IsSquareInList(chessBoard.ValidMoves, destinationSquare) && 
                                                 !ChessBoard.IsSquareOutsideTheBoard(destinationSquare) &&
                                                 (chessBoard.IsSquareNull(destinationSquare) ||
                                                 chessBoard[destinationSquare.Item2, destinationSquare.Item1].Color != currentPlayer[turn]);
        }


        private void ManageSituationAfterPieceMovement(ValueTuple<int, int> destinationSquare)
        {
            if (chessBoard[destinationSquare.Item2, destinationSquare.Item1].Name == "K")
                return;

            Piece king = FindKing(); // maybe it's better to keep track of white & black king by creating 2 obj.

            if (!HasPieceGivenCheck((king.x, king.y), destinationSquare))
                return;

            HandleSituationAfterCheck(king);

            if (!IsCheckmate())
                return;

            var popUp = new RestartForm { StartPosition = FormStartPosition.CenterParent };
            popUp.ShowDialog(this);

            if (RestartForm.NewGame) isRestarted = true;

            else if (RestartForm.MainMenu) this.Close();
        }


        private void ManagePieceMovement(ValueTuple<int, int> originSquare, ValueTuple<int, int> destinationSquare, int backRank)
        {
            if (selectedPiece.Name == "P") ManagePawnPromotion(selectedPiece, destinationSquare);

            Button originalSquare = GetButtonAtPosition(originSquare);
            originalSquare.BackgroundImage = null;
            chessBoard[originSquare.Item2, originSquare.Item1] = null;
            chessBoard[destinationSquare.Item2, destinationSquare.Item1] = new Piece(destinationSquare.Item1, destinationSquare.Item2, 
                                                                                     selectedPiece.Name, selectedPiece.Color);

            if (selectedPiece.Name == "K" && !firstKingMove[turn])
                ManageFirstKingMove(destinationSquare, backRank);

            else if (selectedPiece.Name == "R")
                ManageFirstRookMove(selectedPiece, backRank);

            ref Piece movedPiece = ref chessBoard.GetPieceRef(destinationSquare.Item2, destinationSquare.Item1);

            movedPiece.x = destinationSquare.Item1;  // null reference i don't know why
            movedPiece.y = destinationSquare.Item2;
        }


        private bool IsMoveLegalWhenCheck(ValueTuple<int, int> destinationSquare)
        {
            chessBoard.CopyMoves.Clear();

            //Tuple<int, int> destinationSquare = Tuple.Create(selectedPiece.x, selectedPiece.y);

            if (selectedPiece.Name == "K") chessBoard.CopyMoves.AddRange(chessBoard.ValidMoves);

            else if (chessBoard.StopCheckWithPiece.ContainsKey(destinationSquare))
            {
                List<Square> squareList = chessBoard.StopCheckWithPiece[destinationSquare];

                if (IsSquareInList(squareList, destinationSquare))
                    chessBoard.CopyMoves.AddRange(chessBoard.StopCheckWithPiece[destinationSquare]);
            }

            if (!chessBoard.CopyMoves.Exists(square => IsSquareInList(chessBoard.ValidMoves, square))) return false;

            ClearDictionary(chessBoard.StopCheckWithPiece);
            
            isCheck = false;
            return true;
        }


        private bool HasPieceGivenCheck(ValueTuple<int, int> kingPosition, ValueTuple<int, int> destinationSquare)
        {
            chessBoard.ValidMoves.Clear();
            var lastPieceMoved = chessBoard[destinationSquare.GetY(), destinationSquare.GetX()];

            if (selectedPiece.Name == "R" || selectedPiece.Name == "Q")
                DefineDirectionTowardsKing("Straight", kingPosition, destinationSquare);

            else if (selectedPiece.Name == "B")
                DefineDirectionTowardsKing("Diagonal", kingPosition, destinationSquare);

            else chessBoard.CalculateMoves(lastPieceMoved, ""); // for the pawn or knight


            chessBoard.ValidMoves.Add(new Square(destinationSquare.GetX(), destinationSquare.GetY()));  // piece that gives check can also be captured
                                                                                 // to stop check (neccessary for Knight and Pawn)
            Debug.WriteLine("\ncheck = " + isCheck + '\n');
            return isCheck = IsCheck(kingPosition);
        }


        private bool IsCheck(ValueTuple<int, int> kingPosition)
        {
            if (IsSquareInList(chessBoard.ValidMoves, kingPosition)) return true;
            
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
                    piece.Name == "K" ||
                    piece.Color == selectedPiece.Color)
                    continue;
                
                chessBoard.CalculateMoves(piece, "");

                if (piece.Name == "P")
                {
                    // should also remove the 2 squares in front of the pawn. --> create a method to do that because it's done several times
                    // still works because when moving the king something controls the 2 squares!
                    ManageInvalidDiagonalPawnMoves(piece);
                }

                RemoveSquaresFromList(chessBoard.ValidMoves, (king.x, king.y));
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


        // TODO improve this method     // rename destinationSquare with 'newOriginSquareAfterPieceGotMoved'
        private void DefineDirectionTowardsKing(string moveTo, ValueTuple<int, int> kingPosition, ValueTuple<int, int> destinationSquare)
        {
            chessBoard.ValidMoves.Clear();

            string direction = (moveTo == "Straight") ? FindStraightDirection(kingPosition, destinationSquare) 
                                                      : FindDiagonalDirection(kingPosition, destinationSquare);

            Debug.Write($"\ndirection = {direction}\n");

            switch (moveTo)
            {
                case "Straight":
                    chessBoard.CalculateMoves(chessBoard[destinationSquare.GetY(), destinationSquare.GetX()], direction);
                    break;

                case "Diagonal":
                    chessBoard.CalculateMoves(chessBoard[destinationSquare.GetY(), destinationSquare.GetX()], direction);
                    break;
            }

            if (selectedPiece.Name == "Q" && !isCheck && moveTo != "Diagonal")
                DefineDirectionTowardsKing("Diagonal", kingPosition, destinationSquare);
        }


        private bool IsCastleLegal(int startingFile, int endingFile, int backRank, bool firstRookMove)
        {
            if (firstKingMove[turn] || firstRookMove) return false;

            // controls if [0, 1] is null for O_O_O
            if (startingFile == (int)Files.ThirdFile &&
                !chessBoard.IsSquareNull((startingFile - 1, endingFile)))
                return false;

            var king = selectedPiece;

            List<Square> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            for (; startingFile < endingFile; startingFile++)
            {
                if (!chessBoard.IsSquareNull((startingFile, backRank))) return false; // squares are never outside the board

                                // control if squares from starting to ending File are into opponents piece moves
                foreach (var piece in chessBoard)
                {
                    if (piece == null ||
                        piece.Name == "K" ||
                        piece.Color == king.Color)
                        continue;

                    chessBoard.CalculateMoves(piece, "");

                    if (piece.Name == "P")
                        chessBoard.ValidMoves.RemoveAll(square => square.x == piece.x && 
                                                        (square.y == piece.y - chessBoard.MovePawnTowardsBlackOrWhite ||
                                                        square.y == piece.y - chessBoard.MovePawnTowardsBlackOrWhite * 2));

                    if (IsSquareInList(chessBoard.ValidMoves, (startingFile, backRank))) return false;
                }
            }

            tmpKingMoves.Add(new Square(endingFile - 1, backRank)); // add the square to enable O_O or O_O_O
            chessBoard.ValidMoves.Clear();
            chessBoard.ValidMoves.AddRange(tmpKingMoves);
            return true;
        }


        private string FindStraightDirection(ValueTuple<int, int> kingPosition, ValueTuple<int, int> newPosOfLastMovedPiece)
        {
            if (kingPosition.GetX() == null ||
                kingPosition.GetY() == null || 
                (kingPosition != newPosOfLastMovedPiece)) return "";  // not sure that works

            return newPosOfLastMovedPiece.GetY() == kingPosition.GetY() 
                       ? (newPosOfLastMovedPiece.GetX() > kingPosition.GetX() ? "Left" : "Right") 
                       : (newPosOfLastMovedPiece.GetY() > kingPosition.GetY() ? "Down" : "Up");
        }


        private string FindDiagonalDirection(ValueTuple<int, int> kingPosition, ValueTuple<int, int> newPosOfLastMovedPiece)
        {
            if (kingPosition.GetX() == null ||
                kingPosition.GetY() == null || 
                newPosOfLastMovedPiece.GetX() == kingPosition.GetX() || newPosOfLastMovedPiece.GetY() == kingPosition.GetY()) return "";

            return newPosOfLastMovedPiece.GetY() > kingPosition.GetY() 
                       ? (newPosOfLastMovedPiece.GetX() > kingPosition.GetX() ? "LeftDown" : "RightDown")
                       : (newPosOfLastMovedPiece.GetX() < kingPosition.GetX() ? "RightUp" : "LeftUp");
        }



        // TODO remove also c1 or g1 square in case of O_O or O_O_O
        private void RemoveInvalidSquaresOfKing(Piece king)
        {
            List<Square> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null || 
                    piece.Color == king.Color ||
                    piece.Name == king.Name)
                    continue;


                chessBoard.CalculateMoves(piece, "");

                // to remove straight moves of the PAWN, by one square and/or two squares in case the pawn hasn't been moved yet
                if (piece.Name == "P")
                {
                    RemoveSquaresFromList(chessBoard.ValidMoves, (piece.x, piece.y + chessBoard.MovePawnTowardsBlackOrWhite));
                    RemoveSquaresFromList(chessBoard.ValidMoves, (piece.x, piece.y + chessBoard.MovePawnTowardsBlackOrWhite * 2));
                }

                tmpKingMoves.RemoveAll(square => IsSquareInList(chessBoard.ValidMoves, square)); // strange but ok
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

                    if (!ChessBoard.IsSquareOutsideTheBoard((x, y)) &&
                        !chessBoard.IsSquareNull((x, y)))
                        FindInvalidCapturesKing(king, chessBoard[y, x]);
        }


        

        private void FindInvalidCapturesKing(Piece king, Piece pieceNearKing)
        {
            if (king.Equals(pieceNearKing)) return;

            if (king.Color == pieceNearKing.Color) // if a piece with the same color is placed near the king
            {
                RemoveSquaresFromList(chessBoard.ValidMoves, (pieceNearKing.x, pieceNearKing.y));
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
                    piece.Color != pieceNearKing.Color)
                    continue;
                

                chessBoard.CalculateMoves(piece, "");

                if (IsSquareInList(chessBoard.ValidMoves, (pieceNearKing.x, pieceNearKing.y)))
                {
                    RemoveSquaresFromList(tmpKingMoves, (pieceNearKing.x, pieceNearKing.y));
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


            foreach (var square in chessBoard.ValidMoves)
                if (IsSquareInList(chessBoard.CopyMoves, square))
                    tmpMoves.Add(square);

            if (!tmpMoves.Any())
                return;

            ValueTuple<int, int> key = (piece.x, piece.y);;
            chessBoard.StopCheckWithPiece[key] = tmpMoves;
        }



        private void ManageShortOrLongCastle(int previousRookX, int backRank)
        {
            var tmpRook = chessBoard[backRank, previousRookX];  // copies the rook

            ManageFirstRookMove(tmpRook, backRank);

            chessBoard[backRank, previousRookX] = null;

            Button rookSquare = GetButtonAtPosition((previousRookX, backRank));
            rookSquare.BackgroundImage = null;

            //transpose the rook
            tmpRook.x = (previousRookX == (int)Files.FirstFile) ? (int)Files.FourthFile : (int)Files.SixthFile;
            chessBoard[backRank, tmpRook.x] = tmpRook;

            Bitmap rookImage = GetImageForButton(tmpRook);

            rookSquare = GetButtonAtPosition((tmpRook.x, backRank));
            rookSquare.BackgroundImage = rookImage;

        }


        private void ManageFirstKingMove(ValueTuple<int, int> destinationSquare, int backRank)
        {
            firstKingMove[turn] = true;

            if (O_O[turn] && 
                destinationSquare.Item1 == (int)Files.SeventhFile && 
                destinationSquare.Item2 == backRank)
                ManageShortOrLongCastle((int)Files.SeventhFile, backRank);

            else if (O_O_O[turn] && 
                     destinationSquare.Item1 == (int)Files.ThirdFile && 
                     destinationSquare.Item2 == backRank)
                     ManageShortOrLongCastle((int)Files.FirstFile, backRank);
        }


        private void ManageFirstRookMove(Piece rook, int backRank)
        {
            if (rook.x == (int)Files.FirstFile && rook.y == backRank)
                aRookFirstMove[turn] = true;

            if (rook.x == (int)Files.EighthFile && rook.y == backRank)
                hRookFirstMove[turn] = true;
        }

        
        private void ManageInvalidDiagonalPawnMoves(Piece pawn)
        {
            int movePawnTowardBlackOrWhite = (pawn.Color == PieceColor.White) ? -1 : 1;

            RemoveInvalidDiagonalPawnMoves((pawn.x + 1, pawn.y + movePawnTowardBlackOrWhite));
            RemoveInvalidDiagonalPawnMoves((pawn.x - 1, pawn.y + movePawnTowardBlackOrWhite));
        }


        private void RemoveInvalidDiagonalPawnMoves(ValueTuple<int, int> destinationSquare)
        {
            if (!ChessBoard.IsSquareOutsideTheBoard(destinationSquare) &&
                (chessBoard.IsSquareNull(destinationSquare) ||
                 chessBoard[destinationSquare.Item2, destinationSquare.Item1].Name == "K"))

                RemoveSquaresFromList(chessBoard.ValidMoves, destinationSquare);
        }


        private void ManagePawnPromotion(Piece selectedPawn, ValueTuple<int, int> destinationSquare)
        {
            if (selectedPawn.y - 1 == (int)Ranks.EighthRank 
                && selectedPawn.Color == PieceColor.White ||
                selectedPawn.y + 1 == (int)Ranks.FirstRank && 
                selectedPawn.Color == PieceColor.Black)
            {

                var promotion = new PromotionForm(turn);
                promotion.ShowDialog();

                this.selectedPiece = new Piece(destinationSquare.Item1, destinationSquare.Item2, promotion.Name, selectedPawn.Color);

                Debug.WriteLine("Promotion: " + promotion.Name);
            }
        }


        // maybe it's better to create two objects, whiteKing and blackKing
        private Piece FindKing()
        {
            foreach (var piece in chessBoard)
                if (piece != null && 
                    piece.Color != currentPlayer[turn] && 
                    piece.Name == "K")
                    return piece;

            return null;
        }



        private void RemoveSquaresFromList(List<Square> list, ValueTuple<int, int> destinationSquare)
        {
            list.RemoveAll(destinationSquare);
        }

                                                                                                              // these two in the chessBoard.cs

        private bool IsSquareInList(List<Square> list, ValueTuple<int, int> destinationSquare)
        {
            return list.Exists(destinationSquare);
        }


        public void ClearDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            dictionary.Clear();
        }


        // Find the button at the specified position
        private Button GetButtonAtPosition(ValueTuple<int, int> selectedSquare)
        {
            foreach (var button in Controls.OfType<Button>())
                if (selectedSquare == (ValueTuple<int, int>)button.Tag)
                    return button;

            return null;
        }
    }
}