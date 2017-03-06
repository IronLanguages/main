using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Iron7.Views
{
    public class BaseItemPage : BaseDetailPage
    {
        protected ItemViewModel CurrentItem { get; private set; }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string scriptIndex = NavigationContext.QueryString["ScriptIndex"];

            CurrentItem = App.ViewModel.Items.FirstOrDefault(x => x.UniqueId == scriptIndex);

            if (CurrentItem == null)
            {
                MessageBox.Show("Sorry - script not found", Common.Constants.Title, MessageBoxButton.OK);
                try
                {
                    NavigationService.GoBack();
                }
                catch (InvalidOperationException)
                {
                    // just mask this problem
                }
                return;
            }

            DataContext = CurrentItem;
        }
    }
}
