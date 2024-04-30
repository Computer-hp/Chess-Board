namespace WinFormsApp1
{
    partial class ChessBoardForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>

        private int centerX, centerY;
        private int chessBoardFormSize = BOARD_SIZE * SQUARE_SIZE;



        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChessBoardForm));
            SuspendLayout();
            // 
            // ChessBoardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(chessBoardFormSize + 200, chessBoardFormSize + 50);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ChessBoardForm";
            Load += Form1_Load;
            ResumeLayout(false);
            StartPosition = FormStartPosition.CenterScreen;

            centerX = (ClientSize.Width - BOARD_SIZE * SQUARE_SIZE) / 2 - 70;
            centerY = (ClientSize.Height - BOARD_SIZE * SQUARE_SIZE) / 2;
        }



        private void InitializeChessBoardFormButtons()
        {
            for (int row = 0; row < BOARD_SIZE; row++)

                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    Button square = new()
                    {
                        Size = new Size(SQUARE_SIZE, SQUARE_SIZE),
                        Location = new Point(centerX + col * SQUARE_SIZE, centerY + row * SQUARE_SIZE),
                        BackColor = (row + col) % 2 == 0 ? Color.Ivory : Color.Brown,
                        FlatStyle = FlatStyle.Flat,
                        FlatAppearance = { BorderSize = 0 },
                        BackgroundImageLayout = ImageLayout.Zoom,
                        Tag = (col, row)
                    };

                    if (chessBoard[row, col] != null)
                    {
                        Bitmap resizedImage = SetImageToButton(chessBoard[row, col]);
                        square.BackgroundImage = resizedImage;
                    }
                    else
                        square.BackgroundImage = null;

                    square.Click += Button_Click;

                    Controls.Add(square);
                }
        }



        private void InitializeChessBoardFormTimers()
        {
            timerLabel[0] = new Label
            {
                Text = "White: 00:00",
                Font = new Font("Arial", 18),
                Location = new Point(centerX + BOARD_SIZE * SQUARE_SIZE + 10, ClientSize.Height - SQUARE_SIZE / 2 - 10),
                AutoSize = true
            };

            timerLabel[1] = new Label
            {
                Text = "Black: 00:00",
                Font = new Font("Arial", 18),
                Location = new Point(centerX + BOARD_SIZE * SQUARE_SIZE + 10, centerY),
                AutoSize = true
            };

            Controls.Add(timerLabel[0]);
            Controls.Add(timerLabel[1]);
        }



        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (isRestarted)
            {
                this.FormClosing -= Form1_FormClosing;
                this.Close();
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
                e.Cancel = true;

            else if (result == DialogResult.Yes)
            {
                this.FormClosing -= Form1_FormClosing;
                isClosed = true;
                this.Close();
            }
        }



        private void Timer_Tick(object? sender, EventArgs e)
        {
            secondsElapsed[turn]++;

            TimeSpan time = TimeSpan.FromSeconds(secondsElapsed[turn]);

            string player = (turn == 0) ? "white: " : "black: ";

            string timerText = string.Format(player + "{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            timerLabel[turn].Text = timerText;
        }

        #endregion
    }
}