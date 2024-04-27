namespace WinFormsApp1
{
    partial class MainMenu
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

        private int buttonWidth = 100;
        private int buttonHeight = 60;

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 261);
            Name = "MainMenu";
            Text = "MainMenu";
            ResumeLayout(false);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.RosyBrown;
            ForeColor = Color.White;
            Font = new Font("Arial", 12, FontStyle.Bold);
        }

        private void InitializeMainMenuButtons()
        {
            int formWidth = ClientSize.Width;
            int formHeight = ClientSize.Height;

            Button buttonNewGame = new()
            {
                Text = "New Game",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = (formWidth - buttonWidth) / 2,
                Top = (formHeight - buttonHeight) / 2 - 70,

                BackColor = Color.PaleGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            Button buttonExit = new()
            {
                Text = "Exit",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = (formWidth - buttonWidth) / 2,
                Top = (formHeight - buttonHeight) / 2 + 70,

                BackColor = Color.PaleGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            Button buttonConnect = new()
            {
                Text = "Connect",
                Width = buttonWidth - 20,
                Height = buttonHeight - 20,
                Left = formWidth - 90,
                Top = formHeight - 50,

                BackColor = Color.PaleGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            buttonNewGame.Click += Create_ChessBoard;
            buttonExit.Click += Button_Exit;
            buttonExit.Click += Button_ConnectBluetooth;

            Controls.Add(buttonNewGame);
            Controls.Add(buttonExit);
            Controls.Add(buttonConnect);
        }

        #endregion
    }
}