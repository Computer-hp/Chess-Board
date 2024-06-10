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
using System.Runtime.Serialization;
using WindowHelper;
using ChessGame;


// TODO  Pin on pieces
// checkmate doesn't work for the knight

// FIX: king is able to move in the same direction as the one of the piece that has given check
//      this is because the CalculateMoves method calculates the moves until the position of the king.
//      if there are squares after the king, it should not be able to move.

namespace WinFormsApp1
{
    public partial class ChessBoardForm : Form
    {
        private const int BOARD_SIZE = 8;
        private const int N_PLAYERS = 2;
        public const int SQUARE_SIZE = 70;

        private int turn = 0;       // 0 for white, 1 for black

        private int[] secondsElapsed = new int[N_PLAYERS];

        private static readonly string projectPath = GetProjectPath();  // pathToImages

        private bool isFirstMovePlayed = false;
        private bool isCheck = false;
        private bool[] isKingFirstMove = { false, false };
        private bool[] isRook_A_FirstMove = { false, false };
        private bool[] isRook_H_FirstMove = { false, false };
        private bool[] O_O = { false, false };
        private bool[] O_O_O = { false, false };

        private Color? previousButtonColor = null;
        private static readonly PieceColor[] currentPlayer = { PieceColor.White, PieceColor.Black };

        private ChessBoard chessBoard;

        private Piece? selectedPiece = null;

        private Label[] timerLabel = new Label[2];

        private Timer[] timer = new Timer[2];

        private Button? lastClickedButton = null;

        private static readonly char[] piecesThatMoveStraight = { 'R', 'Q' };
        private static readonly char[] piecesThatMoveDiagonally = { 'B', 'Q' };

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
            DarkThemeWindowHelper.ApplyDarkTheme(this);
        }


        private static string GetProjectPath()
        {
            string appDirectory = Application.StartupPath;
            string imagesFolder = Path.GetFullPath(Path.Combine(appDirectory, "..\\..\\..\\Assets\\"));
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



        private void ManageClickedButton(Button clickedButton, (int x, int y) destinationSquare)
        {
            // 3 cases
            // empty button is clicked and no other button was clicked

            // button with piece got clicked and no other button was clicked

            // button with piece is green and a new button is clicked --> 1) button contains a piece
            //                                                        --> 2) button is empty and could be a valid square or not.
            
            // PROBLEM: example --> clicking second button after first doesnt' remove focus of the first
            // Maybe it's better to use an image in place of buttons and use the coordinates of the mouse.

            if (!chessBoard.IsSquareNull(destinationSquare))
            {
                if (lastClickedButton is not null)
                    lastClickedButton.BackColor = (Color)previousButtonColor!;

                else
                {
                    previousButtonColor = clickedButton.BackColor;
                    clickedButton.BackColor = Color.DarkSeaGreen;
                }

                return;
            }

            if (lastClickedButton is null)
                return;

            lastClickedButton.BackColor = (Color)previousButtonColor!;
        }
        


        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            var destinationSquare = ((int x, int y))clickedButton.Tag;
            var clickedSquare = chessBoard[destinationSquare.y, destinationSquare.x];

            if (!PieceGotClicked(clickedSquare) && selectedPiece is null) return;

            ManageClickedButton(clickedButton, destinationSquare);

            int firstRank = (int)Ranks.FirstRank;
            int backRank = Math.Abs((int)currentPlayer[turn] * firstRank - firstRank);

            if (selectedPiece is null)
            {
                ManageSelectedPiece(destinationSquare, backRank);
                lastClickedButton = clickedButton;
                return;
            }

        // when an illegal move is played the stuff under 'condition' is not executed !!!
        // ###
            if (!IsMoveLegal(destinationSquare) ||
                (isCheck && !IsMoveLegalWhenCheck(destinationSquare)))
            { 
                selectedPiece = null;
                lastClickedButton = null;
                return; 
            }


        // ###
        // create a method that is called something like ManagePieceAfterMoveIsLegal

            ValueTuple<int, int> originSquare = (selectedPiece.X, selectedPiece.Y);

                                // selectedPiece --> Tuple called originSquare
            ManagePieceMovement(originSquare, destinationSquare, backRank);
            ManageClockTick();
        // ###
            lastClickedButton!.BackgroundImage = null;
            clickedButton.BackgroundImage = GetImageForButton(chessBoard[destinationSquare.y, destinationSquare.x]); // maybe is good to use async and await,
                                                                                                                     // so i can make code more readable and
                                                                                                                     // change the BackgroundImage of the Button
            // then this method is fine
            ManageSituationAfterPieceMovement(destinationSquare);
           
        // final things to do
        // ###
            chessBoard.MovePawnTowardsBlackOrWhite = -1 * (turn * 2 - 1);  
            chessBoard.ValidMoves.Clear();
            lastClickedButton.BackColor = (Color)previousButtonColor!;
            turn = (turn + 1) % 2;
            selectedPiece = null;
            lastClickedButton = null;
        // ###

            chessBoard.PrintMovesThatCanStopCheck();
        }


