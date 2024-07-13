using ChessLogic;

namespace ChessUI
{
    public partial class PromotionForm : Form
    {
        private const int NUMBER_OF_PROMOTABLE_PIECES = 4;
        private const int BUTTON_SIZE = ChessBoardForm.SQUARE_SIZE;

        private static readonly char[] promotionPiecesName = { 'Q', 'R', 'B', 'N' };

        public char PromotedPieceName { get; set; }


        public PromotionForm(int turn, Point buttonScreenPos)
        {
            InitializeComponent();

            if (turn == 0) buttonScreenPos.Y -= FORM_HEIGHT + 2; // (0, 0) point of the 'screen' is in the top left corner.

            InitializePromotionForm(buttonScreenPos);
            InitializeGrid();
            PieceColor color = (turn == 0) ? PieceColor.White : PieceColor.Black;

            for (int j = 0; j < NUMBER_OF_PROMOTABLE_PIECES; ++j)
                InitializePromotionFormButtons(color, promotionPiecesName[j], 0, j);
        }


        private void Piece_Promote(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            PromotedPieceName = button.Name[0];
            this.Close();
        }
    }
}
