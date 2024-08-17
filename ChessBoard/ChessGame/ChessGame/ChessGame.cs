using System.Diagnostics;
using ChessLogic;
using ChessUI;
using Events;

namespace ChessGame
{
    public class Game
    {
        private int currentBackRank;
        private int turn = 0;       // 0 for white, 1 for black

        private bool    isCheck =            false;
        private bool[]  isKingFirstMove =    { false, false };
        private bool[]  isRook_A_FirstMove = { false, false };
        private bool[]  isRook_H_FirstMove = { false, false };
        private bool[]  O_O =                { false, false };
        private bool[]  O_O_O =              { false, false };

        private static readonly PieceColor[] currentPlayer = { PieceColor.White, PieceColor.Black };

        private static readonly char[] piecesThatMoveStraight   = { 'R', 'Q' };
        private static readonly char[] piecesThatMoveDiagonally = { 'B', 'Q' };

        private ChessBoard chessBoard;

        private Piece? selectedPiece = null;
        private (int x, int y)[] whiteAndBlackKingPos = new (int x, int y)[2];

        private List<(int x, int y)> validMoves             = new();
        private List<(int x, int y)> validKingMoves         = new();
        private List<(int x, int y)> piecesThatPinnedAPiece = new();
        private List<(int x, int y)> pinnedPieces           = new();
        private Dictionary<(int x, int y), List<(int validX, int validY)>> piecesAbleToStopCheck = new();

        public const int SQUARE_SIZE = 70;

        public bool IsLastClickedButtonNull          = true;
        public bool HasLastClickedButtonChangedColor = true;

        public event EventHandler<CastleEventArgs>?           Castle;
        public event EventHandler<GameFinishEventArgs>?       GameFinish;
        public event EventHandler<UIPieceMovementEventArgs>?  UIPieceMovement;
        public event EventHandler<UIClockTickEventArgs>?      UIClockTick;
        
        public event ManageUIPromotion? Promotion;


        public Game()
        {
            chessBoard = new ChessBoard();

            whiteAndBlackKingPos[0] = ((int)Files.e, (int)Ranks.First);
            whiteAndBlackKingPos[1] = ((int)Files.e, (int)Ranks.Eighth);

            chessBoard.InitializePieces();
        }


