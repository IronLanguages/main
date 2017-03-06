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

namespace Iron7.Common
{
    public class WhichTheme
    {
        public static bool IsLight()
        {
            // thanks to http://insomniacgeek.com/code/how-to-detect-dark-or-light-theme-in-windows-phone-7/
            // don't use binding because of bug - http://betaforums.silverlight.net/forums/p/14658/229679.aspx
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"];
            return isLightTheme == Visibility.Visible;
        }
    }
}
