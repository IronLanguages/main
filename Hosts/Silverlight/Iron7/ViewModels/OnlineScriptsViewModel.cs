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
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using Iron7.Iron7Server;

namespace Iron7
{
    public class OnlineScriptsViewModel : AsyncLoadingViewModel
    {
        public OnlineScriptsViewModel()
        {
            Scripts = new ObservableCollection<OnlineScriptViewModel>();
        }

        public string TagAndCount
        {
            get
            {
                return Tag + " (" + Count.ToString() + ")";
            }
        }
        
        public int Count { get; set; }

        private string _tag;
        public string Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
                NotifyPropertyChanged("Tag");
            }
        }

        private bool _loaded;
        public bool Loaded
        {
            get
            {
                return _loaded;
            }
            set
            {
                _loaded = value;
                NotifyPropertyChanged("Loaded");
            }
        }

        Visibility _moreVisibility = Visibility.Collapsed;
        public Visibility MoreVisibility
        {
            get
            {
                return _moreVisibility;
            }
            set
            {
                if (_moreVisibility == value)
                    return;
                _moreVisibility = value;
                NotifyPropertyChanged("MoreVisibility");
            }

        }

        public ObservableCollection<OnlineScriptViewModel> Scripts { get; set; }

        public string MoreUrl { get; set; }

        public void LoadUrl(System.Windows.Threading.Dispatcher dispatcher, string url)
        {
            string urlToCall = "http://script.iron7.com" + url + "&r=" + new Random().Next(10000).ToString();
            Action<string> funcToCall = OnSuccessfulLoad;
            DoGenericLoad(dispatcher, urlToCall, funcToCall);
        }

        private void OnSuccessfulLoad(string text)
        {
            var listing = JsonConvert.DeserializeObject<ScriptListing>(text);
            Scripts.Clear();
            foreach (var item in listing.Scripts)
            {
                var model = new OnlineScriptViewModel()
                {
                    Author = item.AuthorName,
                    Title = item.Title,
                    ScriptId = item.ScriptId
                };
                Scripts.Add(model);
            }
            if (listing.MoreAvailable)
            {
                this.MoreVisibility = Visibility.Visible;
                this.MoreUrl = listing.NextPageUrl;
            }
        }
    }
}
