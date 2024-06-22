// For the pin i was thinking about adding a new property (bool) to the
// class Piece. After a piece is moved i have to control wheather
// the piece has pinned an opponents piece.

namespace ChessLogic
{
    public class Piece
    {
        public int X {  get; set; }
        public int Y { get; set; }
        public char Name { get; }
        public PieceColor Color { get; }


        public Piece(int x, int y, char name, PieceColor color)
        {
            X = x;
            Y = y;
            Name = name;
            Color = color;
        }
    }
}
