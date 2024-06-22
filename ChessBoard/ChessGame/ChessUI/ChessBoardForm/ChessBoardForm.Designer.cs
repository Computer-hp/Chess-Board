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
            formPanel = new TableLayoutPanel();
            formPanel.SuspendLayout();
            SuspendLayout();
            // 
            // outerPanel
            // 
            outerPanel.AutoSize = true;
            outerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            outerPanel.ColumnCount = 2;
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            outerPanel.Location = new Point(43, 43);
            outerPanel.Name = "outerPanel";
            outerPanel.RowCount = 1;
            outerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            outerPanel.Size = new Size(0, 0);
            outerPanel.TabIndex = 0;
            // 
            // formPanel
            // 
            formPanel.ColumnCount = 3;
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 630F));
            formPanel.ColumnStyles.Add(new ColumnStyle());
            formPanel.Controls.Add(outerPanel, 1, 1);
            formPanel.Dock = DockStyle.Fill;
            formPanel.Location = new Point(0, 0);
            formPanel.Name = "formPanel";
            formPanel.RowCount = 3;
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 480F));
            formPanel.RowStyles.Add(new RowStyle());
            formPanel.Size = new Size(734, 561);
            formPanel.TabIndex = 1;
            // 
            // ChessBoardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(734, 561);
            Controls.Add(formPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(500, 480);
            Name = "ChessBoardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chess Game";
            Load += Form_Load;
            formPanel.ResumeLayout(false);
            formPanel.PerformLayout();
            ResumeLayout(false);
        }

        private void InitializeOtherComponents()
        {
            InitializeForm();
            InitializeOuterPanel();
            InitializeBoardGrid();
            InitializeButtons();
            InitializeGridForTimers();
            InitializeTimerLabels();
            InitializeTimers();
        }


        private void InitializeForm()
        {
            this.ClientSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.MinimumSize = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.Name = "ChessBoardForm";
            this.Text = "Chess Game";
        }


        private void InitializeOuterPanel()
        {
            formPanel.Controls.Add(outerPanel, 1, 1);
            // outerPanel.AutoSize = true;
            // outerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            outerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            outerPanel.ColumnCount = 2;
            outerPanel.RowCount = 1;

            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            outerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            outerPanel.Location = new Point(SQUARE_SIZE / 2, SQUARE_SIZE);
            outerPanel.Padding = new Padding(SQUARE_SIZE / 2, SQUARE_SIZE, 0, 0);

            outerPanel.Name = "outerPanel";
            outerPanel.Size = new Size(0, 0);
            outerPanel.TabIndex = 0;
        }


        private void InitializeBoardGrid()
        {
            boardGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = BOARD_SIZE,
                ColumnCount = BOARD_SIZE,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Size = new Size(BOARD_SIZE * SQUARE_SIZE, BOARD_SIZE * SQUARE_SIZE),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            for (int i = 0; i < BOARD_SIZE; i++)
                boardGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, SQUARE_SIZE));

            for (int j = 0; j < BOARD_SIZE; j++)
                boardGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SQUARE_SIZE));

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
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
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
                Dock = DockStyle.Fill,
                Name = "gridForTimers",
                TabIndex = 0,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
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
                    Dock = DockStyle.Left | DockStyle.Top,
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
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

        
        private void Form_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
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
        private TableLayoutPanel formPanel;
    }
}