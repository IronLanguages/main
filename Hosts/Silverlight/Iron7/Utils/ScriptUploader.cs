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
using System.Text;

namespace Iron7.Utils
{
    public class CommunicationBase
    {
        protected Views.StyledChildWindow childWindow;
        protected Dispatcher dispatcher;
        protected HttpWebRequest myRequest;
        protected CookieContainer cookieContainer = new CookieContainer();
        protected bool cancelRequested = false;

        protected void DoGet(string url, Action<string> doOnFinish)
        {
            try
            {
                myRequest = HttpWebRequest.CreateHttp(url);
                myRequest.Accept = "application/json";
                myRequest.Method = "GET";
                myRequest.CookieContainer = cookieContainer;
                var async1 = myRequest.BeginGetResponse((result) =>
                    {
                        HandleEndResponse(doOnFinish, result);
                    },
                    null);
            }
            catch (Exception exc)
            {
                ShowErrorMessage(exc);
            }
        }

        protected void DoPost(string url, string post, Action<string> doOnFinish)
        {
            try
            {
                myRequest = HttpWebRequest.CreateHttp(url);
                myRequest.Accept = "application/json";
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.CookieContainer = cookieContainer;
                var async1 = myRequest.BeginGetRequestStream((result1) =>
                {
                    HandleEndGetRequestStream(post, doOnFinish, result1);
                },
                    null);
            }
            catch (Exception exc)
            {
                ShowErrorMessage(exc);
            }
        }

        protected void HandleEndGetRequestStream(string post, Action<string> doOnFinish, IAsyncResult result1)
        {
            if (true == cancelRequested)
                return;

            using (var requestStream = myRequest.EndGetRequestStream(result1))
            {
                var data = Encoding.UTF8.GetBytes(post);
                requestStream.Write(data, 0, data.Length);
            }

            var async2 = myRequest.BeginGetResponse((result2) =>
            {
                HandleEndResponse(doOnFinish, result2);
            },
                null);
        }

        protected void HandleEndResponse(Action<string> doOnFinish, IAsyncResult result2)
        {
            try
            {
                var response = myRequest.EndGetResponse(result2);
                using (var rs = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(rs))
                    {
                        var text = sr.ReadToEnd();
                        this.dispatcher.BeginInvoke(() =>
                        {
                            doOnFinish(text);
                        });
                    }
                }
            }
            catch (Exception exc)
            {
                ShowErrorMessage(exc);
            }
        }

        protected void ShowErrorMessage(Exception exc)
        {
            dispatcher.BeginInvoke(() =>
            {
                childWindow.ShowError("Sorry - there was a problem - " + exc.Message);
            });
        }
    }

    public class ScriptUploader : CommunicationBase
    {
        ItemViewModel toUpload;
        AccountViewModel account;

        public void Upload(AccountViewModel account, ItemViewModel toUpload, Dispatcher dispatcher)
        {
            this.account = account;
            this.toUpload = toUpload;
            this.dispatcher = dispatcher;
            childWindow = new Views.StyledChildWindow();

            var backgroundThread = new System.Threading.Thread(UploadProc);
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

            childWindow.Show("uploading script '" + toUpload.Title + "'");
            childWindow.ShowCancelButton();
        }

        private bool TryLogOn(Action doOnSuccess)
        {
            // TODO - POST request sequence needed here... unless you do a nasty hacky combo... which might be worth it...
            if (account.IsEmpty)
                throw new ApplicationException("to upload scripts, you must register with script.iron7.com and provide your account information in 'Share online'");

            var url = "http://script.iron7.com/Account/LogOn";
            string post = string.Format(
                "UserName={0}&Password={1}",
                Uri.EscapeDataString(account.UserName),
                Uri.EscapeDataString(account.Password));

            DoPost(url, post, (s) => {
                if (s.Contains("success"))
                {
                    doOnSuccess();
                }
                else
                {
                    childWindow.ShowError("Error - logon failed - please check your password and your network connection");
                }
            });

            return true;
        }

        private void UploadProc()
        {
            try
            {
                TryLogOn(() =>
                {
                    DoCodePost();
                });
            }
            catch (Exception exc)
            {
                ShowErrorMessage(exc);
            }
        }

        private void DoCodePost()
        {
            var url = "http://script.iron7.com/Script/Upsert";
            string post = string.Format(
                "Code={0}&ScriptId={1}&Title={2}&TagsAsText={3}",
                Uri.EscapeDataString(toUpload.Code),
                Uri.EscapeDataString(toUpload.UniqueId),
                Uri.EscapeDataString(toUpload.Title),
                Uri.EscapeDataString(toUpload.CategoryTag));

            DoPost(url, post, (s) =>
                    {
                        if (s.Contains("success"))
                        {
                            childWindow.Close();
                        }
                        else
                        {
                            childWindow.ShowError("Error - sorry - failed to update the online script");
                        }
                    });
        }

    }
}
