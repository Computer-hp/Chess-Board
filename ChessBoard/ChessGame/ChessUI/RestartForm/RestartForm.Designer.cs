namespace ChessUI
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
            ClientSize = new Size(202, 200);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RestartForm";
            StartPosition = FormStartPosition.Manual;
            ResumeLayout(false);
        }


        private void InitializeRestartMenuForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimumSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.MaximumSize = new Size(FORM_WIDTH, FORM_HEIGHT);
            this.Size = new Size(FORM_WIDTH, FORM_HEIGHT);
        }


        private void InitializeRetartMenuComponents()
        {
            int buttonWidth = 100;
            int buttonHeight = 40;

            string winnerMessage = (!String.IsNullOrEmpty(winner)) ? "Draw" : $"Winner is: { winner }";

            Label label = new Label()
            {
                Text = winnerMessage,
                Width = buttonWidth - 4,
                Height = buttonHeight - 14,
                BackColor = Color.Peru,
                ForeColor = Color.White,
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            label.Left = (FORM_WIDTH - label.Width) / 2;
            label.Top = (FORM_HEIGHT - label.Height) / 2 - 60; 


            Button button1 = new Button()
            {
                Text = "New Game",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = (FORM_WIDTH - buttonWidth) / 2,
                Top = (FORM_HEIGHT - buttonHeight) / 2 - 15,

                BackColor = Color.Peru,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            Button button2 = new Button
            {
                Text = "Main Menu",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = (FORM_WIDTH - buttonWidth) / 2,
                Top = (FORM_HEIGHT - buttonHeight) / 2 + 30,

                BackColor = Color.Peru,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            button1.Click += Button_Click;
            button2.Click += Button_Click;

            Controls.Add(label);
            Controls.Add(button1);
            Controls.Add(button2);
        }

        #endregion


        private const int FORM_WIDTH  = 220;
        private const int FORM_HEIGHT = 200;
    }
}