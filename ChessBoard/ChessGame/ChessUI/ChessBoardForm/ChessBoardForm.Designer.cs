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
            outerPanel.ColumnCount = 1;
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            outerPanel.Dock = DockStyle.Fill;
            outerPanel.Location = new Point(52, 41);
            outerPanel.Margin = new Padding(0);
            outerPanel.Name = "outerPanel";
            outerPanel.RowCount = 1;
            outerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            outerPanel.Size = new Size(630, 480);
            outerPanel.TabIndex = 0;
            // 
            // formPanel
            // 
            formPanel.Anchor = AnchorStyles.None;
            formPanel.ColumnCount = 3;
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 630F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
            formPanel.Controls.Add(outerPanel, 1, 1);
            formPanel.Location = new Point(0, 0);
            formPanel.Margin = new Padding(0);
            formPanel.Name = "formPanel";
            formPanel.RowCount = 3;
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 480F));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            formPanel.Size = new Size(734, 561);
            formPanel.TabIndex = 0;
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
            ResumeLayout(false);
        }

        private void InitializeOtherComponents()
        {
            InitializeForm();
            InitializeFormPanel();
            InitializeOuterPanel();
            //InitializeBoardGrid();
            //InitializeButtons();
            //InitializeGridForTimers();
            //InitializeTimerLabels();
            //InitializeTimers();
        }


        private void InitializeForm()
        {
            this.ClientSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.MinimumSize = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.Name = "ChessBoardForm";
            this.Text = "Chess Game";
        }


        private void InitializeFormPanel()
        {
            formPanel.Name = "formPanel";
            //formPanel.Size = new Size(734, 561);
            formPanel.Location = new Point(0, 0);
            formPanel.Padding = new Padding(0);
            formPanel.Margin = new Padding(0);

            formPanel.Anchor = AnchorStyles.None;
            //formPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            formPanel.Dock = DockStyle.Fill;

            /*
            formPanel.ColumnCount = 3;
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 630F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));

            formPanel.RowCount = 3;
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 480F));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            */

            formPanel.ColumnCount = 3;
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 86F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7F));

            formPanel.RowCount = 3;
            formPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 7F));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 86F));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 7F));

            formPanel.TabIndex = 0;
            
            /*
            for (int row = 0; row < formPanel.RowCount; row++)
                for (int col = 0; col < formPanel.ColumnCount; col++)
                {
                    Control control = formPanel.GetControlFromPosition(col, row);

                    if (control == null) continue;

                    if ((row + col) % 2 == 0) control.BackColor = Color.Red; // Color for even sum (like white squares)

                    else control.BackColor = Color.Blue; // Color for odd sum (like black squares)
                }
            */
        }


        private void InitializeOuterPanel()
        {
            outerPanel.Size = new Size(BOARD_SIZE * SQUARE_SIZE + TIMER_LABEL_WIDTH, BOARD_SIZE * SQUARE_SIZE);
            outerPanel.Name = "outerPanel";
            //outerPanel.Location = new Point(52, 41);
            outerPanel.Padding = new Padding(0);
            outerPanel.Margin = new Padding(0);

            outerPanel.Anchor = AnchorStyles.None;
            outerPanel.Dock = DockStyle.Fill;

            /*
            outerPanel.ColumnCount = 2;
            outerPanel.RowCount = 1;

            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BOARD_SIZE * SQUARE_SIZE));
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, TIMER_LABEL_WIDTH));

            outerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, BOARD_SIZE * SQUARE_SIZE));
            */

            outerPanel.ColumnCount = 2;
            outerPanel.RowCount = 1;

            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 76F));
            outerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));

            outerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            for (int row = 0; row < outerPanel.RowCount; row++)
                for (int col = 0; col < outerPanel.ColumnCount; col++)
                {
                    Control control = outerPanel.GetControlFromPosition(col, row);

                    if (control == null) continue;

                    if ((row + col) % 2 == 0) control.BackColor = Color.Red; // Color for even sum (like white squares)

                    else control.BackColor = Color.Blue; // Color for odd sum (like black squares)
                }


            outerPanel.TabIndex = 0;
            formPanel.Controls.Add(outerPanel, 1, 1);
        }


        private void InitializeBoardGrid()
        {
            boardGrid = new TableLayoutPanel
            {
                //Location = new Point(52, 41),
                Margin = new Padding(0),
                Padding = new Padding(0),
                RowCount = BOARD_SIZE,
                ColumnCount = BOARD_SIZE,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Size = new Size(BOARD_SIZE * SQUARE_SIZE, BOARD_SIZE * SQUARE_SIZE),
                Anchor = AnchorStyles.None,
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
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
                        //Size = new Size(SQUARE_SIZE, SQUARE_SIZE),
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        BackColor = (row + col) % 2 == 0 ? Color.Ivory : Color.Peru,
                        FlatStyle = FlatStyle.Flat,
                        FlatAppearance = { BorderSize = 0 },
                        BackgroundImageLayout = ImageLayout.Zoom,
                        Tag = (col, row),
                        Anchor = AnchorStyles.None,
                        Dock = DockStyle.Fill,
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
                Padding = new Padding(0),
                Margin = new Padding(0),
                ColumnCount = 1,
                RowCount = 2,
                Name = "gridForTimers",
                TabIndex = 0,
                Anchor = AnchorStyles.None,
                Dock = DockStyle.Fill,
            };

            gridForTimers.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, TIMER_LABEL_WIDTH));

            gridForTimers.RowStyles.Add(new RowStyle(SizeType.Absolute, TIMER_LABEL_HEIGHT));
            gridForTimers.RowStyles.Add(new RowStyle(SizeType.Absolute, BOARD_SIZE * SQUARE_SIZE - TIMER_LABEL_HEIGHT * 2));
            gridForTimers.RowStyles.Add(new RowStyle(SizeType.Absolute, TIMER_LABEL_HEIGHT));

            outerPanel.Controls.Add(gridForTimers, 0, 1);
        }


        private void InitializeTimerLabels()
        {
            for (int i = 0, j = 0; i < N_PLAYERS; i++, j += 2)
            {
                timerLabel[i] = new Label
                {
                    Text = (i % 2 == 0) ? "White: 00:00:00" : "Black: 00:00:00",
                    Font = new Font("Arial", 15),
                    Size = new Size(TIMER_LABEL_WIDTH, TIMER_LABEL_HEIGHT),
                    //AutoSize = true,
                    //Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                    //Anchor = AnchorStyles.None

                    Anchor = AnchorStyles.None,//Top | AnchorStyles.Left,
                    Dock = DockStyle.Fill,
                };

                gridForTimers.Controls.Add(timerLabel[i], j, 0);
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