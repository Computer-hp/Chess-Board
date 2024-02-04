namespace WinFormsApp1
{
    partial class RestartForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // RestartForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(202, 165);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RestartForm";
            StartPosition = FormStartPosition.Manual;
            ResumeLayout(false);
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

        #endregion
    }
}