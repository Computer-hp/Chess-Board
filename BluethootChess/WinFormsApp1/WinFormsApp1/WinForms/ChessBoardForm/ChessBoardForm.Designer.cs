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

        private const int formWidth = 800;
        private const int formHeight = 600;
        
        public const int SQUARE_SIZE = 60;
        private const int TIMER_WIDTH = 100;
        private const int TIMER_HEIGHT = 50;

        private TableLayoutPanel boardGrid;
        private Label[] timerLabel = new Label[2];
        private Timer[] timer = new Timer[2];


        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChessBoardForm));
            SuspendLayout();
            // 
            // ChessBoardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(formWidth, formHeight);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ChessBoardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chess Game";
            Load += Form1_Load;
            ResumeLayout(false);

            InitializeBoardGrid();
            InitializeButtons();
        }


        private void InitializeBoardGrid()
        {
            boardGrid = new TableLayoutPanel
            {
                RowCount = BOARD_SIZE,
                ColumnCount = BOARD_SIZE,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Location = new Point(100, 100),
                Size = new Size(BOARD_SIZE * SQUARE_SIZE + 5, BOARD_SIZE * SQUARE_SIZE + 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left,
            };

            for (int i = 0; i < BOARD_SIZE; i++)
                boardGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            for (int j = 0; j < BOARD_SIZE; j++)
                boardGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            this.Controls.Add(boardGrid);
        }


        private void InitializeButtons()
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
                        Tag = (col, row),
                    };

                    var pieceAttributes = GetPieceNotation(col, row);

                    square.BackgroundImage = (pieceAttributes is not null)
                                                ? PieceImages.GetPieceImage(pieceAttributes.Value.color, pieceAttributes.Value.notation)
                                                : null;

                    square.Click += Button_Click;
                    // this.Controls.Add(square);
                    
                    boardGrid.Controls.Add(square, col, row);
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


        private void InitializeTimerLabels()
        {
            timerLabel[0] = new Label
            {
                Text = "White: 00:00:00",
                Font = new Font("Arial", 18),
                // Location = new Point(centerX + BOARD_SIZE * SQUARE_SIZE - 10, ClientSize.Height - SQUARE_SIZE / 2 - 10),
                AutoSize = true
            };

            timerLabel[1] = new Label
            {
                Text = "Black: 00:00:00",
                Font = new Font("Arial", 18),
                // Location = new Point(centerX + BOARD_SIZE * SQUARE_SIZE - 10, centerY),
                AutoSize = true
            };

            Controls.Add(timerLabel[0]);
            Controls.Add(timerLabel[1]);
        }


        private void InitializeTimers()
        {
            for (int i = 0; i < N_PLAYERS; i++)
            {
                timer[i] = new Timer { Interval = 100 };
                timer[i].Tick += Timer_Tick;
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

        #endregion
    }
}