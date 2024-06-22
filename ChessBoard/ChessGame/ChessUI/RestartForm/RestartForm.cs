using WindowHelper;


namespace ChessUI
{
    public partial class RestartForm : Form
    {
        private string winner;
        public static bool NewGame { get; set; }  = false;
        public static bool MainMenu { get; set; } = false;

        public RestartForm(string winner)
        {
            this.winner = winner;
            InitializeComponent();
            InitializeRetartMenu();
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
