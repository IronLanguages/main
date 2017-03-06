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

namespace Iron7
{
    public class OnlineTagListViewModel : BaseViewModel
    {
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
    }
}
