using System.Collections;
using System.Diagnostics;


namespace ChessLogic
{
    public class ChessBoard : IEnumerable<Piece>
    {
        private const int BOARD_SIZE = 8;
        private const int NUMBER_OF_DIRECTIONS = 8;

        private Piece[,] board;

        public static readonly (Directions direction, (int x, int y) moveBy)[] linearDirections =
        {
            ( Directions.Left,      (-1, 0)  ),
            ( Directions.Right,     (1, 0)   ),
            ( Directions.Up,        (0, -1)  ),
            ( Directions.Down,      (0, 1)   ),

            ( Directions.LeftDown,  (-1, 1)  ),
            ( Directions.LeftUp,    (-1, -1) ),
            ( Directions.RightUp,   (1, -1)  ),
            ( Directions.RightDown, (1, 1)   ),
        };

        private static readonly (int x, int y)[] knightMoves = 
        {
            (2, 1), (2, -1), (-2, 1), (-2, -1),
            (1, 2), (1, -2), (-1, 2), (-1, -2)
        };


        public static readonly char[] Pieces = { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' };
        public int MovePawnTowardsBlackOrWhite { get; set; } = -1;


        public Piece this[int y, int x]
        {
            get 
            {
                if (IsSquareOutsideTheBoard((x, y))) throw new IndexOutOfRangeException();
                return board[y, x]; 
            }
            set 
            { 
                if (IsSquareOutsideTheBoard((x, y))) throw new IndexOutOfRangeException();
                board[y, x] = value; 
            }
        }

        public Piece this[(int x, int y) destSquare]
        {
            get 
            {
                if (IsSquareOutsideTheBoard((destSquare.x, destSquare.y))) throw new IndexOutOfRangeException();
                return board[destSquare.y, destSquare.x]; 
            }
            set 
            { 
                if (IsSquareOutsideTheBoard((destSquare.x, destSquare.y))) throw new IndexOutOfRangeException();
                board[destSquare.y, destSquare.x] = value; 
            }
        }


        public IEnumerator<Piece> GetEnumerator()
        {
            foreach (Piece piece in board)
                yield return piece;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /*
        public ref Piece GetPieceRef(int y, int x)
        {
            if (IsSquareOutsideTheBoard((x, y))) throw new IndexOutOfRangeException();

            return ref board[y, x];
        }
        */ 


        public ChessBoard()
        {
            board = new Piece[BOARD_SIZE, BOARD_SIZE];
        }


        /*
            Adds pieces to the board. 
        */

        public void InitializePieces()
        {
            int firstRank = (int)Ranks.First;
            int secondRank = (int)Ranks.Second;
            int seventhRank = (int)Ranks.Seventh;
            int eightRank = (int)Ranks.Eighth;

            for (int x = 0; x < BOARD_SIZE; x++)
            {
                board[secondRank, x] = new Piece(x, secondRank, 'P', PieceColor.White);
                board[firstRank, x] = new Piece(x, firstRank, Pieces[x], PieceColor.White);

                board[seventhRank, x] = new Piece(x, seventhRank, 'P', PieceColor.Black);
                board[eightRank, x] = new Piece(x, eightRank, Pieces[x], PieceColor.Black);
            }
        }


        public List<(int x, int y)> CalculateMoves(Piece piece, Directions? direction = null)
        {
            switch (piece.Name)
            {
                case 'P': return CalculatePawnMoves(piece);

                case 'N': return CalculateKnightMoves(piece);

                default: break;
            }

            return ManageLinearDirections(piece, direction);
        }


        /*
            Calculates the legal and illegal pawn moves.
        */

        // TODO   En passant

        private List<(int x, int y)> CalculatePawnMoves(Piece pawn)
        {
            int movePawnTowardsBlackOrWhite = (pawn.Color == PieceColor.White) ? -1 : 1;
            int destinationRank = pawn.Y + movePawnTowardsBlackOrWhite;

            if (IsSquareOutsideTheBoard((pawn.X, destinationRank))) 
                return new List<(int x, int y)>();

            List<(int x, int y)> validMoves = new();

            if (IsSquareNull((pawn.X, destinationRank))) validMoves.Add((pawn.X, destinationRank));

            for (int x = pawn.X - 1; x < pawn.X + 2; x += 2)
            {
                if (IsSquareOutsideTheBoard((x, destinationRank))) continue;
                validMoves.Add((x, destinationRank));
            }

            int destinationRankOfFirstPawnMove = pawn.Y + movePawnTowardsBlackOrWhite * 2;

            if (!IsPawnBeingMovedForTheFirstTime(pawn) ||
                IsSquareOutsideTheBoard((pawn.X, destinationRankOfFirstPawnMove)) ||
                !IsSquareNull((pawn.X, destinationRankOfFirstPawnMove))) 
                return validMoves;

            validMoves.Add((pawn.X, destinationRankOfFirstPawnMove));
            return validMoves;
        }


        private static bool IsPawnBeingMovedForTheFirstTime(Piece piece)
        {
            return ((piece.Color == PieceColor.White && piece.Y == (int)Ranks.Second) ||
                    (piece.Color == PieceColor.Black && piece.Y == (int)Ranks.Seventh));
        }



        private List<(int x, int y)> ManageLinearDirections(Piece piece, Directions? direction)
        {
            int times = (piece.Name == 'K') ? 1 : BOARD_SIZE;

            List<(int x, int y)> validMoves = new();

            if (direction != null)
            {
                ValueTuple<int, int> incrementForNextSquare = linearDirections.First(storedDirection => storedDirection.direction == direction).moveBy;
                CalculateLinearDirections(validMoves, piece, incrementForNextSquare, times, true);
                return validMoves;
            }

            int startingIndexForDirections = (piece.Name == 'R') ? 0 
                                             : (piece.Name == 'B') ? 4 : 0;

            int endingIndexForDirections = (piece.Name == 'R') ? NUMBER_OF_DIRECTIONS / 2 : NUMBER_OF_DIRECTIONS;

            for (int i = startingIndexForDirections; i < endingIndexForDirections; i++)
                CalculateLinearDirections(validMoves, piece, linearDirections[i].moveBy, times);

            return validMoves;
        }

        
        // Should add other parameters to be able to get all the moves to the king, so later i can control
        // if for (move in moves) --> (chessBoard[move] != null) counter++;
        // if (counter == 1), there is a piece between piece that gave check and king, so it's "pinned".
        // if (counter == 0), there are no pieces in between, so it's "check".

        private void CalculateLinearDirections(List<(int x, int y)> validMoves, Piece piece, (int x, int y) incrementForNextSquare, int times, bool searchingForKing = false)
        {
            int destinationX = piece.X, destinationY = piece.Y;

            for (int i = 0; i < times; i++)
            {
                destinationX += incrementForNextSquare.x;
                destinationY += incrementForNextSquare.y;

                if (IsSquareOutsideTheBoard((destinationX, destinationY))) return;

                if (searchingForKing)
                {
                    if (board[destinationY, destinationX] != null
                        && board[destinationY, destinationX].Name == 'K')
                    {
                        validMoves.Add((destinationX, destinationY));
                        return;
                    }
                }

                else if (!IsSquareNull((destinationX, destinationY)))
                {
                    validMoves.Add((destinationX, destinationY));
                    return;
                }

                validMoves.Add((destinationX, destinationY));
            }
        }


        private static List<(int x, int y)> CalculateKnightMoves(Piece piece)
        {
            List<(int x, int y)> validMoves = new();

            foreach (var (x, y) in knightMoves)
            {
                int newX = piece.X + x;
                int newY = piece.Y + y;

                if (!IsSquareOutsideTheBoard((newX, newY))) validMoves.Add((newX, newY));
            }

            return validMoves;
        }


        public static bool IsSquareOutsideTheBoard((int x, int y) destinationSquare)
        {
            return (destinationSquare.x < 0 || destinationSquare.x >= BOARD_SIZE ||
                    destinationSquare.y < 0 || destinationSquare.y >= BOARD_SIZE);
        }


        public bool IsSquareNull((int x, int y) destinationSquare)
        {
            return (board[destinationSquare.y, destinationSquare.x] == null);
        }


        public static string PrintMoves(List<(int x, int y)> validMoves)
        {
            string output = "";

            foreach (var (x, y) in validMoves)
                output += x + "," + y + " ";

            return output;
        }


        public static void PrintMovesThatCanStopCheck(Dictionary<(int x, int y), List<(int validX, int validY)>> stopCheckWithPiece)
        {
            Debug.Write("\nStopCheckWithPiece = \n");
            
            foreach (var kvp in stopCheckWithPiece)
            {
                Debug.Write($"Key: ({kvp.Key.x}, {kvp.Key.y}), Squares: ");

                foreach (var (validX, validY) in kvp.Value)
                    Debug.Write($"{validX}, {validY} ");

                Debug.Write('\n');
            }

            Debug.Write('\n');
        }
    }
}
