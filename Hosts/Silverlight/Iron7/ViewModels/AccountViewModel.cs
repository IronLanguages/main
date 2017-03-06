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

namespace Iron7
{
    public class AccountViewModel : BaseViewModel
    {
        private string _username;
        public string UserName
        {
            get
            {
                return _username;
            }
            set
            {
                if (value == _username)
                    return;

                _username = value;
                NotifyPropertyChanged("UserName");
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (value == _password)
                    return;

                _password = value;
                NotifyPropertyChanged("Password");
            }
        }

        public void Save(string path)
        {
            WriteTextFile(path, Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }

        public static AccountViewModel LoadFrom(IsolatedStorageFile isf, string path)
        {
            string text = ReadTextFile(isf, path);
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountViewModel>(text);
            return deserialized;
        }

        public bool IsEmpty 
        {
            get
            {
                return string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password);
            }
        }
    }
}
