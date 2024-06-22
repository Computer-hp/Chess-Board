using Timer = System.Windows.Forms.Timer;
using ChessLogic;
using WindowHelper;

namespace ChessUI
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



        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChessBoardForm));
            outerPanel = new TableLayoutPanel();
            SuspendLayout();
            // 
            // outerPanel
            // 
            outerPanel.AutoSize = true;
            outerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            outerPanel.ColumnCount = 2;
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70.00F));
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30.00F));
            outerPanel.Dock = DockStyle.Fill;
            outerPanel.Location = new Point(0, 0);
            outerPanel.Name = "outerPanel";
            outerPanel.RowCount = 1;
            outerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            outerPanel.Size = new Size(800, 600);
            outerPanel.TabIndex = 0;
            // 
            // ChessBoardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            Controls.Add(outerPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ChessBoardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chess Game";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeOtherComponents()
        {
            InitializeBoardGrid();
            InitializeButtons();
            InitializeGridForTimers();
            InitializeTimerLabels();
            InitializeTimers();
        }


        private void InitializeBoardGrid()
        {
            boardGrid = new TableLayoutPanel
            {
                RowCount = BOARD_SIZE,
                ColumnCount = BOARD_SIZE,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Size = new Size(BOARD_SIZE * SQUARE_SIZE, BOARD_SIZE * SQUARE_SIZE),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, SQUARE_SIZE, 0, 0)
            };

            for (int i = 0; i < BOARD_SIZE; i++)
                boardGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            for (int j = 0; j < BOARD_SIZE; j++)
                boardGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            outerPanel.Controls.Add(boardGrid, 0, 0);
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

                    var pieceAttributes = GetPieceAttributes(col, row);

                    square.BackgroundImage = (pieceAttributes is not null)
                                                ? PieceImages.GetPieceImage(pieceAttributes.Value.color, pieceAttributes.Value.notation)
                                                : null;

                    square.Click += Button_Click;
                    boardGrid.Controls.Add(square, col, row);
                }
        }


        private (PieceColor color, char notation)? GetPieceAttributes(int col, int row)
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


        private void InitializeGridForTimers()
        {
            gridForTimers = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, SQUARE_SIZE, 0, 0),
                Dock = DockStyle.Fill,
                Name = "gridForTimers",
                TabIndex = 0
            };

            gridForTimers.RowStyles.Add(new RowStyle(SizeType.Absolute, BOARD_SIZE * SQUARE_SIZE - TIMER_LABEL_HEIGHT / 3));
            gridForTimers.RowStyles.Add(new RowStyle(SizeType.Absolute, TIMER_LABEL_HEIGHT));

            outerPanel.Controls.Add(gridForTimers);
        }


        private void InitializeTimerLabels()
        {
            for (int i = 0; i < N_PLAYERS; i++)
            {
                timerLabel[i] = new Label
                {
                    Text = (i % 2 == 0) ? "White: 00:00:00" : "Black: 00:00:00",
                    Font = new Font("Arial", 15),
                    Size = new Size(TIMER_LABEL_WIDTH, TIMER_LABEL_HEIGHT),
                    Dock = DockStyle.Left | DockStyle.Top
                };

                gridForTimers.Controls.Add(timerLabel[i], Math.Abs(i - 1), 0);
            }
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

        private const int FORM_WIDTH = 750;
        private const int FORM_HEIGHT = 600;

        private const int TIMER_LABEL_WIDTH = 150;
        private const int TIMER_LABEL_HEIGHT = 50;
        public const int SQUARE_SIZE = 60;

        private TableLayoutPanel outerPanel;
        private TableLayoutPanel gridForTimers;


        private TableLayoutPanel boardGrid;
        private Label[] timerLabel = new Label[2];
        private Timer[] timer = new Timer[2];
    }
}