        public void ManageUIClickedButton((int x, int y) squareCoord)
        {
            Piece clickedSquare = chessBoard[squareCoord];

            if (!IsPieceClicked(clickedSquare) && selectedPiece is null) return;

            int firstRank = (int)Ranks.First;
            currentBackRank = Math.Abs((int)currentPlayer[turn] * firstRank - firstRank);

            if (selectedPiece is null)
            {
                // Piece is selected
                selectedPiece = chessBoard[squareCoord];
                validMoves = chessBoard.CalculateMoves(selectedPiece);

                Debug.WriteLine($"{selectedPiece.Name}, {selectedPiece.Color}");
                Debug.WriteLine(ChessBoard.PrintMoves(validMoves) + "\n");

                if (selectedPiece.Name == 'P') 
                    ManageInvalidDiagonalPawnMoves(selectedPiece); 

                else if (selectedPiece.Name == 'K')
                {
                    RemoveKingSquaresCoveredByPieces(selectedPiece);
                    CheckPiecesNearKing(selectedPiece);

                    if (!O_O_O[turn])  
                        O_O_O[turn] = IsCastleLegal((int)Files.b, (int)Files.e, isRook_A_FirstMove[turn]);

                    if (!O_O[turn])
                        O_O[turn] = IsCastleLegal((int)Files.f, (int)Files.h, isRook_H_FirstMove[turn]);

                    Debug.WriteLine("king moves:");
                    Debug.WriteLine(ChessBoard.PrintMoves(validMoves) + "\n");
                }

                IsLastClickedButtonNull = false;
                HasLastClickedButtonChangedColor = false;
                return;
            }
            else if (!IsMoveLegal(squareCoord) ||
                     (isCheck && !IsMoveLegalWhenCheck(squareCoord)) ||
                     pinnedPieces.Contains(squareCoord))
            {
                // An illegal move is played
                selectedPiece = null;
                IsLastClickedButtonNull = true;
                HasLastClickedButtonChangedColor = false;
                return; 
            }

            (int x, int y) originSquare = (selectedPiece.X, selectedPiece.Y);
            (int x, int y) destSquare = (squareCoord);
            char pieceName = selectedPiece.Name;

            if (pieceName == 'P' &&
                IsPawnOneRankFromPromoting(selectedPiece))
            {
                // Manage pawn promotion
                char promotedPieceName = Promotion!(turn, destSquare);
                Debug.Write($"\n********* Promoted piece: '{promotedPieceName}' *********\n");
                pieceName = promotedPieceName;
            }

            // Manage piece movement in the matrix
            chessBoard[originSquare.y, originSquare.x] = null;
            chessBoard[destSquare.y, destSquare.x] = new Piece(destSquare.x, destSquare.y, 
                                                               pieceName, selectedPiece.Color);

            if (pieceName == 'K')
            {
                if (!isKingFirstMove[turn])
                { 
                    // Manage first king move
                    var kingDestSquare = destSquare;
                    isKingFirstMove[turn] = true;

                    // After the king has been moved for the first time
                    // controls if the played move is O-O or O-O-O.
                    if (O_O[turn] && 
                        kingDestSquare.x == (int)Files.g && 
                        kingDestSquare.y == currentBackRank)
                    {
                        ManageShortOrLongCastle((int)Files.h);
                        OnCastle(new CastleEventArgs(kingDestSquare, ((int)Files.f, currentBackRank), currentPlayer[turn]));
                    }

                    else if (O_O_O[turn] && 
                             kingDestSquare.x == (int)Files.c && 
                             kingDestSquare.y == currentBackRank)
                    {

                        ManageShortOrLongCastle((int)Files.a);
                        OnCastle(new CastleEventArgs(kingDestSquare, ((int)Files.d, currentBackRank), currentPlayer[turn]));
                    }
                }

                whiteAndBlackKingPos[turn] = destSquare;
            }
            else if (pieceName == 'R') ManageFirstRookMove(originSquare);

            // Manage piece movement in the UI
            OnPieceMovement(new UIPieceMovementEventArgs(destSquare, currentPlayer[turn], pieceName), 
                            new UIClockTickEventArgs(turn));


            ManageSituationAfterPieceMovement(squareCoord);
            int oppositeClock = (turn + 1) % 2;

            if (!isCheck && IsDraw()) OnGameFinish(new GameFinishEventArgs(oppositeClock, "Draw"));


        // *******************************************************************
        //                              Final Things

            chessBoard.MovePawnTowardsBlackOrWhite = -1 * (turn * 2 - 1);
            HasLastClickedButtonChangedColor = true;
            IsLastClickedButtonNull = true;
            turn = (turn + 1) % 2;
            selectedPiece = null;
            ChessBoard.PrintMovesThatCanStopCheck(piecesAbleToStopCheck);

        // *******************************************************************
        }


        private void CheckIfPiecePinnedOpponentPiece((int x, int y) opponentKingPos, (int x, int y) destinationSquare)
        {
            var direction = DefineDirectionTowardsKing(selectedPiece.Name, opponentKingPos, destinationSquare);
            validMoves = chessBoard.CalculateMoves(chessBoard[destinationSquare.y, destinationSquare.x], direction);
            
            foreach ()
        }


