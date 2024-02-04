namespace WinFormsApp1
{
    partial class Form2
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



        private void InitializePromotionForm(string pieceTypeDir, string buttonName, int counter)
        {
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            Bitmap resizedImage = ChessBoardForm.SetImageToButton(new CPiece(0, 0, buttonName, pieceTypeDir));

            Button button = new()
            {
                Width = buttonWidth,
                Height = buttonHeight,
                Left = (formWidth - buttonWidth) / 2,
                Top = counter,
                Name = buttonName,
                BackColor = Color.Ivory,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1 },
                BackgroundImage = resizedImage,
                BackgroundImageLayout = ImageLayout.Zoom
            };

            button.Click += Piece_Promote;
            this.Controls.Add(button);
        }

        #endregion
    }
}