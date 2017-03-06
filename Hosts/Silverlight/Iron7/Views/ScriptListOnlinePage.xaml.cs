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
    public partial class ScriptListOnlinePage : BaseDetailPage
    {
        private OnlineScriptsViewModel Model = null;

        public ScriptListOnlinePage()
        {
            InitializeComponent();
            LayoutRoot.Background = BackgroundBrush;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var param = NavigationContext.QueryString["Url"];
            if (string.IsNullOrEmpty(param))
            {
                MessageBox.Show("Internal error - empty tag - sorry!");
                return;
            }

            Model = new OnlineScriptsViewModel();
            this.DataContext = Model;
            Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(model_PropertyChanged);
            Model.LoadUrl(Dispatcher, param);
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AsyncLoading")
            {
                this.ProgressBar.Visibility = ((OnlineScriptsViewModel)sender).AsyncLoading ?
                                                        Visibility.Visible :
                                                        Visibility.Collapsed;
            }
        }

        private void ButtonScript_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate();
            var d = new Utils.ScriptDownloader();
            var item = (sender as Button).Tag as OnlineScriptViewModel;
            d.Download(item, this.Dispatcher,
                (simpleScriptDetail) =>
                {
                    var baseItem = new ItemViewModel()
                    {
                        CategoryTag = simpleScriptDetail.TagsAsText,
                        Code = simpleScriptDetail.Code,
                        Title = simpleScriptDetail.Title,
                        UniqueId = simpleScriptDetail.ScriptId
                    };

                    bool sameAuthor = false;
                    if (App.ViewModel.Account.UserName != null)
                        sameAuthor = (simpleScriptDetail.AuthorName.ToLowerInvariant() == App.ViewModel.Account.UserName.ToLowerInvariant());

                    if (sameAuthor)
                    {
                        var existing = App.ViewModel.Items.Where(x => x.UniqueId == simpleScriptDetail.ScriptId).FirstOrDefault();
                        if (existing != null)
                        {
                            var result = MessageBox.Show("Replace your current script?", Iron7.Common.Constants.Title, MessageBoxButton.OKCancel);
                            if (result != MessageBoxResult.OK)
                                return;
                            App.ViewModel.Items.Remove(existing);
                        }                        
                    }
                    App.ViewModel.CreateNewItemBasedOn(baseItem, NavigationService, !sameAuthor, sameAuthor);
                });
        }

        private void ButtonMore_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Views/ScriptListOnlinePage.xaml?Url=" + Uri.EscapeDataString(Model.MoreUrl), UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}