        private void ManageSelectedPiece((int x, int y) destinationSquare, int backRank)
        {
            selectedPiece = chessBoard[destinationSquare.y, destinationSquare.x];
            chessBoard.CalculateMoves(selectedPiece);

            Debug.WriteLine($"{selectedPiece.Name}, {selectedPiece.Color}");
            Debug.WriteLine(chessBoard.ToString() + "\n");

            if (selectedPiece.Name == 'P') 
            { 
                ManageInvalidDiagonalPawnMoves(selectedPiece); 
                return; 
            }

            if (selectedPiece.Name != 'K') return;

            RemoveInvalidSquaresOfKing(selectedPiece);
                                                                //   secondfile         fifthfile
            if (!O_O_O[turn])  O_O_O[turn] = IsCastleLegal((int)Files.bFile, (int)Files.eFile, backRank, isRook_A_FirstMove[turn]);
//                                                                sixthfile             eighthfile
            if (!O_O[turn])  O_O[turn] = IsCastleLegal((int)Files.fFile, (int)Files.hFile, backRank, isRook_H_FirstMove[turn]);

            Debug.WriteLine("king moves:");
            Debug.WriteLine(chessBoard.ToString() + "\n");
        }


        private bool PieceGotClicked(Piece clickedSquare)
        {
            return clickedSquare is not null && 
                   clickedSquare.Color == currentPlayer[turn];
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


        private bool IsMoveLegal((int x, int y) destinationSquare)
        {
            return IsSquareInList(chessBoard.ValidMoves, destinationSquare) &&
                   !ChessBoard.IsSquareOutsideTheBoard(destinationSquare) &&
                   (chessBoard.IsSquareNull(destinationSquare) ||
                   !IsPieceSameColorAsPlayer(destinationSquare));
        }


        private void ManageSituationAfterPieceMovement((int x, int y) destinationSquare)
        {
            var lastMovedPiece = chessBoard[destinationSquare.y, destinationSquare.x];

            if (lastMovedPiece.Name == 'K')
                return;

            Piece? king = FindKing(); // maybe it's better to keep track of white & black king by creating 2 obj.

            if (!HasPieceGivenCheck((king!.X, king!.Y), destinationSquare))
                return;

            if (lastMovedPiece.Name == 'P')
                chessBoard.ValidMoves.RemoveAll(square => square.y == lastMovedPiece.Y + chessBoard.MovePawnTowardsBlackOrWhite);

            ManageSituationAfterCheck(king);

            if (!IsCheckmate())
                return;

            var popUp = new RestartForm { StartPosition = FormStartPosition.CenterParent };
            popUp.ShowDialog(this);

            if (RestartForm.NewGame) isRestarted = true;

            else if (RestartForm.MainMenu) this.Close();
        }


        private void ManagePieceMovement((int x, int y) originSquare, (int x, int y) destinationSquare, int backRank)
        {
            if (selectedPiece!.Name == 'P') ManagePawnPromotion(selectedPiece, destinationSquare);

            chessBoard[originSquare.y, originSquare.x] = null;
            chessBoard[destinationSquare.y, destinationSquare.x] = new Piece(destinationSquare.x, destinationSquare.y, 
                                                                             selectedPiece.Name, selectedPiece.Color);

            if (selectedPiece.Name == 'K' && !isKingFirstMove[turn])
                ManageFirstKingMove(destinationSquare, backRank);

            else if (selectedPiece.Name == 'R')
                ManageFirstRookMove(selectedPiece, backRank);
        }


        // TODO remove CopyMoves to make the code cleaner
        private bool IsMoveLegalWhenCheck(ValueTuple<int, int> destinationSquare)
        {
            chessBoard.CopyMoves.Clear();

            if (selectedPiece!.Name == 'K') chessBoard.CopyMoves.AddRange(chessBoard.ValidMoves);

            else if (chessBoard.StopCheckWithPiece.ContainsKey((selectedPiece.X, selectedPiece.Y)))
            {
                List<ValueTuple<int, int>> movesToStopCheck = chessBoard.StopCheckWithPiece[(selectedPiece.X, selectedPiece.Y)];

                if (IsSquareInList(movesToStopCheck, destinationSquare))
                    chessBoard.CopyMoves.AddRange(chessBoard.StopCheckWithPiece[(selectedPiece.X, selectedPiece.Y)]);
            }

            if (!chessBoard.CopyMoves.Exists(square => IsSquareInList(chessBoard.ValidMoves, square))) return false;

            ClearDictionary(chessBoard.StopCheckWithPiece);
            
            isCheck = false;
            return true;
        }


        private bool HasPieceGivenCheck((int x, int y) kingPosition, (int x, int y) destinationSquare)
        {
            chessBoard.ValidMoves.Clear(); // maybe is not neccesary
            var lastPieceMoved = chessBoard[destinationSquare.y, destinationSquare.x];

            if (selectedPiece!.Name == 'P' || selectedPiece.Name == 'N')
                chessBoard.CalculateMoves(lastPieceMoved);

            else DefineDirectionTowardsKing(selectedPiece.Name, kingPosition, destinationSquare);

            chessBoard.ValidMoves.Add((destinationSquare.x, destinationSquare.y));  // piece that gives check can also be captured
                                                                                 // to stop check (neccessary for Knight and Pawn)
            isCheck = IsCheck(kingPosition);
            Debug.WriteLine($"\n\n******* IS CHECK = {isCheck} *******\n");
            return isCheck;
        }


        private bool IsCheck(ValueTuple<int, int> kingPosition)
        {
            if (IsSquareInList(chessBoard.ValidMoves, kingPosition)) return true;
            
            return false;
        }


        private void ManageSituationAfterCheck(Piece king)
        {
            chessBoard.CopyMoves.Clear();
            
            chessBoard.CopyMoves.AddRange(chessBoard.ValidMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null ||
                    piece.Name == 'K' ||
                    piece.Color == selectedPiece!.Color)
                    continue;
                
                chessBoard.CalculateMoves(piece);

                if (piece.Name == 'P') ManageInvalidDiagonalPawnMoves(piece);

                RemoveSquaresFromList(chessBoard.ValidMoves, (king.X, king.Y));
                StopCheck(piece);
            }

            chessBoard.CalculateMoves(king);
            RemoveInvalidSquaresOfKing(king);
        }


