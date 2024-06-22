using WindowHelper;
using ChessLogic;
using ChessGame;
using Events;
using System.Diagnostics;
using System.DirectoryServices;


// TODO  Pin on pieces

// Checkmate doesn't work for the knight

// FIX: king is able to move in the same direction as the one of the piece that has given check
//      this is because the CalculateMoves method calculates the moves until the position of the king.
//      if there are squares after the king, it should not be able to move.

namespace ChessUI
{
    public partial class ChessBoardForm : Form
    {
        private const int BOARD_SIZE = 8;
        private const int N_PLAYERS = 2;

        private int[] secondsElapsed = new int[N_PLAYERS];


        private Color? previousButtonColor = null;
        private Button? lastClickedButton = null;

        private Game game;

        public bool IsRestarted { get; private set; } = false;
        public bool IsClosed { get; private set; } = false;


        public ChessBoardForm()
        {
            game = new Game();
            InitializeEvents();
            InitializeComponent();
            InitializeOtherComponents();
            DarkThemeWindowHelper.ApplyDarkTheme(this);
        }


        private void InitializeEvents()
        {
            game.Castle += Game_Castle!;
            game.GameFinish += Game_Finish!;
            game.UIPieceMovement += UI_Piece_Movement!;
            game.UIClockTick += UI_Clock_Tick!;
        }




        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            var destinationSquare = ((int x, int y))clickedButton.Tag;

            ManageClickedButton(clickedButton, destinationSquare);
            game.ManageUIClickedButton(destinationSquare);

            if (game.IsLastClickedButtonNull)
                lastClickedButton = null;

            else if (game.HasLastClickedButtonChangedColor &&
                     game.IsLastClickedButtonNull)
            {
                lastClickedButton!.BackColor = (Color)previousButtonColor!;
                lastClickedButton = null;
            }
            else if (!game.IsLastClickedButtonNull)
                lastClickedButton = clickedButton;
        }


        // PROBLEM: example --> clicking second button after first doesnt' remove focus of the first
        private void ManageClickedButton(Button clickedButton, (int x, int y) destinationSquare)
        {
            if (game.IsPieceClicked(destinationSquare))
            {
                if (lastClickedButton is not null)
                    lastClickedButton.BackColor = (Color)previousButtonColor!;

                else
                {
                    previousButtonColor = clickedButton.BackColor;
                    clickedButton.BackColor = Color.DarkSeaGreen;
                }
            }

            else if (lastClickedButton is not null)
                lastClickedButton.BackColor = (Color)previousButtonColor!;
        }


        private void UI_Piece_Movement(object sender, UIPieceMovementEventArgs e)
        {
            DisplayPieceMovement(e.DestButtonTag, e.ButtonImage);
        }


        private void DisplayPieceMovement((int x, int y) destSquare, Bitmap image)
        {
            Button destButton = GetButtonAtPosition(destSquare);
            destButton.BackgroundImage = image; //lastClickedButton!.BackgroundImage;

            lastClickedButton!.BackgroundImage = null;
        }


        private void UI_Clock_Tick(object sender, UIClockTickEventArgs e)
        {
            ManageClockTick(e.BlackOrWhiteClock);
        }


        private void ManageClockTick(int blackOrWhiteClock)
        {
            Debug.Write($"blackOrWhiteClock = {blackOrWhiteClock}");
            timer[blackOrWhiteClock].Stop();

            int oppositeClock = Math.Abs(blackOrWhiteClock - 1);
            timer[oppositeClock].Tag = oppositeClock;

            timerLabel[oppositeClock].BeginInvoke((MethodInvoker)delegate
            {
                timer[oppositeClock].Start();
            });
        }


        private void Game_Castle(object sender, CastleEventArgs e)
        {
            DisplayCastle(e.KingDestination, e.RookDestination, e.Color);
        }


        private void DisplayCastle((int x, int y) kingDest, (int x, int y) rookDest, PieceColor color)
        {
            int rookX = (kingDest.x < BOARD_SIZE / 2) ? 0 : (BOARD_SIZE - 1);
            int rookY = kingDest.y;

            Button rookSquare = GetButtonAtPosition((rookX, rookY)); // rookDestination is not enough.
                                                                      // need to recognise wheather O_O or O_O_O
            rookSquare.BackgroundImage = null;

            Bitmap rookImage = PieceImages.GetPieceImage(color, 'R');

            rookSquare = GetButtonAtPosition((rookDest.x, rookDest.y));
            rookSquare.BackgroundImage = rookImage;
        }


        private void Game_Finish(object sender, GameFinishEventArgs e)
        {
            ShowRestartForm(e.Winner);
        }


        private void ShowRestartForm(string winner)
        {
            var popUp = new RestartForm(winner) { StartPosition = FormStartPosition.CenterParent };
            popUp.ShowDialog(this);

            if (RestartForm.NewGame) IsRestarted = true;

            else if (RestartForm.MainMenu) this.Close();
        }


        // Find the button at the specified position
        private Button GetButtonAtPosition(ValueTuple<int, int> selectedSquare)
        {
            var boardGrid = outerPanel.GetControlFromPosition(0, 0);

            foreach (var button in boardGrid.Controls.OfType<Button>())
                if (selectedSquare == (ValueTuple<int, int>)button.Tag)
                    return button;

            return null;
        }
    }
}