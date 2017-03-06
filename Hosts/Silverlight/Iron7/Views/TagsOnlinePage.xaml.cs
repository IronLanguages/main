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

namespace Iron7.Views
{
    public partial class TagsOnlinePage : BaseDetailPage
    {
        public TagsOnlinePage()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel;
            LayoutRoot.Background = BackgroundBrush;
            App.ViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ViewModel_PropertyChanged);
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AsyncLoading")
            {
                ProgressBar.Visibility = App.ViewModel.AsyncLoading ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (App.ViewModel.OnlineTags.Count == 0)
            {
                App.ViewModel.LoadOnlineTags(Dispatcher);
            }
        }

        private void ButtonTag_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            var url = new Uri("/Views/ScriptListOnlinePage.xaml?Url=" + Uri.EscapeDataString("/Script/ByTag?tag=" + tag), UriKind.Relative);
            NavigationService.Navigate(url);
        }
    }
}