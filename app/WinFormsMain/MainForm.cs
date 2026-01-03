using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsMain
{
    public class MainForm : Form
    {
        private readonly Button btnOpen;
        private readonly Button btnSettings;
        private readonly Label lblTitle;
        private readonly Label lblStatus;

        public MainForm()
        {
            Text = "Main Page";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "Welcome to your App",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            btnOpen = new Button
            {
                Text = "Open",
                Location = new Point(20, 80),
                Size = new Size(120, 40)
            };

            btnSettings = new Button
            {
                Text = "Settings",
                Location = new Point(160, 80),
                Size = new Size(120, 40)
            };

            lblStatus = new Label
            {
                Text = "Status: Ready",
                AutoSize = true,
                Location = new Point(20, 140)
            };

            btnOpen.Click += (s, e) => MessageBox.Show("Open clicked", "Action", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnSettings.Click += (s, e) => MessageBox.Show("Settings clicked", "Action", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Controls.AddRange(new Control[] { lblTitle, btnOpen, btnSettings, lblStatus });
        }
    }
}
