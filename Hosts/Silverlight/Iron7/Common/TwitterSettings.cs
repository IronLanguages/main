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
    public class TwitterSettings
    {
        // Make sure you set your own ConsumerKey and Secret from dev.twitter.com
        public static string ConsumerKey = "V3NLoYbTHsidf67BJ5ZlA";
        public static string ConsumerKeySecret = "uZcCs4mGwrqb4rVLCcjcf2BwOojFqtSTi0Sv94nZU";

        public static string RequestTokenUri = "https://api.twitter.com/oauth/request_token";
        public static string OAuthVersion = "1.0";
        public static string CallbackUri = "http://www.cirrious.com";
        public static string AuthorizeUri = "https://api.twitter.com/oauth/authorize";
        public static string AccessTokenUri = "https://api.twitter.com/oauth/access_token";
    }

    public class TwitterAccess
    {
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public string UserId { get; set; }
        public string ScreenName { get; set; }
    }
}
