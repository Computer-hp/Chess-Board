using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class MainMenu : Form
    {
        private ChessBoardForm chessBoardForm;
        private Thread mainFormThread;



        public MainMenu()
        {
            InitializeComponent();
            InitializeMainMenuButtons();
        }



        private void Create_ChessBoard(object? sender, EventArgs e)
        {
            chessBoardForm = new ChessBoardForm();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // popUp to choose the color and the time

            this.Invoke(chessBoardForm.Show);
            this.Invoke(new Action(Hide));

            mainFormThread = new Thread(() => HandleChessBoard());
            mainFormThread.Start();
        }



        private void HandleChessBoard()
        {
            while (true)
            {
                if (chessBoardForm.isRestarted)
                    break;

                if (chessBoardForm.isClosed)
                {
                    this.Invoke(new Action(Show));
                    return;
                }

            }

            this.Invoke(new Action(chessBoardForm.Close));
            Create_ChessBoard(null, EventArgs.Empty);
        }

        

        private void Button_ConnectBluetooth(object sender, EventArgs e)
        {

        }



        private void Button_Exit(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}