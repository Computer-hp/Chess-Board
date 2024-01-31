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
        public static bool NewGame = false;
        public static bool MainMenu = false;

        public RestartForm()
        {
            InitializeComponent();
            InitializeRetartMenu();
        }

        private void InitializeRetartMenu()
        {
            BackColor = Color.RosyBrown;
            ForeColor = Color.White;
            Font = new Font("Arial", 12, FontStyle.Bold);

            int buttonWidth = 100;
            int buttonHeight = 40;
            int formWidth = ClientSize.Width;
            int formHeight = ClientSize.Height;

            Button button1 = new Button()
            {
                Text = "New Game",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = (formWidth - buttonWidth) / 2,
                Top = (formHeight - buttonHeight) / 2 - 30,

                BackColor = Color.PaleGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            Button button2 = new Button 
            {
                Text = "Main Menu",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = (formWidth - buttonWidth) / 2,
                Top = (formHeight - buttonHeight) / 2 + 30,

                BackColor = Color.PaleGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            button1.Click += Button_Click;
            button2.Click += Button_Click;

            Controls.Add(button1);
            Controls.Add(button2);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            if (clickedButton.Text == "New Game")
                NewGame = true;

            if (clickedButton.Text == "Main Menu")
                MainMenu = true;

            Close();
        }
    }
}