        private bool IsCheckmate()
        {
            bool noValidMoves = !chessBoard.ValidMoves.Any();
            bool noBlockingPieces = !chessBoard.StopCheckWithPiece.Any(); // maybe incorrect

            return noValidMoves && noBlockingPieces;
        }


        private void DefineDirectionTowardsKing(char pieceName, (int, int) kingPosition, (int x, int y) destinationSquare)
        {
            Directions? direction = null;

            if (PieceMovesStraight(pieceName))
                direction = FindStraightDirection(kingPosition, destinationSquare);

            Debug.Write($"\n'direction' = {direction}\n");

            if (direction is not null)
                chessBoard.CalculateMoves(chessBoard[destinationSquare.y, destinationSquare.x], direction);

            if (pieceName == 'R' || direction is not null) return;

            if (PieceMovesDiagonally(pieceName))
                direction = FindDiagonalDirection(kingPosition, destinationSquare);

            Debug.Write($"\n'direction' = {direction}\n");

            if (direction is not null)
                chessBoard.CalculateMoves(chessBoard[destinationSquare.y, destinationSquare.x], direction);
        }


        private static bool PieceMovesStraight(char pieceName)
        {
            return Array.Exists(piecesThatMoveStraight, pieceNotation => pieceNotation == pieceName);
        }