        private void ManageSituationAfterPieceMovement((int x, int y) destinationSquare)
        {
            var lastMovedPiece = chessBoard[destinationSquare];

            if (lastMovedPiece.Name == 'K') return;

            int oppositeKingIdx = (turn + 1) % 2;
            Piece opponentKing = chessBoard[whiteAndBlackKingPos[oppositeKingIdx]];

            if (!HasPieceGivenCheck((opponentKing.X, opponentKing.Y), destinationSquare))
            {
                CheckIfPiecePinnedOpponentPiece((opponentKing.X, opponentKing.Y), destinationSquare);
                return;
            }

            // keeps only the position of the piece, removes all other moves
            if (lastMovedPiece.Name == 'P' || lastMovedPiece.Name == 'N')
                validMoves.RemoveAll(square => square.x != lastMovedPiece.X && square.y != lastMovedPiece.Y);

            var squaresBetweenKingAndThePieceThatGaveCheck = new List<(int x, int y)>();
            squaresBetweenKingAndThePieceThatGaveCheck.AddRange(validMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null ||
                    piece.Name == 'K' ||
                    piece.Color == selectedPiece!.Color)
                    continue;
                
                validMoves = chessBoard.CalculateMoves(piece);

                if (piece.Name == 'P') ManageInvalidDiagonalPawnMoves(piece);

                RemoveSquaresFromList(validMoves, (opponentKing.X, opponentKing.Y));
                ControlIfPieceIsAbleToStopCheck(squaresBetweenKingAndThePieceThatGaveCheck, piece);
            }

            validMoves = chessBoard.CalculateMoves(opponentKing); // this is done to check later if king is able to move
            validMoves.RemoveAll(kingMove => kingMove.x == lastMovedPiece.X || kingMove.y == lastMovedPiece.Y);

            // Removing of illegal king moves

            int sign = 0;

            if ((lastMovedPiece.X > opponentKing.X && lastMovedPiece.Y > opponentKing.Y) ||
                (lastMovedPiece.X < opponentKing.X && lastMovedPiece.Y < opponentKing.Y))
                sign = -1;

            else if ((lastMovedPiece.X > opponentKing.X && lastMovedPiece.Y < opponentKing.Y) ||
                     (lastMovedPiece.X < opponentKing.X && lastMovedPiece.Y > opponentKing.Y))
                sign = 1;

            for (int i = validMoves.Count - 1, movesToRemove = 2; i >= 0 && movesToRemove > 0; i--)
                if (Math.Abs(validMoves[i].x + sign * validMoves[i].y) == Math.Abs(lastMovedPiece.X + sign * lastMovedPiece.Y))
                {
                    // Removes king moves which are in the same direction as the check.
                    validMoves.RemoveAt(i);
                    movesToRemove--;
                }

            RemoveKingSquaresCoveredByPieces(opponentKing);
            CheckPiecesNearKing(opponentKing);

            validKingMoves.Clear();
            validKingMoves.AddRange(validMoves);

            if (!IsCheckmate()) return;

            int oppositeClock = (turn + 1) % 2;
            OnGameFinish(new GameFinishEventArgs(oppositeClock, currentPlayer[turn].ToString()));
        }


        private Directions? DefineDirectionTowardsKing(char pieceName, (int, int) opponentKingPos, (int x, int y) destinationSquare)
        {
            Directions? direction = null;

            if (PieceMovesStraight(pieceName))
                direction = FindStraightDirection(opponentKingPos, destinationSquare);

            Debug.Write($"\n'direction' = {direction}\n");

            if (pieceName == 'R' || direction is not null) return direction;

            if (PieceMovesDiagonally(pieceName))
                direction = FindDiagonalDirection(opponentKingPos, destinationSquare);

            Debug.Write($"\n'direction' = {direction}\n");
            return direction;
        }


        private static Directions? FindStraightDirection((int x, int y) opponentKingPos, (int x, int y) newPosOfLastMovedPiece)
        {
            if (newPosOfLastMovedPiece == opponentKingPos) return null;

            return newPosOfLastMovedPiece.y == opponentKingPos.y
                        ? ((newPosOfLastMovedPiece.x > opponentKingPos.x) ? Directions.Left : Directions.Right) 

                        : (newPosOfLastMovedPiece.x == opponentKingPos.x  
                                ? ((newPosOfLastMovedPiece.y > opponentKingPos.y) ? Directions.Up : Directions.Down)
                                : null);
        }


