using ChessLogic;
using WindowHelper;

namespace ChessUI
{
    partial class PromotionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Form2
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(68, 280);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form2";
            RightToLeft = RightToLeft.No;
            ShowIcon = false;
            BackColor = Color.Black;
            ResumeLayout(false);
        }


        private void InitializePromotionForm()
        {
            // this.StartPosition = FormStartPosition.CenterScreen; --> make the form spawn under or above the cell.
            this.MinimumSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.MaximumSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.Size = new Size(FORM_WIDTH, FORM_HEIGHT);
        }


        private void InitializePromotionFormComponents(PieceColor color, char notation, int counter)
        {
            Bitmap resizedImage = PieceImages.GetPieceImage(color, notation);

            Button button = new()
            {
                Width = BUTTON_WIDTH,
                Height = BUTTON_HEIGHT,
                Left = (FORM_WIDTH - BUTTON_WIDTH) / 2,
                Top = counter,
                Name = notation.ToString(),
                BackColor = Color.Ivory,
                Padding = new Padding(0),
                Margin = new Padding(0),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackgroundImage = resizedImage,
                BackgroundImageLayout = ImageLayout.Zoom
            };

            button.Click += Piece_Promote;
            this.Controls.Add(button);
        }

        #endregion
        
        private const int FORM_WIDTH  = 68;
        private const int FORM_HEIGHT = 260;
    }
}