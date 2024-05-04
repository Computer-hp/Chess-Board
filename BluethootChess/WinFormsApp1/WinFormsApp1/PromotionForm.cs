using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class PromotionForm : Form
    {
        public string Name { get; set; }

        private const int BUTTON_WIDTH = ChessBoardForm.SQUARE_SIZE;
        private const int BUTTON_HEIGHT = ChessBoardForm.SQUARE_SIZE;

        private static readonly string[] promotionPiecesName = { "Q", "R", "B", "N" };  // better to use a char[]


        public PromotionForm(int turn)
        {
            InitializeComponent();

            PieceColor pieceTypeDir = (turn == 0) ? PieceColor.White : PieceColor.Black;

            int counter = 0;

            foreach (var buttonName in promotionPiecesName)
            {
                InitializePromotionForm(pieceTypeDir, buttonName, counter);
                counter += BUTTON_HEIGHT;
            }
        }



        private void Piece_Promote(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Name = button.Name;
            this.Close();
        }
    }
}
