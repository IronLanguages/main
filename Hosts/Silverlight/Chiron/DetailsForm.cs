using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Chiron {
    internal partial class DetailsForm : Form {
        public DetailsForm(string rootPath, int port) {
            InitializeComponent();

            _urlValue.Text = "http://localhost:" + port;
            _portValue.Text = port.ToString();
            _pathValue.Text = rootPath;
        }        

        private void StopClicked(object sender, EventArgs e) {
            Environment.Exit(0);
        }

        private void UrlClicked(object sender, EventArgs e) {
            Process.Start(_urlValue.Text);
        }

        private void ShowDetails(object sender, EventArgs e) {
            Show();
        }
    }
}
