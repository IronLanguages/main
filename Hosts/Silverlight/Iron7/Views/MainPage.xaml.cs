///#define Iron7Free

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
using Microsoft.Phone.Tasks;
using Iron7.Common;

namespace Iron7.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            UpdateCachedTrialResult();

            if (CachedTrialResult == TrialResult.Full)
            {
                this.Purchase1.Visibility = Visibility.Collapsed;
                this.Purchase2.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.ThanksPurchase1.Visibility = Visibility.Collapsed;
            }

            var url = Common.WhichTheme.IsLight()
                ? "/Assets/jimkster_2846315611_light_header.png"
                : "/Assets/jimkster_2846315611_dark_header.png";

            Panorama1.Background = new ImageBrush()
            {
                ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(url, UriKind.Relative))
            };
        }

        private void ButtonCreateNew_Simple_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCommon("script", "simple", AppResources.NewAppTemplate);
        }

        private void ButtonCreateNew_Location_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCommon("location", "location", AppResources.NewLocationAppTemplate);
        }

        private void ButtonCreateNew_Canvas_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCommon("canvas", "canvas", AppResources.NewCanvasTemplate);
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCommon("script", "simple", string.Empty);
        }

        private void ButtonCreateNew_Accelerometer_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCommon("sensor", "sensor", AppResources.NewAccelerometerAppTemplate); 
        }

        private void ButtonCreateNew_EventBased_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCommon("events", "simple", AppResources.NewButtonAndTimerAppTemplate);
        }

        private void CreateNewCommon(string title, string tags, string code) 
        {
            var selectedItem = new ItemViewModel()
            {
                CategoryTag = tags,
                Code = code,
                Title = title
            };
            App.ViewModel.CreateNewItemBasedOn(selectedItem, NavigationService, false);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void HyperlinkButtonCirrious_Click(object sender, RoutedEventArgs e)
        {
            ShowWebPage("http://www.cirrious.com/");
        }

        private void PinwheelGalaxyHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWebPage("http://m.flickr.com/#/photos/jimkster/2846315611/");
        }

        private void CodeMirrorHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/CodeMirrorCredits.xaml", UriKind.Relative));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWebPage("http://ironruby.codeplex.com/");
        }

        private void HyperlinkButtonCreateMsdnCom_Click(object sender, RoutedEventArgs e)
        {
            ShowWebPage("http://create.msdn.com/");
        }

        private void CreativeCommonsHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWebPage("http://creativecommons.org/about/licenses/");
        }

        private void HyperlinkButtonScriptIron7_Click(object sender, RoutedEventArgs e)
        {
            ShowWebPage("http://script.iron7.com/");
        }

        private void HyperlinkButtonIron7_Click(object sender, RoutedEventArgs e)
        {
            ShowWebPage("http://www.iron7.com/");
        }

        private void ButtonDownloadScript_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/TagsOnlinePage.xaml", UriKind.Relative));
        }

        private void ButtonMyScripts_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(App.ViewModel.Account.UserName))
            {
                MessageBox.Show("To download your scripts, you must register with script.iron7.com and provide your account information in 'Share online'", Constants.Title, MessageBoxButton.OK);
                return;
            }
            var url = new Uri("/Views/ScriptListOnlinePage.xaml?Url=" + Uri.EscapeDataString("/Script/UserScripts?userName=" + App.ViewModel.Account.UserName), UriKind.Relative);
            NavigationService.Navigate(url);
        }

        private void ButtonIron7Scripts_Click(object sender, RoutedEventArgs e)
        {
            var url = new Uri("/Views/ScriptListOnlinePage.xaml?Url=" + Uri.EscapeDataString("/Script/UserScripts?userName=iron7"), UriKind.Relative);
            NavigationService.Navigate(url);
        }

        //private void TwitterHyperlinkButton_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/Views/EditPage.xaml", UriKind.Relative));
        //}

        private void ShowWebPage(string url)
        {
            var task = new WebBrowserTask()
            {
                URL = url
            };
            task.Show();
        }

        private void ButtonListItem_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (null != button)
            {
                if (CheckLicense() == false)
                {
                    DoMarketPlaceMessage();
                    return;
                }
                var item = button.Tag as ItemViewModel;
                if (null != item)
                {
                    NavigationService.Navigate(new Uri("/Views/EditPage.xaml?ScriptIndex=" + item.UniqueId, UriKind.Relative));
                    //NavigationService.Navigate(new Uri("/Views/EditPage.xaml?ScriptIndex=" + App.ViewModel.Items[ListBoxScripts.SelectedIndex].UniqueId, UriKind.Relative));
                }
            }
        }

        private void PurchaseHyperlinkButton_Click(object sender, EventArgs e)
        {
            GoToMarketplace();
        }

        private bool DoMarketPlaceMessage()
        {
            if (MessageBox.Show("Thanks for using Iron7. Purchasing Iron7 helps support future development. Would you like to purchase now?", "Iron7", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                GoToMarketplace();
                return false;
            }
            return true;
        }

        private static void GoToMarketplace()
        {
#if Iron7Free
            var task = new Microsoft.Phone.Tasks.MarketplaceDetailTask()
            {
                ContentIdentifier = "10aa051d-e907-e011-9264-00237de2db9e"
            };
#else
            var task = new Microsoft.Phone.Tasks.MarketplaceDetailTask();
#endif
            task.Show();
        }

        private const int Trial_Prompt_Every_N_Activities = 6;

        private enum TrialResult
        {
            Unknown,
            Trial,
            Full
        }

        private TrialResult CachedTrialResult = TrialResult.Unknown;

        static int numChecks = 0;

        private bool CheckLicense()
        {
            UpdateCachedTrialResult();

            if (CachedTrialResult == TrialResult.Full)
                return true;

            numChecks++;

            // numChecks>0 every nth time at random we ask them about upgrading
            if (numChecks > 3 && new Random().Next(Trial_Prompt_Every_N_Activities) == 0)
            {
                numChecks = 0;
                return false;
            }

            return true;
        }

        private void UpdateCachedTrialResult()
        {
#if Iron7Free
            CachedTrialResult = TrialResult.Trial;
#else
            if (CachedTrialResult == TrialResult.Unknown)
            {
                Microsoft.Phone.Marketplace.LicenseInformation license = new Microsoft.Phone.Marketplace.LicenseInformation();
                if (license.IsTrial())
                    CachedTrialResult = TrialResult.Trial;
                else
                    CachedTrialResult = TrialResult.Full;
            }
#endif
        }


    }
}