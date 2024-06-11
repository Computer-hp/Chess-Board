using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessLogic;
using WinFormsApp1;
using Events;

namespace ChessGame
{
    public class Game
    {
        private const int BOARD_SIZE = 8;
        private const int N_PLAYERS = 2;
        public const int  SQUARE_SIZE = 70;

        private int[] secondsElapsed = new int[N_PLAYERS];

        private bool   isCheck = false;
        private bool[] isKingFirstMove = { false, false };
        private bool[] isRook_A_FirstMove = { false, false };
        private bool[] isRook_H_FirstMove = { false, false };
        private bool[] O_O = { false, false };
        private bool[] O_O_O = { false, false };

        private static readonly PieceColor[] currentPlayer = { PieceColor.White, PieceColor.Black };

        private ChessBoard chessBoard;

        private Piece? selectedPiece = null;

        private static readonly char[] piecesThatMoveStraight = { 'R', 'Q' };
        private static readonly char[] piecesThatMoveDiagonally = { 'B', 'Q' };

        public int Turn { get; private set; } = 0;       // 0 for white, 1 for black
        public int MovePawnTowardsBlackOrWhite { get; private set; } = -1;
        public bool IsFirstMovePlayed { get; set; } = false;

        public event EventHandler<CastleEventArgs> Castle;
        public event EventHandler<CheckmateEventArgs> Checkmate;


        public Game()
        {
            chessBoard = new ChessBoard();
            chessBoard.InitializePieces();
        }


        public void ManageUIClickedButton((int x, int y) destinationSquare)
        {
            var clickedSquare = chessBoard[destinationSquare];

            if (!PieceGotClicked(clickedSquare) && selectedPiece is null) return;

            // there was ManageClickedButton to eventualy change the color of the button.

            int firstRank = (int)Ranks.FirstRank;
            int backRank = Math.Abs((int)currentPlayer[Turn] * firstRank - firstRank);

            if (selectedPiece is null)
            {
                ManageSelectedPiece(destinationSquare, backRank);
                //lastClickedButton = clickedButton;
                return;
            }

        // when an illegal move is played the stuff under 'condition' is not executed !!!
        // ###
            if (!IsMoveLegal(destinationSquare) ||
                (isCheck && !IsMoveLegalWhenCheck(destinationSquare)))
            { 
                selectedPiece = null;
                //lastClickedButton = null;
                return; 
            }


        // ###
        // create a method that is called something like ManagePieceAfterMoveIsLegal

            ValueTuple<int, int> originSquare = (selectedPiece.X, selectedPiece.Y);

                                // selectedPiece --> Tuple called originSquare
            ManagePieceMovement(originSquare, destinationSquare, backRank);
            //ManageClockTick();
        // ###
            //lastClickedButton!.BackgroundImage = null;
            //clickedButton.BackgroundImage = GetImageForButton(chessBoard[destinationSquare.y, destinationSquare.x]); // maybe is good to use async and await,
                                                                                                                     // so i can make code more readable and
                                                                                                                     // change the BackgroundImage of the Button
            // then this method is fine
            ManageSituationAfterPieceMovement(destinationSquare);
           
        // final things to do
        // ###
            chessBoard.MovePawnTowardsBlackOrWhite = -1 * (Turn * 2 - 1);  
            chessBoard.ValidMoves.Clear();
            //lastClickedButton.BackColor = (Color)previousButtonColor!;
            Turn = (Turn + 1) % 2;
            selectedPiece = null;
            //lastClickedButton = null;
        // ###

            chessBoard.PrintMovesThatCanStopCheck();
        }
        

        private bool PieceGotClicked(Piece clickedSquare)
        {
            return clickedSquare is not null && 
                   clickedSquare.Color == currentPlayer[Turn];
        }
        

