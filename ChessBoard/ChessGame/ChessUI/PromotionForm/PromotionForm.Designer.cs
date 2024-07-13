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
            // PromotionForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.Black;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(68, 280);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PromotionForm";
            RightToLeft = RightToLeft.No;
            ShowIcon = false;
            ResumeLayout(false);
        }

        private void InitializePromotionForm(Point buttonScreenPos)
        {
            this.Padding = new Padding(0);
            this.MinimumSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.MaximumSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.Size = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(buttonScreenPos.X, buttonScreenPos.Y);
        }
        
        private void InitializeGrid()
        {
            grid.Dock = DockStyle.Fill;

            grid.ColumnCount = 1;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            grid.RowCount = NUMBER_OF_PROMOTABLE_PIECES;
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            this.Controls.Add(grid);
        }


        private void InitializePromotionFormButtons(PieceColor color, char notation, int buttonX, int buttonY)
        {
            Bitmap buttonImage = PieceImages.GetPieceImage(color, notation);

            Button button = new()
            {
                // Width = BUTTON_SIZE,
                // Height = BUTTON_SIZE,
                // Location = new Point(0, 0),
                Dock = DockStyle.Fill,
                Name = notation.ToString(),
                BackColor = Color.Ivory,
                Padding = new Padding(0),
                Margin = new Padding(0),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackgroundImage = buttonImage,
                BackgroundImageLayout = ImageLayout.Zoom,
            };

            button.Click += Piece_Promote;
            grid.Controls.Add(button, buttonX, buttonY);
        }

        #endregion
        
        private const int FORM_WIDTH  = BUTTON_SIZE;
        private const int FORM_HEIGHT = BUTTON_SIZE * NUMBER_OF_PROMOTABLE_PIECES;
        private TableLayoutPanel grid = new();
    }
}