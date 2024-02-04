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
    public partial class Form2 : Form
    {
        public string PieceName { get; set; }

        private const int buttonWidth = ChessBoardForm.squareSize, buttonHeight = ChessBoardForm.squareSize;

        private readonly List<string> promotionPiecesName = new() { "Q", "R", "B", "N" };


        public Form2(int turn)
        {
            InitializeComponent();

            string pieceTypeDir = (turn == 0) ? "white" : "black";

            int counter = 0;

            foreach (var buttonName in promotionPiecesName)
            {
                InitializePromotionForm(pieceTypeDir, buttonName, counter);
                counter += buttonHeight;
            }
        }



        private void Piece_Promote(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            PieceName = button.Name;
            this.Close();
        }
    }
}