        private static Directions? FindDiagonalDirection((int x, int y) opponentKingPos, (int x, int y) newPosOfLastMovedPiece)
        {
            if (newPosOfLastMovedPiece == opponentKingPos) return null;

            return newPosOfLastMovedPiece.y < opponentKingPos.y 
                       ? (newPosOfLastMovedPiece.x > opponentKingPos.x ? Directions.LeftDown : Directions.RightDown)
                       : (newPosOfLastMovedPiece.x > opponentKingPos.x ? Directions.LeftUp : Directions.RightUp);
        }


        private void RemoveKingSquaresCoveredByPieces(Piece king)
        {
            List<(int x, int y)> tmpKingMoves = new();
            tmpKingMoves.AddRange(validMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null || 
                    piece.Color == king.Color)
                    continue;


                validMoves = chessBoard.CalculateMoves(piece);

                // to remove straight moves of the Pawn, by one square and/or two squares in case the pawn hasn't been moved yet
                if (piece.Name == 'P')
                {
                    RemoveSquaresFromList(validMoves, (piece.X, piece.Y - chessBoard.MovePawnTowardsBlackOrWhite));
                    RemoveSquaresFromList(validMoves, (piece.X, piece.Y - chessBoard.MovePawnTowardsBlackOrWhite * 2));
                }

                tmpKingMoves.RemoveAll(square => IsSquareInList(validMoves, square));
            }
            
