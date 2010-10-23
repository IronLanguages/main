/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License (Ms-PL). A copy of the license can be found in the license.htm file
 * included in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Cassini {
    class MainForm : Form {
        // web server settings
        static string _appPath;
        static string _portString;
        static string _virtRoot;

        // the web server
        Server _server;

        // form controls
        Panel logoPanel = new Panel();
        Label logoLabel = new Label();
        Label appDirLabel = new Label();
        TextBox appDirTextBox = new TextBox();
        Label portLabel = new Label();
        TextBox portTextBox = new TextBox();
        Label vrootLabel = new Label();
        TextBox vrootTextBox = new TextBox();
        Label browseLabel = new Label();
        LinkLabel browseLink = new LinkLabel();
        Button startButton = new Button();
        Button stopButton = new Button();

        [STAThread]
        static void Main(string[] args) {
            Application.Run(new MainForm(args));
        }

        public MainForm(String[] args) {
            _portString = "80";
            _virtRoot = "/";
            _appPath = string.Empty;

            try {
                if (args.Length >= 1) _appPath = args[0];
                if (args.Length >= 2) _portString = args[1];
                if (args.Length >= 3) _virtRoot = args[2];
            }
            catch {
            }

            InitializeForm();

            if (string.IsNullOrEmpty(_appPath)) {
                appDirTextBox.Focus();
                return;
            }

            Start();
        }

        void Start() {
            _appPath = appDirTextBox.Text;
            if (_appPath.Length == 0 || !Directory.Exists(_appPath)) {
                ShowError("Invalid Application Directory");
                appDirTextBox.SelectAll();
                appDirTextBox.Focus();
                return;
            }

            _portString = portTextBox.Text;
            int portNumber = -1;
            try {
                portNumber = Int32.Parse(_portString);
            }
            catch {
            }
            if (portNumber <= 0) {
                ShowError("Invalid Port");
                portTextBox.SelectAll();
                portTextBox.Focus();
                return;
            }

            _virtRoot = vrootTextBox.Text;
            if (_virtRoot.Length == 0 || !_virtRoot.StartsWith("/")) {
                ShowError("Invalid Virtual Root");
                vrootTextBox.SelectAll();
                vrootTextBox.Focus();
                return;
            }

            try {
                _server = new Server(portNumber, _virtRoot, _appPath);
                _server.Start();
            }
            catch {
                ShowError(
                    "Cassini Managed Web Server failed to start listening on port " + portNumber + ".\r\n" +
                    "Possible conflict with another Web Server on the same port.");
                portTextBox.SelectAll();
                portTextBox.Focus();
                return;
            }

            startButton.Enabled = false;
            appDirTextBox.Enabled = false;
            portTextBox.Enabled = false;
            vrootTextBox.Enabled = false;
            browseLabel.Visible = true;
            browseLink.Text = GetLinkText();
            browseLink.Visible = true;
            browseLink.Focus();
        }

        void Stop() {
            try {
                if (_server != null)
                    _server.Stop();
            }
            catch {
            }
            finally {
                _server = null;
            }

            Close();
        }

        string GetLinkText() {
            string s = "http://localhost";
            if (_portString != "80") s += ":" + _portString;
            s += _virtRoot;
            if (!s.EndsWith("/")) s += "/";
            return s;
        }

        void ShowError(String err) {
            MessageBox.Show(err, "Cassini Personal Web Server v3.5", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void InitializeForm() {
            logoPanel.SuspendLayout();
            SuspendLayout();

            logoLabel.BackColor = Color.White;
            logoLabel.Font = new Font("Arial", 18F, (FontStyle.Bold | FontStyle.Italic), GraphicsUnit.Point, (byte)(0));
            logoLabel.ForeColor = Color.RoyalBlue;
            logoLabel.Location = new Point(24, 24);
            logoLabel.Name = "logoLabel";
            logoLabel.Size = new Size(515, 46);
            logoLabel.TabIndex = 0;
            logoLabel.Text = "Cassini Personal Web Server v3.5";

            logoPanel.BackColor = Color.White;
            logoPanel.BorderStyle = BorderStyle.FixedSingle;
            logoPanel.Controls.AddRange(new Control[] { logoLabel });
            logoPanel.Name = "logoPanel";
            logoPanel.Size = new Size(560, 80);
            logoPanel.TabIndex = 0;

            appDirLabel.BackColor = Color.Transparent;
            appDirLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, (byte)(0));
            appDirLabel.Location = new Point(24, 104);
            appDirLabel.Name = "appDirLabel";
            appDirLabel.Size = new Size(152, 20);
            appDirLabel.TabIndex = 1;
            appDirLabel.Text = "Application &Directory:";
            appDirLabel.TextAlign = ContentAlignment.TopRight;

            appDirTextBox.Location = new Point(184, 104);
            appDirTextBox.Name = "appDirTextBox";
            appDirTextBox.Size = new Size(344, 22);
            appDirTextBox.TabIndex = 2;
            appDirTextBox.Text = _appPath;

            portLabel.BackColor = Color.Transparent;
            portLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, (byte)(0));
            portLabel.Location = new Point(24, 144);
            portLabel.Name = "portLabel";
            portLabel.Size = new Size(152, 19);
            portLabel.TabIndex = 3;
            portLabel.Text = "Server &Port:";
            portLabel.TextAlign = ContentAlignment.TopRight;

            portTextBox.Location = new Point(184, 144);
            portTextBox.Name = "portTextBox";
            portTextBox.Size = new Size(72, 22);
            portTextBox.TabIndex = 4;
            portTextBox.Text = _portString;

            vrootLabel.BackColor = Color.Transparent;
            vrootLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, (byte)(0));
            vrootLabel.Location = new Point(24, 184);
            vrootLabel.Name = "vrootLabel";
            vrootLabel.Size = new Size(152, 20);
            vrootLabel.TabIndex = 5;
            vrootLabel.Text = "Virtual &Root:";
            vrootLabel.TextAlign = ContentAlignment.TopRight;

            vrootTextBox.Location = new Point(184, 184);
            vrootTextBox.Name = "vrootTextBox";
            vrootTextBox.Size = new Size(120, 22);
            vrootTextBox.TabIndex = 6;
            vrootTextBox.Text = _virtRoot;

            browseLabel.BackColor = Color.Transparent;
            browseLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, (byte)(0));
            browseLabel.Location = new Point(24, 224);
            browseLabel.Name = "browseLabel";
            browseLabel.Size = new Size(152, 19);
            browseLabel.TabIndex = 7;
            browseLabel.Text = "Click To Browse:";
            browseLabel.TextAlign = ContentAlignment.TopRight;
            browseLabel.Visible = false;

            browseLink.Location = new Point(184, 224);
            browseLink.Name = "browseLink";
            browseLink.Size = new Size(308, 30);
            browseLink.TabIndex = 8;
            browseLink.Text = "";
            browseLink.LinkClicked += delegate(object sender, LinkLabelLinkClickedEventArgs e) {
                browseLink.Links[browseLink.Links.IndexOf(e.Link)].Visited = true;
                System.Diagnostics.Process.Start(browseLink.Text);
            };
            browseLabel.Visible = false;

            startButton.BackColor = SystemColors.Control;
            startButton.FlatStyle = FlatStyle.Popup;
            startButton.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, (byte)(0));
            startButton.Location = new Point(328, 264);
            startButton.Name = "startButton";
            startButton.Size = new Size(96, 28);
            startButton.TabIndex = 9;
            startButton.Text = "Start";
            startButton.Click += delegate { Start(); };

            stopButton.BackColor = SystemColors.Control;
            stopButton.DialogResult = DialogResult.Cancel;
            stopButton.FlatStyle = FlatStyle.Popup;
            stopButton.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, (byte)(0));
            stopButton.Location = new Point(440, 264);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(96, 28);
            stopButton.TabIndex = 10;
            stopButton.Text = "Stop";
            stopButton.Click += delegate { Stop(); };

            AcceptButton = startButton;
            AutoScaleBaseSize = new Size(6, 15);
            AutoScroll = true;
            CancelButton = stopButton;
            ClientSize = new Size(560, 312);
            Controls.AddRange(new Control[] { 
                logoPanel, 
                appDirLabel, appDirTextBox, 
                portLabel, portTextBox, 
                vrootLabel, vrootTextBox, 
                browseLabel, browseLink,
                startButton, stopButton
            });

            Text = "Cassini Personal Web Server v3.5";
            Icon = Cassini.Properties.Resources.Cassini;

            MaximizeBox = false;
            MinimizeBox = true;
            Name = "MainForm";
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterParent;

            logoPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
