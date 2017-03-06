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
    public class BaseDetailPage : PhoneApplicationPage
    {
        protected Brush BackgroundBrush
        {
            get
            {
                var url = Common.WhichTheme.IsLight()
                    ? "/Assets/jimkster_2846315611_light.png"
                    : "/Assets/jimkster_2846315611_dark.png";
                return new ImageBrush()
                {
                    ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(url, UriKind.Relative)),
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                };
            }
        }
    }
}
