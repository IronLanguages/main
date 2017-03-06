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
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using Microsoft.Phone.Tasks;
using System.Windows.Navigation;

namespace Iron7.Views
{
    public partial class EditPage : BaseItemPage
    {
        private int failedSets = 0;

        private enum EditorState
        {
            None,
            WaitingToNavigate,
            Navigating,
            Navigated,
            AlmostFullyInitialised,
            FullyInitialised,
            MovedAway
        }

        EditorState currentState = EditorState.None;

        DispatcherTimer timer;

        public EditPage()
        {
            InitializeComponent();
            webBrowser1.Visibility = Visibility.Collapsed;
            webBrowser1.Base = "WebContent";
            webBrowser1.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(webBrowser1_Navigated);
            //webBrowser1.Background = BackgroundBrush;
        }

        void  timer_Tick(object sender, EventArgs e)
        {
            switch (currentState)
            {
                case EditorState.None:
                case EditorState.WaitingToNavigate:
                    try
                    {
                        webBrowser1.Navigate(new Uri("baseEditor.html", UriKind.Relative));
                        currentState = EditorState.Navigating;
                        ProgressBar.Visibility = Visibility.Visible;
                        ProgressBar.IsIndeterminate = true;
                        LoadingBlock.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        // do nothing...
                    }
                    break;
                case EditorState.Navigating:
                    break;
                case EditorState.Navigated:
                    if (CurrentItem != null)
                    try
                    {
                        if (failedSets == 10)
                        {
                            MessageBox.Show("Sorry - the editor is taking a long time", Common.Constants.Title, MessageBoxButton.OK);
                        }

                        var setOK = (string)webBrowser1.InvokeScript("showCode", CurrentItem.Code);

                        if (setOK == "1")
                        {
                            webBrowser1.InvokeScript("setBackground", Common.WhichTheme.IsLight() ? "true" : "false");
                            currentState = EditorState.AlmostFullyInitialised;
                        }
                        else
                        {
                            failedSets++;
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing - the webbrowser can be flakey :/
                        failedSets++;
                    }
                    break;
                case EditorState.AlmostFullyInitialised:
                    webBrowser1.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Collapsed;
                    LoadingBlock.Visibility = Visibility.Collapsed;
                    currentState = EditorState.FullyInitialised;
                    break;

                case EditorState.FullyInitialised:
                    // do nothing
                    timer.Stop();
                    break;
                case EditorState.MovedAway:
                    // do nothing
                    break;
                default:
                    break;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            webBrowser1.Visibility = Visibility.Collapsed;
            base.OnNavigatedTo(e);
            if (CurrentItem == null)
                return;

            currentState = EditorState.WaitingToNavigate;
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(0.5);
                timer.Tick += new EventHandler(timer_Tick);
            }
            timer.Start();
        }

        private bool PerformCodeUpdates()
        {
            if (currentState == EditorState.FullyInitialised)
            {
                try
                {
                    var code = (string)webBrowser1.InvokeScript("getCode");
                    CurrentItem.Code = code;
                }
                catch (Exception exc)
                {
                    Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Sorry - there was a problem with the editor - " + exc.Message);
                        });
                    return false;
                }
            }
            return true;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (false == PerformCodeUpdates())
            {
                e.Cancel = true;
                return;
            }

            currentState = EditorState.MovedAway;
            base.OnNavigatingFrom(e);
        }

        private bool TestHtmlCheckbox(string checkboxName)
        {
            var obj = webBrowser1.InvokeScript(checkboxName);
            if (obj == null)
                return false;

            if (obj.ToString() == "1")
                return true;

            return false;
        }

        void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (currentState == EditorState.Navigating)
                currentState = EditorState.Navigated;
        }

        private void ApplicationBarPlay_Click (object sender, EventArgs e)
        {
            if (false == PerformCodeUpdates())
                return;

            NavigationService.Navigate(new Uri("/Views/IronPage.xaml?ScriptIndex=" + CurrentItem.UniqueId, UriKind.Relative));
        }

        private void ApplicationBarCopyToNew_Click(object sender, EventArgs e)
        {
            if (false == PerformCodeUpdates())
                return;

            App.ViewModel.CreateNewItemBasedOn(CurrentItem, NavigationService);
        }

        private void ApplicationBarDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;

            try
            {
                App.ViewModel.Items.Remove(CurrentItem);
                NavigationService.GoBack();
            }
            catch
            {
                // I've never seen an exception here... but just in case (because of the mail task problems)
            }
        }

        private void ApplicationBarEmail_Click(object sender, EventArgs e)
        {
            if (false == PerformCodeUpdates())
                return;

            try
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("Hello!\r\nHere's my latest script\r\nI am sending it to you under a Creative Commons 2.0 Attribution license\r\nYou may use this script however you wish - as long as you include my twitter id - !HERE!\r\n");
                sb.Append("\r\nThe script is called: " + this.CurrentItem.Title);
                sb.Append("\r\nThe script is of type: " + this.CurrentItem.CategoryTag);
                //sb.Append("\r\nI fully understand that you may decide not to use this script and that you do not need to send me any reason for this decision.");
                //sb.Append("\r\nI fully understand that you may have other similar scripts or similar ideas in the past or in the future, and I have no right or claim over any of them.");
                sb.Append("\r\nHere's my script\r\n");
                sb.Append(this.CurrentItem.Code);

                EmailComposeTask emailComposeTask = new EmailComposeTask();
                //emailComposeTask.To = "scripts@iron7.com";
                emailComposeTask.Subject = "My Script:\r\n\r\n" + this.CurrentItem.Title;
                emailComposeTask.Body = sb.ToString();
                emailComposeTask.Show();
            }
            catch (InvalidOperationException)
            {
                // see http://blogs.msdn.com/b/oren/archive/2010/12/01/wp7-gotcha-launchers-will-crash-while-navigating.aspx
            }
        }

        private void ApplicationBarProperties_Click(object sender, EventArgs e)
        {
            if (false == PerformCodeUpdates())
                return;

            NavigationService.Navigate(new Uri("/Views/PropertiesPage.xaml?ScriptIndex=" + CurrentItem.UniqueId, UriKind.Relative));
        }

        private void ApplicationBarShareToIron7_Click(object sender, EventArgs e)
        {
            if (false == PerformCodeUpdates())
                return;

            var u = new Utils.ScriptUploader();
            u.Upload(App.ViewModel.Account, this.CurrentItem, this.Dispatcher);
        }
        
        private void ApplicationBarUpdateFromIron7_Click(object sender, EventArgs e)
        {
            var d = new Utils.ScriptDownloader();
            var item = new OnlineScriptViewModel
            {
                Author = App.ViewModel.Account.UserName,
                ScriptId = base.CurrentItem.UniqueId,
                Title = base.CurrentItem.Title
            };
            d.Download(item, this.Dispatcher,
                (simpleScriptDetail) =>
                {
                    base.CurrentItem.CategoryTag = simpleScriptDetail.TagsAsText;
                    base.CurrentItem.Title = simpleScriptDetail.Title;
                    base.CurrentItem.Code = simpleScriptDetail.Code;
                    base.CurrentItem.WhenLastModified = DateTime.UtcNow.Ticks;
                    try
                    {
                        webBrowser1.InvokeScript("showCode", CurrentItem.Code);
                    }
                    catch (Exception exc)
                    {
                        // ho hum!
                        MessageBox.Show("Sorry - there was a problem updating the editor - " + exc.Message);
                    }
                });
        }

    }
}