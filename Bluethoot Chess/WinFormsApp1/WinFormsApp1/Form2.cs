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

            this.BackColor = Color.Black;

            string pieceTypeDir;

            pieceTypeDir = (turn == 0) ? "white" : "black";

            int counter = 0;

            foreach (var buttonName in promotionPiecesName)
            {
                InitializePromotionForm(pieceTypeDir, buttonName, counter);
                counter += buttonHeight;
            }
        }

        private void InitializePromotionForm(string pieceTypeDir, string buttonName, int counter)
        {
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            Bitmap resizedImage = ChessBoardForm.SetImageToButton(new CPiece(0, 0, buttonName, pieceTypeDir));

            Button button = new()
            {
                Width = buttonWidth, Height = buttonHeight, 
                Left = (formWidth - buttonWidth) / 2, Top = counter, Name = buttonName, 
                BackColor = Color.Ivory, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 1 }, 
                BackgroundImage = resizedImage, BackgroundImageLayout = ImageLayout.Zoom
            };

            button.Click += Piece_Promote;
            this.Controls.Add(button);
        }

        private void Piece_Promote(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            PieceName = button.Name;
            this.Close();
        }
    }
}
