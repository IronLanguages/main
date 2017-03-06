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
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.IO;

namespace Iron7
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected static void WriteTextFile(string path, string text)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                WriteTextFile(isf, path, text);
            }
        }

        protected static void WriteTextFile(IsolatedStorageFile isf, string path, string text)
        {
            using (var fs = isf.CreateFile(path))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }
        }

        protected static string ReadTextFile(string path)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return ReadTextFile(isf, path);
            }
        }

        protected static string ReadTextFile(IsolatedStorageFile isf, string path)
        {
            string text;
            using (var fileStream = isf.OpenFile(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    text = sr.ReadToEnd();
                }
            }
            return text;
        }
    }

    public class AsyncLoadingViewModel : BaseViewModel
    {

        private bool _asyncLoading;
        public bool AsyncLoading
        {
            get
            {
                return _asyncLoading;
            }
            set
            {
                _asyncLoading = value;
                NotifyPropertyChanged("AsyncLoading");
            }
        }

        protected void DoGenericLoad(System.Windows.Threading.Dispatcher dispatcher, string urlToCall, Action<string> funcToCall)
        {
            var request = (HttpWebRequest)WebRequest.Create(urlToCall);
            AsyncLoading = true;
            request.Accept = "application/json";
            try
            {
                var async = request.BeginGetResponse((result) =>
                {
                    try
                    {
                        var response = request.EndGetResponse(result);
                        using (var rs = response.GetResponseStream())
                        {
                            using (var sr = new StreamReader(rs))
                            {
                                var text = sr.ReadToEnd();
                                dispatcher.BeginInvoke(() =>
                                {
                                    try
                                    {
                                        AsyncLoading = false;
                                        funcToCall(text);
                                    }
                                    catch (Exception exc)
                                    {
                                        ShowError(dispatcher, exc);
                                    }
                                });
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        ShowError(dispatcher, exc);
                    }
                },
                    null);
            }
            catch (Exception exc)
            {
                ShowError(dispatcher, exc);
            }
        }

        protected static void ShowError(System.Windows.Threading.Dispatcher dispatcher, Exception exc)
        {
            dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Sorry - there was a problem - " + exc.Message);
            });
        }
    }
}
