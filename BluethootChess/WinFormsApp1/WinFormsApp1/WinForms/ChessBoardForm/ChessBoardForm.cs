using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;
using WindowHelper;
using ChessLogic;
using ChessGame;


// TODO  Pin on pieces
// checkmate doesn't work for the knight

// FIX: king is able to move in the same direction as the one of the piece that has given check
//      this is because the CalculateMoves method calculates the moves until the position of the king.
//      if there are squares after the king, it should not be able to move.

namespace WinFormsApp1
{
    public partial class ChessBoardForm : Form
    {
        private const int BOARD_SIZE = 8;
        private const int N_PLAYERS = 2;
        public const int SQUARE_SIZE = 70;

        private static readonly string projectPath = GetProjectPath();  // pathToImages

        private Label[] timerLabel = new Label[2];
        private Timer[] timer = new Timer[2];

        private Color? previousButtonColor = null;
        private Button? lastClickedButton = null;

        private Game game = new();

        public bool isRestarted { get; private set; } = false;
        public bool isClosed { get; private set; } = false;


        public ChessBoardForm()
        {
            InitializeComponent();
            InitializeChessBoardFormButtons();
            InitializeChessBoardFormTimers();
            InitializeTimers();
            DarkThemeWindowHelper.ApplyDarkTheme(this);
        }


        private static string GetProjectPath()
        {
            string appDirectory = Application.StartupPath;
            string imagesFolder = Path.GetFullPath(Path.Combine(appDirectory, "..\\..\\..\\Assets\\"));
            return imagesFolder;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
        }


        private void InitializeTimers()
        {
            for (int i = 0; i < N_PLAYERS; i++)
            {
                timer[i] = new Timer { Interval = 1000 };
                timer[i].Tick += Timer_Tick;
            }
        }


        public static Bitmap GetImageForButton(Piece piece)
        {
            string DIR = piece.Color.ToString().ToLower();
            string imagePath = DIR + "\\" + piece.Name + ".png";

            Bitmap originalImage = (Bitmap)Image.FromFile(projectPath + imagePath);
            return originalImage;
        }



        private void ManageClickedButton(Button clickedButton, (int x, int y) destinationSquare)
        {
            // 3 cases
            // empty button is clicked and no other button was clicked

            // button with piece got clicked and no other button was clicked

            // button with piece is green and a new button is clicked --> 1) button contains a piece
            //                                                        --> 2) button is empty and could be a valid square or not.
            
            // PROBLEM: example --> clicking second button after first doesnt' remove focus of the first
            // Maybe it's better to use an image in place of buttons and use the coordinates of the mouse.

            if (!chessBoard.IsSquareNull(destinationSquare))
            {
                if (lastClickedButton is not null)
                    lastClickedButton.BackColor = (Color)previousButtonColor!;

                else
                {
                    previousButtonColor = clickedButton.BackColor;
                    clickedButton.BackColor = Color.DarkSeaGreen;
                }

                return;
            }

            if (lastClickedButton is null)
                return;

            lastClickedButton.BackColor = (Color)previousButtonColor!;
        }
        


        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            var destinationSquare = ((int x, int y))clickedButton.Tag;

            game.ManageUIClickedButton(destinationSquare);

            // for these methods use async and await
            ManageClickedButton(clickedButton, destinationSquare);
        }


        
        private void ManageClockTick()
        {
            // after white moves, black timer starts
            if (!isFirstMovePlayed)
            {
                isFirstMovePlayed = true;
                timer[1].Start();
            }

            // control 'if else' fix later
            // use 2 threads,
            // whiteClockThread and blackClockThread,
            // that have to wait for each other

            else 
            { 
                timer[turn - chessBoard.MovePawnTowardsBlackOrWhite].Stop(); 
                timer[turn].Start(); 
            }
        }



        // Find the button at the specified position
        private Button? GetButtonAtPosition(ValueTuple<int, int> selectedSquare)
        {
            foreach (var button in Controls.OfType<Button>())
                if (selectedSquare == (ValueTuple<int, int>)button.Tag)
                    return button;

            return null;
        }
    }
}