        private static bool PieceMovesDiagonally(char pieceName)
        {
            return Array.Exists(piecesThatMoveDiagonally, pieceNotation => pieceNotation == pieceName);
        }


        private static Directions? FindStraightDirection((int x, int y) kingPosition, (int x, int y) newPosOfLastMovedPiece)
        {
            if (newPosOfLastMovedPiece == kingPosition) return null;

            return newPosOfLastMovedPiece.y == kingPosition.y
                        ? ((newPosOfLastMovedPiece.x > kingPosition.x) ? Directions.Left : Directions.Right) 

                        : (newPosOfLastMovedPiece.x == kingPosition.x  
                                ? ((newPosOfLastMovedPiece.y > kingPosition.y) ? Directions.Up : Directions.Down)
                                : null);
        }


        private static Directions? FindDiagonalDirection((int x, int y) kingPosition, (int x, int y) newPosOfLastMovedPiece)
        {
            if (newPosOfLastMovedPiece == kingPosition) return null;

            return newPosOfLastMovedPiece.y < kingPosition.y 
                       ? (newPosOfLastMovedPiece.x > kingPosition.x ? Directions.LeftDown : Directions.RightDown)
                       : (newPosOfLastMovedPiece.x > kingPosition.x ? Directions.LeftUp : Directions.RightUp);
        }


        private bool IsCastleLegal(int startingFile, int endingFile, int backRank, bool isRookFirstMove)
        {
            if (isKingFirstMove[turn] || isRookFirstMove) return false;

            var king = selectedPiece;

            List<ValueTuple<int, int>> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            for (int tmpStartingFile = startingFile; tmpStartingFile < endingFile; tmpStartingFile++)
            {
                if (!chessBoard.IsSquareNull((tmpStartingFile, backRank))) return false; // squares are never outside the board

                if (tmpStartingFile == (int)Files.bFile) continue;

                // control if squares from starting to ending File are into opponents piece moves ('create a method for this')
                foreach (var piece in chessBoard)
                {
                    if (piece == null || piece.Name == 'K' ||
                        piece.Color == king!.Color)
                        continue;

                    chessBoard.CalculateMoves(piece);

                    if (piece.Name == 'P')
                        chessBoard.ValidMoves.RemoveAll(square => square.x == piece.X &&
                                                        (square.y == piece.Y - chessBoard.MovePawnTowardsBlackOrWhite ||
                                                        square.y == piece.Y - chessBoard.MovePawnTowardsBlackOrWhite * 2));

                    if (IsSquareInList(chessBoard.ValidMoves, (tmpStartingFile, backRank)))
                    {
                        chessBoard.ValidMoves.Clear();
                        chessBoard.ValidMoves.AddRange(tmpKingMoves);
                        return false;
                    }
                }
            }

            tmpKingMoves.Add((startingFile + 1, backRank)); // add the square to enable O_O or O_O_O
            chessBoard.ValidMoves.Clear();
            chessBoard.ValidMoves.AddRange(tmpKingMoves);
            return true;
        }



