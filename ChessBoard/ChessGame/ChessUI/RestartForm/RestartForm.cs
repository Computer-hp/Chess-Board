using WindowHelper;


namespace ChessUI
{
    public partial class RestartForm : Form
    {
        private string winner;
        public bool NewGame { get; private set; }  = false;
        public bool MainMenu { get; private set; } = false;

        public RestartForm(string winner)
        {
            this.winner = winner;
            InitializeComponent();
            InitializeRestartMenuForm();
            InitializeRetartMenuComponents();
            DarkThemeWindowHelper.ApplyDarkTheme(this);
        }



        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            if (clickedButton.Text == "New Game")
                NewGame = true;

            if (clickedButton.Text == "Main Menu")
                MainMenu = true;

            this.Close();
        }
    }
}
