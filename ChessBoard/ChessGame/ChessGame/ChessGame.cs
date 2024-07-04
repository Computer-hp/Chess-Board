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

        private List<(int x, int y)> validMoves = new();
        // private List<(int x, int y)> copyMoves = new();
        private Dictionary<(int x, int y), List<(int validX, int validY)>> piecesAbleToStopCheck = new();

        public const int SQUARE_SIZE = 70;

        public bool IsLastClickedButtonNull          = true;
        public bool HasLastClickedButtonChangedColor = true;

        public event EventHandler<CastleEventArgs>?           Castle;
        public event EventHandler<GameFinishEventArgs>?       GameFinish;
        public event EventHandler<UIPieceMovementEventArgs>?  UIPieceMovement;
        public event EventHandler<UIClockTickEventArgs>?      UIClockTick;


        public Game()
        {
            chessBoard = new ChessBoard();

            whiteAndBlackKingPos[0] = ((int)Files.eFile, (int)Ranks.FirstRank);
            whiteAndBlackKingPos[1] = ((int)Files.eFile, (int)Ranks.EighthRank);

            chessBoard.InitializePieces();
        }


        public bool IsPieceClicked((int x, int y) destSquare)
        {
            return !chessBoard.IsSquareNull(destSquare) && IsPieceSameColorAsPlayer(destSquare);
        }


        public void ManageUIClickedButton((int x, int y) destinationSquare)
        {
            var clickedSquare = chessBoard[destinationSquare];

            if (!PieceGotClicked(clickedSquare) && selectedPiece is null) return;

            // there was ManageClickedButton to eventualy change the color of the button.

            int firstRank = (int)Ranks.FirstRank;
            currentBackRank = Math.Abs((int)currentPlayer[turn] * firstRank - firstRank);

            if (selectedPiece is null)
            {
                ManageSelectedPiece(destinationSquare);
                IsLastClickedButtonNull = false;
                HasLastClickedButtonChangedColor = false;
                return;
            }

        // when an illegal move is played the stuff under 'condition' is not executed !!!
        // ###
            if (!IsMoveLegal(destinationSquare) ||
                (isCheck && !IsMoveLegalWhenCheck(destinationSquare)))
            { 
                selectedPiece = null;
                IsLastClickedButtonNull = true;
                HasLastClickedButtonChangedColor = false;
                return; 
            }


        // ###
        // create a method that is called something like ManagePieceAfterMoveIsLegal

            ValueTuple<int, int> originSquare = (selectedPiece.X, selectedPiece.Y);

                                // selectedPiece --> Tuple called originSquare
            ManagePieceMovement(originSquare, destinationSquare);

        // ###
        // then this method is fine

            ManageSituationAfterPieceMovement(destinationSquare);

            int oppositeClock = (turn + 1) % 2;

            if (!isCheck && IsDraw()) OnGameFinish(new GameFinishEventArgs(oppositeClock, "Draw"));
           

        // #####
        // final things to do

            chessBoard.MovePawnTowardsBlackOrWhite = -1 * (turn * 2 - 1);
            // validMoves.Clear(); // not neccesary

            HasLastClickedButtonChangedColor = true;
            IsLastClickedButtonNull = true;

            turn = (turn + 1) % 2;
            selectedPiece = null;
            ChessBoard.PrintMovesThatCanStopCheck(piecesAbleToStopCheck);

        // #####
        }
        

        private bool PieceGotClicked(Piece clickedSquare)
        {
            return clickedSquare is not null && 
                   clickedSquare.Color == currentPlayer[turn];
        }
        

        private void ManageSelectedPiece((int x, int y) piecePos)
        {
            selectedPiece = chessBoard[piecePos];
            validMoves = chessBoard.CalculateMoves(selectedPiece);

            Debug.WriteLine($"{selectedPiece.Name}, {selectedPiece.Color}");
            Debug.WriteLine(ChessBoard.PrintMoves(validMoves) + "\n");

            if (selectedPiece.Name == 'P') 
            { 
                ManageInvalidDiagonalPawnMoves(selectedPiece); 
                return; 
            }

            if (selectedPiece.Name != 'K') return;

            RemoveKingSquaresCoveredByPieces(selectedPiece);
            CheckPiecesNearKing(selectedPiece);
            
            if (!O_O_O[turn])  
                O_O_O[turn] = IsCastleLegal((int)Files.bFile, (int)Files.eFile, isRook_A_FirstMove[turn]);

            if (!O_O[turn])
                O_O[turn] = IsCastleLegal((int)Files.fFile, (int)Files.hFile, isRook_H_FirstMove[turn]);

            Debug.WriteLine("king moves:");
            Debug.WriteLine(ChessBoard.PrintMoves(validMoves) + "\n");
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
            //copyMoves.Clear();
            var tmpMoves = new List<(int x, int y)>(); // contains moves of the last clicked piece.

            if (selectedPiece!.Name == 'K') tmpMoves.AddRange(validMoves); // have to remove the moves that are illegal.

            else if (piecesAbleToStopCheck.ContainsKey((selectedPiece.X, selectedPiece.Y)))
            {
                List<ValueTuple<int, int>> movesToStopCheck = piecesAbleToStopCheck[(selectedPiece.X, selectedPiece.Y)];

                if (IsSquareInList(movesToStopCheck, destinationSquare))
                    tmpMoves.AddRange(piecesAbleToStopCheck[(selectedPiece.X, selectedPiece.Y)]);
            }

            if (!tmpMoves.Exists(square => IsSquareInList(validMoves, square))) return false;

            ClearDictionary(piecesAbleToStopCheck);
            
            isCheck = false;
            return true;
        }
        

        private void ManagePieceMovement((int x, int y) originSquare, (int x, int y) destinationSquare)
        {
            if (selectedPiece!.Name == 'P') ManagePawnPromotion(selectedPiece, destinationSquare);

            chessBoard[originSquare.y, originSquare.x] = null;
            chessBoard[destinationSquare.y, destinationSquare.x] = new Piece(destinationSquare.x, destinationSquare.y, 
                                                                             selectedPiece.Name, selectedPiece.Color);

            if (selectedPiece.Name == 'K')
            {
                if (!isKingFirstMove[turn]) ManageFirstKingMove(destinationSquare);

                whiteAndBlackKingPos[turn] = destinationSquare;
            }
            else if (selectedPiece.Name == 'R') ManageFirstRookMove(originSquare);

            OnPieceMovement(new UIPieceMovementEventArgs(destinationSquare, currentPlayer[turn], selectedPiece.Name), 
                            new UIClockTickEventArgs(turn));
        }


        private void ManageSituationAfterPieceMovement((int x, int y) destinationSquare)
        {
            var lastMovedPiece = chessBoard[destinationSquare];

            if (lastMovedPiece.Name == 'K') return;

            int oppositeKingIdx = (turn + 1) % 2;
            Piece opponentKing = chessBoard[whiteAndBlackKingPos[oppositeKingIdx]];

            if (!HasPieceGivenCheck((opponentKing.X, opponentKing.Y), destinationSquare)) return;

            // keeps only position of the piece, removes all other moves
            if (lastMovedPiece.Name == 'P' || lastMovedPiece.Name == 'N')
                validMoves.RemoveAll(square => square.x != lastMovedPiece.X && square.y != lastMovedPiece.Y);

            ManageSituationAfterCheck(opponentKing, lastMovedPiece);

            if (!IsCheckmate()) return;

            int oppositeClock = (turn + 1) % 2;
            OnGameFinish(new GameFinishEventArgs(oppositeClock, currentPlayer[turn].ToString()));
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
            // chessBoard.ValidMoves.Clear(); // maybe is not neccesary
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
            if (IsSquareInList(validMoves, kingPosition)) return true;
            
            return false;
        }


        private void ManageSituationAfterCheck(Piece opponentKing, Piece lastMovedPiece)
        {
            var squaresBetweenKingAndThePieceThatGaveCheck = new List<(int x, int y)>();
            squaresBetweenKingAndThePieceThatGaveCheck.AddRange(validMoves);

            //copyMoves.Clear();
            //copyMoves.AddRange(validMoves);

            foreach (var piece in chessBoard)
            {
                if (piece == null ||
                    piece.Name == 'K' ||
                    piece.Color == selectedPiece!.Color)
                    continue;
                
                validMoves = chessBoard.CalculateMoves(piece);

                if (piece.Name == 'P') ManageInvalidDiagonalPawnMoves(piece);

                RemoveSquaresFromList(validMoves, (opponentKing.X, opponentKing.Y));
                CheckIfPieceIsAbleToStopCheck(squaresBetweenKingAndThePieceThatGaveCheck, piece);
            }

            validMoves = chessBoard.CalculateMoves(opponentKing); // this is done to check later if king is able to move
            validMoves.RemoveAll(kingMove => kingMove.x == lastMovedPiece.X || kingMove.y == lastMovedPiece.Y);

            int sign = 0;

            if ((lastMovedPiece.X > opponentKing.X && lastMovedPiece.Y > opponentKing.Y) ||
                (lastMovedPiece.X < opponentKing.X && lastMovedPiece.Y < lastMovedPiece.Y))
                sign = -1;

            else if ((lastMovedPiece.X > opponentKing.X && lastMovedPiece.Y < opponentKing.Y) ||
                     (lastMovedPiece.X < opponentKing.X && lastMovedPiece.Y > lastMovedPiece.Y))
                sign = 1;


            for (int i = validMoves.Count - 1, movesToRemove = 2; i >= 0 && movesToRemove > 0; i--)
                if (Math.Abs(validMoves[i].x + sign * validMoves[i].y) == Math.Abs(lastMovedPiece.X + sign * lastMovedPiece.Y))
                {
                    validMoves.RemoveAt(i);
                    movesToRemove--;
                }

            RemoveKingSquaresCoveredByPieces(opponentKing);

            CheckPiecesNearKing(opponentKing);
        }


        private bool IsCheckmate()
        {
            return !validMoves.Any() && piecesAbleToStopCheck.Count == 0; // maybe incorrect
        }


        private Directions? DefineDirectionTowardsKing(char pieceName, (int, int) opponentKingPos, (int x, int y) destinationSquare)
        {
            Directions? direction = null;

            if (PieceMovesStraight(pieceName))
                direction = FindStraightDirection(opponentKingPos, destinationSquare);

            Debug.Write($"\n'direction' = {direction}\n");

            //if (direction is not null)
                //validMoves = chessBoard.CalculateMoves(chessBoard[destinationSquare.y, destinationSquare.x], direction);

            if (pieceName == 'R' || direction is not null) return direction;

            if (PieceMovesDiagonally(pieceName))
                direction = FindDiagonalDirection(opponentKingPos, destinationSquare);

            Debug.Write($"\n'direction' = {direction}\n");
            return direction;

            //if (direction is not null)
                //validMoves = chessBoard.CalculateMoves(chessBoard[destinationSquare.y, destinationSquare.x], direction);
        }


        private static bool PieceMovesStraight(char pieceName)
        {
            return Array.Exists(piecesThatMoveStraight, pieceNotation => pieceNotation == pieceName);
        }

        private static bool PieceMovesDiagonally(char pieceName)
        {
            return Array.Exists(piecesThatMoveDiagonally, pieceNotation => pieceNotation == pieceName);
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


        private bool IsCastleLegal(int startingFile, int endingFile, bool isRookFirstMove)
        {
            if (isKingFirstMove[turn] || isRookFirstMove) return false;

            var king = selectedPiece;

            List<ValueTuple<int, int>> tmpKingMoves = new();
            tmpKingMoves.AddRange(validMoves);

            for (int tmpStartingFile = startingFile; tmpStartingFile < endingFile; tmpStartingFile++)
            {
                if (!chessBoard.IsSquareNull((tmpStartingFile, currentBackRank))) return false; // squares are never outside the board!

                if (tmpStartingFile == (int)Files.bFile) continue;

                // control if squares from starting to ending File are into opponents piece moves ('create a method for this')
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

                // to remove straight moves of the PAWN, by one square and/or two squares in case the pawn hasn't been moved yet
                if (piece.Name == 'P')
                {
                    RemoveSquaresFromList(validMoves, (piece.X, piece.Y - chessBoard.MovePawnTowardsBlackOrWhite));
                    RemoveSquaresFromList(validMoves, (piece.X, piece.Y - chessBoard.MovePawnTowardsBlackOrWhite * 2));
                }

                tmpKingMoves.RemoveAll(square => IsSquareInList(validMoves, square)); // strange but ok
            }
            
            validMoves.Clear();  // if this is removed than ValidMoves will contain the moves of the piece that protects the one that gave check
            validMoves.AddRange(tmpKingMoves);
        }


        /*
            Checks each square next to the king, 8 squares in total,
            and removes squares where king is not able to move.
        */

        private void CheckPiecesNearKing(Piece king)
        {
            RemoveMovesThatTargetPiecesOfSameColor(ref validMoves, king.Color);

            foreach (var square in validMoves)
                if (!chessBoard.IsSquareNull(square))
                    RemoveIllegalKingCaptures(king, square);


            void RemoveIllegalKingCaptures(Piece king, (int x, int y) pieceNearKingPos)
            {
                List<ValueTuple<int, int>> tmpKingMoves = new();
                tmpKingMoves.AddRange(validMoves);

                // controls whether the piece near king is protected by another piece,
                // so the king can't capture it

                if (IsPieceNearKingProtected(pieceNearKingPos))
                    RemoveSquaresFromList(tmpKingMoves, pieceNearKingPos);

                validMoves.Clear();
                validMoves.AddRange(tmpKingMoves);


                bool IsPieceNearKingProtected((int x, int y) pieceNearKingPos)
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
            }
        }



        private void CheckIfPieceIsAbleToStopCheck(List<(int x, int y)> criticalSquares, Piece piece)
        {
            List<ValueTuple<int, int>> tmpMoves = new();

            /*foreach (var square in validMoves)
                if (IsSquareInList(copyMoves, square))
                    tmpMoves.Add(square);*/

            foreach (var square in validMoves)
                if (IsSquareInList(criticalSquares, square))
                    tmpMoves.Add(square);

            if (!tmpMoves.Any()) return;

            ValueTuple<int, int> key = (piece.X, piece.Y);
            piecesAbleToStopCheck[key] = tmpMoves;
        }


        private void ManageFirstKingMove((int x, int y) kingDestSquare)
        {
            isKingFirstMove[turn] = true;

            if (O_O[turn] && 
                kingDestSquare.x == (int)Files.gFile && 
                kingDestSquare.y == currentBackRank)
            {
                ManageShortOrLongCastle((int)Files.hFile);
                OnCastle(new CastleEventArgs(kingDestSquare, ((int)Files.fFile, currentBackRank), currentPlayer[turn]));
            }

            else if (O_O_O[turn] && 
                     kingDestSquare.x == (int)Files.cFile && 
                     kingDestSquare.y == currentBackRank)
            {

                ManageShortOrLongCastle((int)Files.aFile);
                OnCastle(new CastleEventArgs(kingDestSquare, ((int)Files.dFile, currentBackRank), currentPlayer[turn]));  // repetition !!!
            }
        }


        private void ManageShortOrLongCastle(int previousRookX)
        {
            ManageFirstRookMove((previousRookX, currentBackRank));

            var tmpRook = chessBoard[currentBackRank, previousRookX];  // copies the rook
            chessBoard[currentBackRank, previousRookX] = null;

            //transposes the rook
            tmpRook.X = (previousRookX == (int)Files.aFile) ? (int)Files.dFile : (int)Files.fFile;
            Debug.WriteLine($"\t\tRook moved to y, x: {tmpRook.Y}, {tmpRook.X}\n");
            chessBoard[currentBackRank, tmpRook.X] = tmpRook;
        }



        // problem: if i return back the rook it probably cocount's as not yet moved
        private void ManageFirstRookMove((int x, int y ) rookPosition)
        {
            if (rookPosition.x == (int)Files.aFile && rookPosition.y == currentBackRank)
                isRook_A_FirstMove[turn] = true;

            if (rookPosition.x == (int)Files.hFile && rookPosition.y == currentBackRank)
                isRook_H_FirstMove[turn] = true;
        }

        
        private void ManageInvalidDiagonalPawnMoves(Piece pawn)
        {
            //int movePawnTowardBlackOrWhite = (pawn.Color == PieceColor.White) ? -1 : 1;

            //RemoveInvalidDiagonalPawnMoves((pawn.X + 1, pawn.Y + movePawnTowardBlackOrWhite));
            //RemoveInvalidDiagonalPawnMoves((pawn.X - 1, pawn.Y + movePawnTowardBlackOrWhite));

            validMoves.RemoveAll(square => square.x != pawn.X && 
                                 (chessBoard.IsSquareNull(square) || chessBoard[square].Name == 'K'));
        }


        /*private void RemoveInvalidDiagonalPawnMoves((int x, int y) destinationSquare)
        {
            if (!ChessBoard.IsSquareOutsideTheBoard(destinationSquare) &&
                (chessBoard.IsSquareNull(destinationSquare) ||
                 chessBoard[destinationSquare.y, destinationSquare.x].Name == 'K'))

                RemoveSquaresFromList(validMoves, destinationSquare);
        }*/


        private void ManagePawnPromotion(Piece selectedPawn, (int x, int y) destinationSquare)
        {
            if (!IsPawnOneRankFromPromoting(selectedPawn)) return;

            var promotion = new PromotionForm(turn);
            promotion.ShowDialog();

            selectedPiece = new Piece(destinationSquare.x, destinationSquare.y, promotion.PromotedPieceName, selectedPawn.Color);
            Debug.Write($"\nPromotion: {promotion.Name}\n");
        }
        

        private static bool IsPawnOneRankFromPromoting(Piece selectedPawn)
        {
            return (selectedPawn.Y - 1 == (int)Ranks.EighthRank && selectedPawn.Color == PieceColor.White) ||
                   (selectedPawn.Y + 1 == (int)Ranks.FirstRank && selectedPawn.Color == PieceColor.Black);
        }



        private static void RemoveSquaresFromList(List<ValueTuple<int, int>>? list, ValueTuple<int, int> destinationSquare)
        {
            list!.Remove(destinationSquare);
        }


        private static bool IsSquareInList(List<ValueTuple<int, int>>? list, ValueTuple<int, int> destinationSquare)
        {
            return list!.Exists(square => square == destinationSquare);
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


        private void RemoveMovesThatTargetPiecesOfSameColor(ref List<(int x, int y)> moves, PieceColor color)
        {
            moves = moves.Where(move => chessBoard.IsSquareNull(move) || chessBoard[move].Color != color).ToList();
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