        private void RemoveInvalidSquaresOfKing(Piece king)
        {
            List<(int x, int y)> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null || 
                    piece.Color == king.Color ||
                    piece.Name == king.Name)
                    continue;


                chessBoard.CalculateMoves(piece);

                // to remove straight moves of the PAWN, by one square and/or two squares in case the pawn hasn't been moved yet
                if (piece.Name == 'P')
                {
                    RemoveSquaresFromList(chessBoard.ValidMoves, (piece.X, piece.Y + chessBoard.MovePawnTowardsBlackOrWhite));
                    RemoveSquaresFromList(chessBoard.ValidMoves, (piece.X, piece.Y + chessBoard.MovePawnTowardsBlackOrWhite * 2));
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
            for (int x = king.X - 1; x < (king.X + 2); x++)
                for (int y = king.Y - 1; y < (king.Y + 2); y++)
                    if (!ChessBoard.IsSquareOutsideTheBoard((x, y)) && !chessBoard.IsSquareNull((x, y)))
                        FindInvalidCapturesKing(king, chessBoard[y, x]);
        }


        

        private void FindInvalidCapturesKing(Piece king, Piece pieceNearKing)
        {
            if (king.Equals(pieceNearKing)) return;

            if (king.Color == pieceNearKing.Color) // if a piece with the same color is placed near the king
            {
                RemoveSquaresFromList(chessBoard.ValidMoves, (pieceNearKing.X, pieceNearKing.Y));
                return;
            }


            List<ValueTuple<int, int>> tmpKingMoves = new();
            tmpKingMoves.AddRange(chessBoard.ValidMoves);

            // controls whether the piece near king is protected by another piece,
            // so the king can't capture it
            foreach (var piece in chessBoard)
            {
                if (piece == null || 
                    (piece.X == pieceNearKing.X && piece.Y == pieceNearKing.Y) ||
                    piece.Color != pieceNearKing.Color)
                    continue;

                chessBoard.CalculateMoves(piece);

                if (IsSquareInList(chessBoard.ValidMoves, (pieceNearKing.X, pieceNearKing.Y)))
                {
                    RemoveSquaresFromList(tmpKingMoves, (pieceNearKing.X, pieceNearKing.Y));
                    break;
                }
            }

            chessBoard.ValidMoves.Clear();
            chessBoard.ValidMoves.AddRange(tmpKingMoves);
        }



        private void StopCheck(Piece piece)
        {
            List<ValueTuple<int, int>> tmpMoves = new();

            foreach (var square in chessBoard.ValidMoves)
                if (IsSquareInList(chessBoard.CopyMoves, square))
                    tmpMoves.Add(square);

            if (!tmpMoves.Any())
                return;

            ValueTuple<int, int> key = (piece.X, piece.Y);
            chessBoard.StopCheckWithPiece[key] = tmpMoves;
        }



        private void ManageShortOrLongCastle(int previousRookX, int backRank)
        {
            var tmpRook = chessBoard[backRank, previousRookX];  // copies the rook

            ManageFirstRookMove(tmpRook, backRank);

            chessBoard[backRank, previousRookX] = null;

            Button? rookSquare = GetButtonAtPosition((previousRookX, backRank));
            rookSquare!.BackgroundImage = null;

            //transposes the rook
            tmpRook.X = (previousRookX == (int)Files.aFile) ? (int)Files.dFile : (int)Files.fFile;
            chessBoard[backRank, tmpRook.X] = tmpRook;

            Bitmap rookImage = GetImageForButton(tmpRook);

            rookSquare = GetButtonAtPosition((tmpRook.X, backRank));
            rookSquare!.BackgroundImage = rookImage;

        }


