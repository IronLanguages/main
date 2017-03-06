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

namespace Iron7.Views
{
    public class IronScriptHostProxy : IIronScriptHost
    {
        readonly IIronScriptHost realHost;

        public IronScriptHostProxy(IIronScriptHost realHost)
        {
            this.realHost = realHost;
        }

        public void Vibrate(TimeSpan duration)
        {
            this.realHost.Vibrate(duration);
        }

        public void LoadSoundEffect(string name, string url, string callback)
        {
            realHost.LoadSoundEffect(name, url, callback);
        }

        public void CallJsonService(string name, string url, string callback)
        {
            realHost.CallJsonService(name, url, callback);
        }

        public void CallTextWebService(string name, string url, string callback)
        {
            realHost.CallTextWebService(name, url, callback);
        }
        
        public void FixOrientationLandscape()
        {
            realHost.FixOrientationLandscape();
        }

        public void FixOrientationPortrait()
        {
            realHost.FixOrientationPortrait();
        }

        public void StartAccelerometer(double secondsBetweenReadings, string callback)
        {
            realHost.StartAccelerometer(secondsBetweenReadings, callback);
        }

        public void StartGeoCoordinateWatcher(string statusCallback, string readingCallback)
        {
            realHost.StartGeoCoordinateWatcher(statusCallback, readingCallback);
        }

        public void StartGeoCoordinateWatcher(string needHigh, string statusCallback, string readingCallback)
        {
            realHost.StartGeoCoordinateWatcher(needHigh, statusCallback, readingCallback);
        }

        public void StartTimer(string name, TimeSpan interval, string callback)
        {
            realHost.StartTimer(name, interval, callback);
        }

        public void ChangeTimer(string name, TimeSpan interval)
        {
            realHost.ChangeTimer(name, interval);
        }

        public void StopTimer(string name)
        {
            realHost.StopTimer(name);
        }


        public void MonitorControl(object sender, object element, string callback)
        {
            realHost.MonitorControl(sender, element, callback);
        }

        public Grid ContentHolder 
        {
            get
            {
                return realHost.ContentHolder;
            }
        }

        public void MonitorComposition(object hint, string callback)
        {
            realHost.MonitorComposition(hint, callback);
        }

        public void MonitorTouchPoints(object hint, string callback)
        {
            realHost.MonitorTouchPoints(hint, callback);
        }
    }
}
