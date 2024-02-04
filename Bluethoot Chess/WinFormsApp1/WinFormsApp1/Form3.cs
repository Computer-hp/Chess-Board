using Microsoft.Win32;
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
    public partial class RestartForm : Form
    {
        public static bool NewGame { get; set; }  = false;
        public static bool MainMenu { get; set; } = false;

        public RestartForm()
        {
            InitializeComponent();
            InitializeRetartMenu();
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