        private void ManageFirstKingMove((int x, int y) destinationSquare, int backRank)
        {
            isKingFirstMove[turn] = true;

            if (O_O[turn] && 
                destinationSquare.x == (int)Files.gFile && 
                destinationSquare.y == backRank)
                ManageShortOrLongCastle((int)Files.hFile, backRank);

            else if (O_O_O[turn] && 
                     destinationSquare.x == (int)Files.cFile && 
                     destinationSquare.y == backRank)
                     ManageShortOrLongCastle((int)Files.aFile, backRank);
        }


        private void ManageFirstRookMove(Piece rook, int backRank)
        {
            if (rook.X == (int)Files.aFile && rook.Y == backRank)
                isRook_A_FirstMove[turn] = true;

            if (rook.X == (int)Files.hFile && rook.Y == backRank)
                isRook_H_FirstMove[turn] = true;
        }

        
        private void ManageInvalidDiagonalPawnMoves(Piece pawn)
        {
            int movePawnTowardBlackOrWhite = (pawn.Color == PieceColor.White) ? -1 : 1;

            RemoveInvalidDiagonalPawnMoves((pawn.X + 1, pawn.Y + movePawnTowardBlackOrWhite));
            RemoveInvalidDiagonalPawnMoves((pawn.X - 1, pawn.Y + movePawnTowardBlackOrWhite));
        }


        private void RemoveInvalidDiagonalPawnMoves((int x, int y) destinationSquare)
        {
            if (!ChessBoard.IsSquareOutsideTheBoard(destinationSquare) &&
                (chessBoard.IsSquareNull(destinationSquare) ||
                 chessBoard[destinationSquare.y, destinationSquare.x].Name == 'K'))

                RemoveSquaresFromList(chessBoard.ValidMoves, destinationSquare);
        }


        private void ManagePawnPromotion(Piece selectedPawn, (int x, int y) destinationSquare)
        {
            if (!IsPawnOneRankFromPromoting(selectedPawn)) return;

            var promotion = new PromotionForm(turn);
            promotion.ShowDialog();

            this.selectedPiece = new Piece(destinationSquare.x, destinationSquare.y, promotion.PromotedPieceName, selectedPawn.Color);
            Debug.Write($"\nPromotion: {promotion.Name}\n");
        }
        

        private static bool IsPawnOneRankFromPromoting(Piece selectedPawn)
        {
            return (selectedPawn.Y - 1 == (int)Ranks.EighthRank && selectedPawn.Color == PieceColor.White) ||
                   (selectedPawn.Y + 1 == (int)Ranks.FirstRank && selectedPawn.Color == PieceColor.Black);
        }



        // maybe it's better to create two objects, whiteKing and blackKing
        private Piece? FindKing()
        {
            foreach (var piece in chessBoard)
                if (piece != null && 
                    piece.Color != currentPlayer[turn] && 
                    piece.Name == 'K')
                    return piece;

            return null;
        }



        private static void RemoveSquaresFromList(List<ValueTuple<int, int>> list, ValueTuple<int, int> destinationSquare)
        {
            list.Remove(destinationSquare);
        }


        private static bool IsSquareInList(List<ValueTuple<int, int>> list, ValueTuple<int, int> destinationSquare)
        {
            return list.Exists(square => square == destinationSquare);
        }


        public static void ClearDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            dictionary.Clear();
        }


        private bool IsPieceSameColorAsPlayer((int x, int y) destinationSquare)
        {
            return chessBoard[destinationSquare.y, destinationSquare.x].Color == currentPlayer[turn];
        }


        // Find the button at the specified position
        private Button? GetButtonAtPosition(ValueTuple<int, int> selectedSquare)
        {
            foreach (var button in Controls.OfType<Button>())
                if (selectedSquare == (ValueTuple<int, int>)button.Tag)
                    return button;

            return null;
        }
    }
}