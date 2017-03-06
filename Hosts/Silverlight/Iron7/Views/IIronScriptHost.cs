using System;
using System.Windows;
using System.Windows.Controls;
namespace Iron7.Views
{
    public interface IIronScriptHost
    {
        void Vibrate(TimeSpan duration);
        void CallJsonService(string name, string url, string callback);
        void CallTextWebService(string name, string url, string callback);
        void LoadSoundEffect(string name, string url, string callback);
        void FixOrientationLandscape();
        void FixOrientationPortrait();
        void StartAccelerometer(double secondsBetweenReadings, string readingCallback);
        void StartGeoCoordinateWatcher(string statusCallback, string positionCallback);
        void StartGeoCoordinateWatcher(string needHigh, string statusCallback, string positionCallback);
        void StartTimer(string name, TimeSpan interval, string callback);
        void ChangeTimer(string name, TimeSpan interval);
        void StopTimer(string name);
        void MonitorControl(object sender, object element, string callback);
        Grid ContentHolder { get; }
        void MonitorComposition(object hint, string callback);
        void MonitorTouchPoints(object hint, string callback);
    }
}
