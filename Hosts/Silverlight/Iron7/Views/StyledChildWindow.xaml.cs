using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Iron7.Views
{
    public partial class StyledChildWindow : System.Windows.Controls.ChildWindow
    {
        public StyledChildWindow()
        {
            InitializeComponent();

            closeButton.Click += (s, e) =>
            {
                this.DialogResult = true;
                this.Close();
            };

        }

        public void Show(string text)
        {
            this.Show();
            MessageBlock.Text = text;
        }

        public void ShowCancelButton()
        {
            this.closeButton.Content = "Cancel"; // TextProvider.Lookup(TextProvider.Strings.Button_Cancel);
            this.closeButton.Visibility = System.Windows.Visibility.Visible;
        }

        public void ShowError(string errorMessage)
        {
            MessageBlock.Text = errorMessage;
            this.closeButton.Content = "Close"; // TextProvider.Lookup(TextProvider.Strings.Button_Close);
            TheProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            closeButton.Visibility = System.Windows.Visibility.Visible;
        }

        public void ShowComplete(string message)
        {
            MessageBlock.Text = message;
            TheProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            closeButton.Visibility = System.Windows.Visibility.Visible;
            AutoHideSoon();
        }

        private bool autoHideCalled = false;
        private void AutoHideSoon()
        {
            if (autoHideCalled)
                return;

            autoHideCalled = true;

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                try
                {
                    if (this.Visibility == System.Windows.Visibility.Visible)
                    {
                        this.Close();
                    }
                }
                catch
                {
                    // just hide any error - probably means the user found some other way to hide us!
                }
            };
        }
    }
}