        private void ManageSelectedPiece((int x, int y) destinationSquare, int backRank)
        {
            selectedPiece = chessBoard[destinationSquare];
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
            if (!O_O_O[Turn])  O_O_O[Turn] = IsCastleLegal((int)Files.bFile, (int)Files.eFile, backRank, isRook_A_FirstMove[Turn]);
    //                                                                sixthfile             eighthfile
            if (!O_O[Turn])  O_O[Turn] = IsCastleLegal((int)Files.fFile, (int)Files.hFile, backRank, isRook_H_FirstMove[Turn]);

            Debug.WriteLine("king moves:");
            Debug.WriteLine(chessBoard.ToString() + "\n");
        }


        private bool IsMoveLegal((int x, int y) destinationSquare)
        {
            return IsSquareInList(chessBoard.ValidMoves, destinationSquare) &&
                   !ChessBoard.IsSquareOutsideTheBoard(destinationSquare) &&
                   (chessBoard.IsSquareNull(destinationSquare) ||
                   !IsPieceSameColorAsPlayer(destinationSquare));
        }


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
        

        private void ManagePieceMovement((int x, int y) originSquare, (int x, int y) destinationSquare, int backRank)
        {
            if (selectedPiece!.Name == 'P') ManagePawnPromotion(selectedPiece, destinationSquare);

            chessBoard[originSquare.y, originSquare.x] = null;
            chessBoard[destinationSquare.y, destinationSquare.x] = new Piece(destinationSquare.x, destinationSquare.y, 
                                                                             selectedPiece.Name, selectedPiece.Color);

            if (selectedPiece.Name == 'K' && !isKingFirstMove[Turn])
                ManageFirstKingMove(destinationSquare, backRank);

            else if (selectedPiece.Name == 'R')
                ManageFirstRookMove(selectedPiece, backRank);
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

            if (!IsCheckmate()) // should also add Draw
                return;

            OnCheckmate(new CheckmateEventArgs(currentPlayer[Turn].ToString()));
        }


        protected virtual void OnCheckmate(CheckmateEventArgs e)
        {
            Checkmate?.Invoke(this, e);
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
            if (isKingFirstMove[Turn] || isRookFirstMove) return false;

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



        private void ManageShortOrLongCastle((int x, int y) kingDestSquare, int previousRookX, int backRank)
        {
            (int x, int y) previousRookPosition = (previousRookX, backRank);
            ManageFirstRookMove(previousRookPosition, backRank);

            chessBoard[previousRookPosition] = null;

            

            //transposes the rook

            var tmpRook = chessBoard[previousRookPosition];  // copies the rook
            tmpRook.X = (previousRookX == (int)Files.aFile) ? (int)Files.dFile : (int)Files.fFile;
            chessBoard[backRank, tmpRook.X] = tmpRook;

            /*
                        */

            OnCastle(new CastleEventArgs(kingDestSquare, (tmpRook.X, tmpRook.Y)));
        }


        protected virtual void OnCastle(CastleEventArgs e)
        {
            Castle?.Invoke(this, e);
        }


        private void ManageFirstKingMove((int x, int y) destinationSquare, int backRank)
        {
            isKingFirstMove[Turn] = true;

            if (O_O[Turn] && 
                destinationSquare.x == (int)Files.gFile && 
                destinationSquare.y == backRank)
                ManageShortOrLongCastle(destinationSquare, (int)Files.hFile, backRank);

            else if (O_O_O[Turn] && 
                     destinationSquare.x == (int)Files.cFile && 
                     destinationSquare.y == backRank)
                     ManageShortOrLongCastle(destinationSquare, (int)Files.aFile, backRank);
        }


        // problem: if i return back the rook it probably cocount's as not yet moved
        private void ManageFirstRookMove((int x, int y) rookPosition, int backRank)
        {
            if (rookPosition.x == (int)Files.aFile && rookPosition.y == backRank)
                isRook_A_FirstMove[Turn] = true;

            if (rookPosition.x == (int)Files.hFile && rookPosition.y == backRank)
                isRook_H_FirstMove[Turn] = true;
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

            var promotion = new PromotionForm(Turn);
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
                    piece.Color != currentPlayer[Turn] && 
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
            return chessBoard[destinationSquare.y, destinationSquare.x].Color == currentPlayer[Turn];
        }
    }
}
