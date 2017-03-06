using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Iron7.Common;
using System.IO.IsolatedStorage;
using System.IO;

namespace Iron7
{
    public class ItemViewModel : BaseViewModel
    {
        private string _uniqueId;
        public string UniqueId
        {
            get { return _uniqueId; }
            set
            {
                if (value == _uniqueId)
                    return;

                _uniqueId = value;
                NotifyPropertyChanged("UniqueId");
            }
        }

        private long _whenLastModified;
        public long WhenLastModified
        {
            get
            {
                return _whenLastModified;
            }
            set
            {
                if (value == _whenLastModified)
                    return;
                _whenLastModified = value;
                NotifyPropertyChanged("WhenLastModified");
            }
        }

        private string _title;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private bool _hasWebPermission;
        public bool HasWebPermission
        {
            get { return _hasWebPermission; }
            set
            {
                if (_hasWebPermission == value)
                    return;

                _hasWebPermission = value;
                NotifyPropertyChanged("HasWebPermission");
            }
        }

        private bool _hasGPSPermission;
        public bool HasGPSPermission
        {
            get { return _hasGPSPermission; }
            set
            {
                if (_hasGPSPermission == value)
                    return;

                _hasGPSPermission = value;
                NotifyPropertyChanged("HasGPSPermission");
            }
        }

        private string _code;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public string Code
        {
            get
            {
                if (_code == null)
                {
                    LoadCode();
                }
                return _code;
            }
            set
            {
                if (value != _code)
                {
                    _code = value;
                    NotifyPropertyChanged("Code");
                }
            }
        }

        private string _categoryTag;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string CategoryTag
        {
            get
            {
                return _categoryTag;
            }
            set
            {
                if (value != _categoryTag)
                {
                    _categoryTag = value;
                    NotifyPropertyChanged("CategoryTag");
                }
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                if (value == _author)
                    return;

                _author = value;
                NotifyPropertyChanged("Author");
            }
        }

        // This should not be here - I must learn how to use Blend! (and Silverlight!
        [Newtonsoft.Json.JsonIgnore]
        public SolidColorBrush HighlightColorBrush
        {
            get
            {
                return Common.WhichTheme.IsLight() ?
                    new SolidColorBrush(Colors.Green) :
                    new SolidColorBrush(Color.FromArgb(255, 127, 255, 63));
            }
        }

        public void SetAll(string code, string title, string tags, bool canUseLocation, bool canUseWeb)
        {
            Code = code;
            Title = title;
            CategoryTag = tags;
            HasGPSPermission = canUseLocation;
            HasWebPermission = canUseWeb;
        }

        public ItemViewModel()
        {
            UniqueId = Guid.NewGuid().ToString("N");
            WhenLastModified = DateTime.UtcNow.Ticks;
        }

        public void DeleteFromStore()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (false == isf.DirectoryExists(Constants.MyScriptsDirectoryName))
                {
                    isf.CreateDirectory(Constants.MyScriptsDirectoryName);
                }
                var path = GetIndexPath();
                if (isf.FileExists(path))
                    isf.DeleteFile(path);
                var textPath = GetCodePath();
                if (isf.FileExists(textPath))
                    isf.DeleteFile(textPath);
            }
        }

        public static ItemViewModel LoadItemIndexFrom(IsolatedStorageFile isf, string path)
        {
            string text = ReadTextFile(isf, path);
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<ItemViewModel>(text);
            return deserialized;
        }

        private string GetIndexPath()
        {
            return string.Format("\\{0}\\{1}.index", Constants.MyScriptsDirectoryName, UniqueId);
        }

        private string GetCodePath()
        {
            return string.Format("\\{0}\\{1}.code", Constants.MyScriptsDirectoryName, UniqueId);
        }

        public void Store()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (false == isf.DirectoryExists(Constants.MyScriptsDirectoryName))
                {
                    isf.CreateDirectory(Constants.MyScriptsDirectoryName);
                }

                var path = GetIndexPath();

                var text = JsonConvert.SerializeObject(this);
                WriteTextFile(isf, path, text);

                if (_code != null)
                {
                    var textPath = GetCodePath();
                    WriteTextFile(isf, textPath, _code);
                }
            }
        }

        private void LoadCode()
        {
            try
            {
                var path = GetCodePath();
                _code = ReadTextFile(path);
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry - there was a problem reading the code file");
                _code = "Sorry - there was a problem reading the code file";
            }
        }
    }
}