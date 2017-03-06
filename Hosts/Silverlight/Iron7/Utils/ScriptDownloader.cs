using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Navigation;

namespace Iron7.Utils
{
    public class ScriptDownloader : CommunicationBase
    {
        OnlineScriptViewModel toDownload;
        Action<Iron7Server.SimpleScriptDetail> successfulAction;

        public void Download(OnlineScriptViewModel toDownload, Dispatcher dispatcher, Action<Iron7Server.SimpleScriptDetail> successfulAction)
        {
            this.successfulAction = successfulAction;
            this.toDownload = toDownload;
            this.dispatcher = dispatcher;
            childWindow = new Views.StyledChildWindow();

            var backgroundThread = new System.Threading.Thread(DownloadProc);
            backgroundThread.Start();

            childWindow.Closed += (sender, args) =>
            {
                cancelRequested = true;
                try
                {
                    if (myRequest != null)
                        myRequest.Abort();
                }
                catch
                {
                    // ignore problems :/
                }
            };

            childWindow.Show("copying script " + toDownload.Title);
            childWindow.ShowCancelButton();
        }

        private void DownloadProc()
        {
            try
            {
                if (string.IsNullOrEmpty(toDownload.Author))
                    throw new ApplicationException("to download scripts from your own account, you must register with script.iron7.com and provide your account information in 'Share online'");

                var url = string.Format("http://script.iron7.com/Script/Detail?scriptId={0}&userLowerCaseName={1}&rand={2}",
                                            Uri.EscapeDataString(toDownload.ScriptId),
                                            Uri.EscapeDataString(toDownload.Author),
                                            new Random().Next(100000));

                DoGet(url, (s) => {
                    try
                    {
                        var script = JsonConvert.DeserializeObject<Iron7Server.SimpleScriptDetail>(s);
                        childWindow.Close();
                        this.successfulAction(script);
                    }
                    catch (Exception exc)
                    {
                        ShowErrorMessage(exc);
                    }
                });
            }
            catch (Exception exc)
            {
                ShowErrorMessage(exc);
            }
        }
    }
}
