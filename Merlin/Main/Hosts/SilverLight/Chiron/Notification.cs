using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace Chiron {
    class Notification {
        private NotifyIcon _notifyIcon = new NotifyIcon();
        private DetailsForm _details;
        private readonly string _rootPath;
        private readonly int _port;

        public Notification(string rootPath, int port) {
            _rootPath = rootPath;
            _port = port;

            _notifyIcon.Visible = true;
            _notifyIcon.Icon = new Icon(new MemoryStream(HttpServer.GetResourceBytes("NotifyIcon.ico")));
            _notifyIcon.BalloonTipClicked += ShowDetails;
            _notifyIcon.ContextMenu = new ContextMenu(
                new[] { 
                    new MenuItem("Open in Web Browser", ShowInBrowser),
                    new MenuItem("Stop", Stop),
                    new MenuItem("Show Details", ShowDetails)
                }
            );

            _notifyIcon.ShowBalloonTip(1000, "Chiron Development Server", "Chiron Development Server is running on port " + port, ToolTipIcon.Info);
        }

        public void ShowDetails(object sender, EventArgs args) {
            if (_details == null) {
                _details = new DetailsForm(_rootPath, _port);
                _details.Closed += (sender_inner, args_inner) => { _details = null; };
            }
            
            _details.Show();
        }

        public void ShowInBrowser(object sender, EventArgs args) {
            Process.Start("http://localhost:" + _port);
        }

        public void Stop(object sender, EventArgs args) {
            _notifyIcon.Visible = false;
            Environment.Exit(0);
        }
    }
}