            validMoves.Clear();  // if this is removed than validMoves will contain the moves of the piece that protects the one that gave check
            validMoves.AddRange(tmpKingMoves);
        }


        private void CheckPiecesNearKing(Piece king)
        {
            RemoveMovesThatTargetPiecesOfSameColor(ref validMoves, king.Color);

            // Removes the king's moves that involve
            // capturing a protected piece of the opponent.
            foreach (var square in validMoves)
            {
                if (chessBoard.IsSquareNull(square)) continue;
                    
                var pieceNearKingPos = square;
                List<ValueTuple<int, int>> tmpKingMoves = new();
                tmpKingMoves.AddRange(validMoves);

                if (IsPieceNearKingProtected(king, pieceNearKingPos))
                    RemoveSquaresFromList(tmpKingMoves, pieceNearKingPos); // King cannot capture the protected piece

                validMoves.Clear();
                validMoves.AddRange(tmpKingMoves);
            }
        }

        private void ControlIfPieceIsAbleToStopCheck(List<(int x, int y)> criticalSquares, Piece piece)
        {
            List<ValueTuple<int, int>> tmpMoves = new();

            foreach (var square in validMoves)
                if (IsSquareInList(criticalSquares, square))
                    tmpMoves.Add(square);

            if (!tmpMoves.Any()) return;

            ValueTuple<int, int> key = (piece.X, piece.Y);
            piecesAbleToStopCheck[key] = tmpMoves;
        }


        private void ManageFirstRookMove((int x, int y) rookPosition)
        {
            if (rookPosition.x == (int)Files.a && rookPosition.y == currentBackRank)
                isRook_A_FirstMove[turn] = true;

            if (rookPosition.x == (int)Files.h && rookPosition.y == currentBackRank)
                isRook_H_FirstMove[turn] = true;
        }


        private void ManageShortOrLongCastle(int previousRookX)
        {
            ManageFirstRookMove((previousRookX, currentBackRank));

            var tmpRook = chessBoard[currentBackRank, previousRookX];  // copies the rook
            chessBoard[currentBackRank, previousRookX] = null;

            //transposes the rook
            tmpRook.X = (previousRookX == (int)Files.a) ? (int)Files.d : (int)Files.f;
            Debug.WriteLine($"\t\tRook moved to y, x: {tmpRook.Y}, {tmpRook.X}\n");
            chessBoard[currentBackRank, tmpRook.X] = tmpRook;
        }
        
        
        private void ManageInvalidDiagonalPawnMoves(Piece pawn)
        {
            validMoves.RemoveAll(square => square.x != pawn.X && 
                                 (chessBoard.IsSquareNull(square) || chessBoard[square].Name == 'K'));
        }



        //=============================================================================


        private bool IsPieceClicked(Piece clickedSquare)
        {
            return clickedSquare is not null && 
                   clickedSquare.Color == currentPlayer[turn];
        }
        
        private static bool PieceMovesStraight(char pieceName)
        {
            return Array.Exists(piecesThatMoveStraight, pieceNotation => pieceNotation == pieceName);
        }

        private static bool PieceMovesDiagonally(char pieceName)
        {
            return Array.Exists(piecesThatMoveDiagonally, pieceNotation => pieceNotation == pieceName);
        }

        private bool IsMoveLegal((int x, int y) destinationSquare)
        {
            return IsSquareInList(validMoves, destinationSquare) &&
                   !ChessBoard.IsSquareOutsideTheBoard(destinationSquare) &&
                   (chessBoard.IsSquareNull(destinationSquare) ||
                   !IsPieceSameColorAsPlayer(destinationSquare));
        }

        private bool IsMoveLegalWhenCheck(ValueTuple<int, int> destinationSquare)
        {
            if (selectedPiece!.Name == 'K')
            {
                if (!IsSquareInList(validKingMoves, destinationSquare)) return false;
                    
                isCheck = false;
            }
            else if (piecesAbleToStopCheck.ContainsKey((selectedPiece.X, selectedPiece.Y)))
            {
                List<ValueTuple<int, int>> movesToStopCheck = piecesAbleToStopCheck[(selectedPiece.X, selectedPiece.Y)];

                if (!IsSquareInList(movesToStopCheck, destinationSquare)) return false; 
                isCheck = false;
            }

            ClearDictionary(piecesAbleToStopCheck);
            isCheck = false;
            return true;
        }

        private bool IsCastleLegal(int startingFile, int endingFile, bool isRookFirstMove)
        {
            if (isKingFirstMove[turn] || isRookFirstMove) return false;

            var king = selectedPiece;

            List<ValueTuple<int, int>> tmpKingMoves = new();
            tmpKingMoves.AddRange(validMoves);

            for (int tmpStartingFile = startingFile; tmpStartingFile < endingFile; tmpStartingFile++)
            {
                if (!chessBoard.IsSquareNull((tmpStartingFile, currentBackRank))) return false; // squares are never outside the board!

                if (tmpStartingFile == (int)Files.b) continue;

                // control if squares from starting to ending 'file' are into opponents piece moves (create a method for this)
                foreach (var piece in chessBoard)
                {
                    if (piece == null || piece.Name == 'K' ||
                        piece.Color == king!.Color)
                        continue;

                    validMoves = chessBoard.CalculateMoves(piece);

                    if (piece.Name == 'P')
                        validMoves.RemoveAll(square => square.x == piece.X &&
                                                        (square.y == piece.Y - chessBoard.MovePawnTowardsBlackOrWhite ||
                                                        square.y == piece.Y - chessBoard.MovePawnTowardsBlackOrWhite * 2));

                    if (IsSquareInList(validMoves, (tmpStartingFile, currentBackRank)))
                    {
                        validMoves.Clear();
                        validMoves.AddRange(tmpKingMoves);
                        return false;
                    }
                }
            }

            tmpKingMoves.Add((startingFile + 1, currentBackRank)); // add the square to enable O_O or O_O_O
            validMoves.Clear();
            validMoves.AddRange(tmpKingMoves);
            return true;
        }

        private bool IsPawnOneRankFromPromoting(Piece selectedPawn)
        {
            return (selectedPawn.Y - 1 == (int)Ranks.Eighth && selectedPawn.Color == PieceColor.White) ||
                   (selectedPawn.Y + 1 == (int)Ranks.First && selectedPawn.Color == PieceColor.Black);
        }

        private bool IsPieceNearKingProtected(Piece king, (int x, int y) pieceNearKingPos)
        {
            foreach (var piece in chessBoard)
            {
                if (piece == null ||
                    (piece.X == pieceNearKingPos.x && piece.Y == pieceNearKingPos.y) ||
                    piece.Color == king.Color)
                    continue;

                validMoves = chessBoard.CalculateMoves(piece);

                if (IsSquareInList(validMoves, pieceNearKingPos)) return true;
            }

            return false;
        }

        private bool IsDraw()
        {
            int oppositeColor = (turn + 1) % 2;

            foreach (var piece in chessBoard)
            {
                if (piece == null || piece.Color == currentPlayer[turn]) continue;

                validMoves = chessBoard.CalculateMoves(piece);
                RemoveMovesThatTargetPiecesOfSameColor(ref validMoves, currentPlayer[oppositeColor]);

                if (piece.Name == 'K')
                {
                    RemoveKingSquaresCoveredByPieces(piece);
                    CheckPiecesNearKing(piece);
                }

                if (validMoves.Count > 0) return false;
            }

            return true;
        }

        private bool HasPieceGivenCheck((int x, int y) opponentKingPos, (int x, int y) destinationSquare)
        {
            var lastPieceMoved = chessBoard[destinationSquare.y, destinationSquare.x];

            if (selectedPiece!.Name == 'P' || selectedPiece.Name == 'N')
                validMoves = chessBoard.CalculateMoves(lastPieceMoved);

            else
            {
                var direction = DefineDirectionTowardsKing(selectedPiece.Name, opponentKingPos, destinationSquare);
                validMoves = chessBoard.CalculateMoves(chessBoard[destinationSquare.y, destinationSquare.x], direction);
            }

            validMoves.Add((destinationSquare.x, destinationSquare.y));  // piece that gives check can also be captured (neccessary for Knight and Pawn)
            isCheck = IsCheck(opponentKingPos);
            Debug.WriteLine($"\n\n******* IS CHECK = {isCheck} *******\n");
            return isCheck;
        }

        private bool IsCheck(ValueTuple<int, int> kingPosition)
        {
            if (IsSquareInList(validMoves, kingPosition)) return true; // maybe it's better to check the last move of the list because it should be the king position.
            
            return false;
        }

        private bool IsCheckmate()
        {
            return !validKingMoves.Any() && piecesAbleToStopCheck.Count == 0; // maybe incorrect
        }

        private static bool IsSquareInList(List<ValueTuple<int, int>>? list, ValueTuple<int, int> destinationSquare)
        {
            return list!.Exists(square => square == destinationSquare);
        }

        private bool IsPieceSameColorAsPlayer((int x, int y) destinationSquare)
        {
            return chessBoard[destinationSquare.y, destinationSquare.x].Color == currentPlayer[turn];
        }
        public bool IsPieceClicked((int x, int y) destSquare)
        {
            return !chessBoard.IsSquareNull(destSquare) && IsPieceSameColorAsPlayer(destSquare);
        }



        private static void RemoveSquaresFromList(List<ValueTuple<int, int>> list, ValueTuple<int, int> destinationSquare)
        {
            list.Remove(destinationSquare);
        }

        private void RemoveMovesThatTargetPiecesOfSameColor(ref List<(int x, int y)> moves, PieceColor color)
        {
            moves = moves.Where(move => chessBoard.IsSquareNull(move) || chessBoard[move].Color != color).ToList();
        }

        private static void ClearDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            dictionary.Clear();
        }



        protected virtual void OnCastle(CastleEventArgs e)
        {
            Castle?.Invoke(this, e);
        }

        protected virtual void OnGameFinish(GameFinishEventArgs e)
        {
            GameFinish?.Invoke(this, e);
        }

        protected virtual void OnPieceMovement(UIPieceMovementEventArgs e1, UIClockTickEventArgs e2)
        {
            UIPieceMovement?.Invoke(this, e1);
            UIClockTick?.Invoke(this, e2);
        }
    }
}
