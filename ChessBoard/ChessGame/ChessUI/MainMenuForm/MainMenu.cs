using System.Diagnostics;
using WindowHelper;

namespace ChessUI
{
    public partial class MainMenu : Form
    {
        private ChessBoardForm chessBoardForm;
        private CancellationTokenSource? cts;


        public MainMenu()
        {
            InitializeComponent();
            InitializeMainMenuForm();
            InitializeMainMenuButtons();
            DarkThemeWindowHelper.ApplyDarkTheme(this);
        }


        private void Create_ChessBoard(object? sender, EventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                chessBoardForm = new ChessBoardForm();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                chessBoardForm.Show();
                this.Hide();
            }));

            cts = new CancellationTokenSource();
            Task mainFormTask = Task.Run(() => HandleChessBoard(cts.Token), cts.Token);
        }


        private void HandleChessBoard(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (chessBoardForm.IsRestarted)
                    {
                        this.Invoke(new Action(() =>
                        {
                            chessBoardForm.Close();
                            Create_ChessBoard(null, EventArgs.Empty);
                        }));

                        return;
                    }

                    if (chessBoardForm.IsClosed)
                    {
                        this.Invoke(new Action(this.Show));
                        return;
                    }

                    Thread.Sleep(100);
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.Write("\nObjectDisposedException occured\n");
            }
        }


        private void Button_ConnectBluetooth(object sender, EventArgs e)
        {

        }


        private void Button_Exit(object sender, EventArgs e)
        {
            cts?.Cancel();
            Application.Exit();
        }
    }
}