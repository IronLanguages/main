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
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace Iron7.Common
{
    public static class Helper
    {
        private static Object _thisLock = new Object();

        public static T LoadSetting<T>(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                    return default(T);

                lock (_thisLock)
                {
                    try
                    {
                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            using (var sr = new StreamReader(stream))
                            {
                                return (T)JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                            }
                        }
                    }
                    catch (Newtonsoft.Json.JsonSerializationException se)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(
                            () => MessageBox.Show(String.Format("Serialize file error {0}:{1}", se.Message, fileName)));
                        return default(T);
                    }
                    catch (Exception e)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(
                            () => MessageBox.Show(String.Format("Load file error {0}:{1}", e.Message, fileName)));
                        return default(T);
                    }
                }
            }
        }

        public static void SaveSetting<T>(string fileName, T dataToSave)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (_thisLock)
                {
                    try
                    {
                        using (var stream = store.CreateFile(fileName))
                        {
                            using (var sw = new StreamWriter(stream))
                            {
                                var serial = JsonConvert.SerializeObject(dataToSave);
                                sw.Write(serial);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(String.Format("Save file error {0}:{1}", e.Message, fileName));
                        return;
                    }
                }
            }
        }

        public static void SaveEditAreaHtml()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.DirectoryExists("WebContent") == false)
                    store.CreateDirectory("WebContent");                

                WriteLocalWebFile(store, "baseEditor.html", AppResources.BaseEditorHtml);
                WriteLocalWebFile(store, "codemirror.js", ScriptResources.codemirror_js);
                WriteLocalWebFile(store, "editor.js", ScriptResources.editor_js);
                WriteLocalWebFile(store, "parseruby.js", ScriptResources.parseruby_js);
                WriteLocalWebFile(store, "rubycolors.css", ScriptResources.rubycolors_css);
                WriteLocalWebFile(store, "select.js", ScriptResources.select_js);
                WriteLocalWebFile(store, "stringstream.js", ScriptResources.stringstream_js);
                WriteLocalWebFile(store, "tokenize.js", ScriptResources.tokenize_js);
                WriteLocalWebFile(store, "tokenizeruby.js", ScriptResources.tokenizeruby_js);
                WriteLocalWebFile(store, "undo.js", ScriptResources.undo_js);
                WriteLocalWebFile(store, "util.js", ScriptResources.util_js);
                //WriteLocalWebFile(store, "greyback.jpg", ScriptResources.greyback);
            }
        }

        private static void WriteLocalWebFile(IsolatedStorageFile store, string filename, byte[] resource)
        {
            using (IsolatedStorageFileStream isfs = store.OpenFile("WebContent\\" + filename, System.IO.FileMode.Create))
            using (StreamWriter sw = new StreamWriter(isfs))
            {
                sw.Write(resource);
            }
        }

        private static void WriteLocalWebFile(IsolatedStorageFile store, string filename, string resource)
        {
            using (IsolatedStorageFileStream isfs = store.OpenFile("WebContent\\" + filename, System.IO.FileMode.Create))
            using (StreamWriter sw = new StreamWriter(isfs))
            {
                sw.Write(resource);
            }
        }

        public static void DeleteFile(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(fileName))
                    store.DeleteFile(fileName);
            }
        }

        public static DateTime ParseDateTime(string date)
        {
            var dayOfWeek = date.Substring(0, 3).Trim();
            var month = date.Substring(4, 3).Trim();
            var dayInMonth = date.Substring(8, 2).Trim();
            var time = date.Substring(11, 9).Trim();
            var offset = date.Substring(20, 5).Trim();
            var year = date.Substring(25, 5).Trim();
            var dateTime = string.Format("{0}-{1}-{2} {3}", dayInMonth, month, year, time);
            var ret = DateTime.Parse(dateTime).ToLocalTime();

            return ret;
        }

        public static void ShowMessage(string message)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(message));
        }
    }
}
