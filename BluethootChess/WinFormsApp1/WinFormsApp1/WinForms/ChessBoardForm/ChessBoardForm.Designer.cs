using Timer = System.Windows.Forms.Timer;
using ChessLogic;
using WindowHelper;

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
        private TableLayoutPanel tableLayoutPanel;


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
            Text = "Chess Game";
            Name = "ChessBoardForm";
            Load += Form1_Load;
            ResumeLayout(false);
            StartPosition = FormStartPosition.CenterScreen;

            centerX = (ClientSize.Width - BOARD_SIZE * SQUARE_SIZE) / 2 - 70;
            centerY = (ClientSize.Height - BOARD_SIZE * SQUARE_SIZE) / 2;

            InitializeTableLayoutPanel();
            InitializeChessBoardFormButtons();
            InitializeChessBoardFormTimers();
            InitializeTimers();
        }


        private void InitializeTableLayoutPanel()
        {
            /*
            tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.ColumnCount = BOARD_SIZE;
            tableLayoutPanel.RowCount = BOARD_SIZE + 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            
            for (int i = 0; i < BOARD_SIZE; i++)
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, SQUARE_SIZE));
            
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, TIMER_LABEL_HEIGHT));
            */

            tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            tableLayoutPanel.ColumnCount = BOARD_SIZE;
            tableLayoutPanel.RowCount = BOARD_SIZE;
            Controls.Add(tableLayoutPanel);
        }


        private void InitializeChessBoardFormButtons()
        {
            for (int row = 0; row < BOARD_SIZE; row++)

                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    Button square = new()
                    {
                        Size = new Size(SQUARE_SIZE, SQUARE_SIZE),
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        BackColor = (row + col) % 2 == 0 ? Color.Ivory : Color.Peru,
                        FlatStyle = FlatStyle.Flat,
                        FlatAppearance = { BorderSize = 0 },
                        BackgroundImageLayout = ImageLayout.Zoom,
                        Tag = (col, row)
                    };

                    var pieceAttributes = GetPieceNotation(col, row);

                    square.BackgroundImage = (pieceAttributes is not null) 
                                                ? PieceImages.GetPieceImage(pieceAttributes.Value.color, pieceAttributes.Value.notation)
                                                : null;

                    square.Click += Button_Click;
                    tableLayoutPanel.Controls.Add(square, col, row);
                }
        }


        private (PieceColor color, char notation)? GetPieceNotation(int col, int row)
        {
            switch (row)
            {
                case 1: return (PieceColor.Black, 'P');
                case 0: return (PieceColor.Black, ChessBoard.Pieces[col]);

                case 6: return (PieceColor.White, 'P');
                case 7: return (PieceColor.White, ChessBoard.Pieces[col]);

                default: return null;
            }
        }


        private void InitializeChessBoardFormTimers()
        {
            /*
            timerLabel[0] = new Label
            {
                Text = "White: 00:00:00",
                Font = new Font("Arial", 18),
                Location = new Point(centerX + BOARD_SIZE * SQUARE_SIZE - 10, ClientSize.Height - SQUARE_SIZE / 2 - 10),
                AutoSize = true
            };

            timerLabel[1] = new Label
            {
                Text = "Black: 00:00:00",
                Font = new Font("Arial", 18),
                Location = new Point(centerX + BOARD_SIZE * SQUARE_SIZE - 10, centerY),
                AutoSize = true
            };

            Controls.Add(timerLabel[0]);
            Controls.Add(timerLabel[1]);
            */
/*
            for (int i = 0; i < N_PLAYERS; i++)
            {
                timerLabel[i] = new Label
                {
                    Text = i == 0 ? "White: 00:00:00" : "Black: 00:00:00",
                    Font = new Font("Arial", 18),
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // Add timer labels to the TableLayoutPanel
                tableLayoutPanel.Controls.Add(timerLabel[i], i == 0 ? 0 : 1, BOARD_SIZE); // Place labels at bottom right corner
            }
*/
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsRestarted)
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
                IsClosed = true;
                this.Close();
            }
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            int turn = (int)timer.Tag;

            secondsElapsed[turn]++;

            TimeSpan time = TimeSpan.FromMilliseconds(secondsElapsed[turn] * timer.Interval);

            string player = (turn == 0) ? "White: " : "Black: ";

            string milliseconds = (time.Milliseconds / 10).ToString("D2");

            string timerText = string.Format(player + "{0:D2}:{1:D2}:{2}", 
                                                time.Minutes, time.Seconds, milliseconds);
            timerLabel[turn].Text = timerText;
        }

        #endregion
    }
}