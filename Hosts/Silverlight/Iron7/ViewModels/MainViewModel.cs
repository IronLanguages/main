using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.IO;
using Iron7.Common;
using System.Windows.Navigation;
using Newtonsoft.Json;
using System.Net;

namespace Iron7
{
    public class MainViewModel : AsyncLoadingViewModel
    {
        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
            this.OnlineTags = new ObservableCollection<OnlineTagListViewModel>();
            Account = new AccountViewModel();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items { get; private set; }

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                if (value != _sampleProperty)
                {
                    _sampleProperty = value;
                    NotifyPropertyChanged("SampleProperty");
                }
            }
        }

        /// <summary>
        /// The user's Account data
        /// </summary>
        public AccountViewModel Account { get; set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public void LoadData()
        {
            LoadAccountData();
            LoadScripts();
        }

        private void LoadAccountData()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(Constants.MyAccountFileName))
                {
                    try
                    {
                        Account = AccountViewModel.LoadFrom(isf, Constants.MyAccountFileName);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("Sorry - there was a problem loading your Iron7 account details - " + exc.Message);
                    }
                }
            }
        }

        public void LoadOnlineTags(System.Windows.Threading.Dispatcher dispatcher)
        {
            var url = "http://script.iron7.com/Script/Index";
            base.DoGenericLoad(dispatcher, url, OnSuccessfulLoad);
        }

        private void OnSuccessfulLoad(string text)
        {
            var items = JsonConvert.DeserializeObject<Iron7Server.TagListItem[]>(text);
            OnlineTags.Clear();
            foreach (var item in items)
            {
                var model = new OnlineTagListViewModel()
                {
                    Tag = item.Tag,
                    Count = item.Count
                };
                OnlineTags.Add(model);
            }
        }

        /// <summary>
        /// Online scripts
        /// </summary>
        public ObservableCollection<OnlineTagListViewModel> OnlineTags { get; set; }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        private void LoadScripts()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // for debug only...
                //DeleteAllFiles(isf);

                bool errorShown = false;
                if (false == isf.DirectoryExists(Constants.MyScriptsDirectoryName))
                {
                    CreateDefaultData();
                }

                var files = isf.GetFileNames(string.Format("{0}\\*.index", Constants.MyScriptsDirectoryName));
                var list = new List<ItemViewModel>();
                foreach (var f in files)
                {
                    if (false == f.EndsWith(".index"))
                    {
                        // note sure why this happens...
                        continue;
                    }

                    try
                    {
                        var path = string.Format("{0}\\{1}", Constants.MyScriptsDirectoryName, f);
                        list.Add(LoadItem(isf, path));
                    }
                    catch (Exception exc)
                    {
                        if (errorShown == false)
                        {
                            errorShown = true;
                            MessageBox.Show("Sorry - there was a problem loading one or more of your scripts");
                        }
                    }                    
                }
                list.Sort((x, y) =>
                    {
                        // return reverse time order
                        return -x.WhenLastModified.CompareTo(y.WhenLastModified);
                    });
                this.Items = new ObservableCollection<ItemViewModel>();
                foreach (var item in list)
                {
                    this.Items.Add(item);
                }
            }

            this.IsDataLoaded = true;
        }

        private static void DeleteAllFiles(IsolatedStorageFile isf)
        {
            if (false == isf.DirectoryExists(Constants.MyScriptsDirectoryName))
                return;

            var filesDel = isf.GetFileNames(string.Format("{0}\\*", Constants.MyScriptsDirectoryName));
            foreach (var f in filesDel)
            {
                isf.DeleteFile(Constants.MyScriptsDirectoryName + "\\" + f);
            }
            isf.DeleteDirectory(Constants.MyScriptsDirectoryName);
        }

        private ItemViewModel LoadItem(IsolatedStorageFile isf, string path)
        {
            return ItemViewModel.LoadItemIndexFrom(isf, path);
        }

        private void StoreItem(ItemViewModel item)
        {
            item.Store();
        }

        public void CreateNewItemBasedOn(ItemViewModel currentItem, NavigationService navigationService, bool includeCopyText = true, bool copyUniqueId = false)
        {
            var newItem = new ItemViewModel()
            {
                Title = (includeCopyText ? "From " : string.Empty) + currentItem.Title,
                Code = currentItem.Code,
                HasGPSPermission = currentItem.HasGPSPermission,
                HasWebPermission = currentItem.HasWebPermission,
                CategoryTag = currentItem.CategoryTag
            };
            if (copyUniqueId)
                newItem.UniqueId = currentItem.UniqueId;
            StoreItem(newItem);

            App.ViewModel.Items.Insert(0, newItem);

            // TODO - get navigation service out of here!
            if (navigationService != null)
            {
                navigationService.Navigate(new Uri("/Views/EditPage.xaml?ScriptIndex=" + newItem.UniqueId, UriKind.Relative));
            }
        }

        public void CreateDefaultData()
        {
            var items = new List<ItemViewModel>();
            
            // just included for experimentation
            //items.Add(new ItemViewModel() { Title = "CSharp1", Code = ScriptResources.CSharpDemo, CategoryTag = "simple" });

            items.Add(new ItemViewModel() { Title = "Hello World", Code = AppResources.StaticHelloWorld, CategoryTag = "simple" });
            items.Add(new ItemViewModel() { Title = "Ruby 101", Code = AppResources.RubyIntro, CategoryTag = "ruby" });
            items.Add(new ItemViewModel() { Title = "Clock", Code = AppResources.GraphicsExample, CategoryTag = "canvas, timer" });
            items.Add(new ItemViewModel() { Title = "Fractal Tree", Code = AppResources.FractalTree, CategoryTag = "canvas, timer, math" });
            items.Add(new ItemViewModel() { Title = "FingerPaint", Code = AppResources.Drawing, CategoryTag = "canvas, event" });
            items.Add(new ItemViewModel() { Title = "Button Game", Code = AppResources.ButtonGame, CategoryTag = "game" });
            items.Add(new ItemViewModel() { Title = "Snake Game", Code = AppResources.SnakeGame, CategoryTag = "game, sensor, canvas" });
            items.Add(new ItemViewModel() { Title = "SquaresIV", Code = AppResources.square_aliens, CategoryTag = "game, canvas, image" });
            items.Add(new ItemViewModel() { Title = "Tetris", Code = AppResources.Tetris, CategoryTag = "canvas, timer, game" });
            items.Add(new ItemViewModel() { Title = "Twitter", Code = AppResources.Twitter, CategoryTag = "network, json" });

            /*
            items.Add(new ItemViewModel() { Title = "A Button", Code = AppResources.DynamicExamples, CategoryTag = "simple, event" });
            items.Add(new ItemViewModel() { Title = "A Timer", Code = AppResources.TimerExample, CategoryTag = "simple, timer" });
            items.Add(new ItemViewModel() { Title = "Hello Map", Code = AppResources.FirstMap, CategoryTag = "tutorial, map" });
            items.Add(new ItemViewModel() { Title = "Mandelbrot Set", Code = AppResources.MandelBrotDrawing, CategoryTag = "canvas, timer, math" });
            items.Add(new ItemViewModel() { Title = "Text Add", Code = AppResources.LogicExample, CategoryTag = "simple, event" });
            items.Add(new ItemViewModel() { Title = "Number Add", Code = AppResources.MathsExample, CategoryTag = "simple, event" });
            items.Add(new ItemViewModel() { Title = "Flickr", Code = AppResources.Flickr, CategoryTag = "network, webtext" });
            items.Add(new ItemViewModel() { Title = "Accelerometer", Code = AppResources.Accelerometer, CategoryTag = "sensor, canvas" });
            items.Add(new ItemViewModel() { Title = "SpaceShip", Code = AppResources.AccelerometerSpace, CategoryTag = "sensor, image" });
            items.Add(new ItemViewModel() { Title = "Location", Code = AppResources.MapWithLocation, CategoryTag = "sensor, map" });
            items.Add(new ItemViewModel() { Title = "Squares", Code = AppResources.square_drawing, CategoryTag = "canvas, timer" });
            items.Add(new ItemViewModel() { Title = "SquaresII", Code = AppResources.square_animating, CategoryTag = "canvas, timer" });
            items.Add(new ItemViewModel() { Title = "SquaresIII", Code = AppResources.square_animating_game, CategoryTag = "game, canvas" });
            items.Add(new ItemViewModel() { Title = "Circles", Code = AppResources.circle_drawing, CategoryTag = "canvas, touch" });
             */
            long fakeWhenLastModified = DateTime.UtcNow.Ticks;
            long oneHour = TimeSpan.FromHours(1.0).Ticks;
            foreach (var i in items)
            {
                fakeWhenLastModified -= oneHour;
                i.WhenLastModified = fakeWhenLastModified;
                i.UniqueId = i.Title;
                i.Author = "iron7";
                StoreItem(i);
            }
        }
    }
}