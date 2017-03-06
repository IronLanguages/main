using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Iron7.Common;
using Iron7.Utils;
using IronRuby;
using Microsoft.Devices;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using Microsoft.Scripting;
using System.Windows.Media.Animation;

namespace Iron7.Views
{
    public partial class IronPage : BaseItemPage, IIronScriptHost
    {        
        DispatcherTimer startTimer = null;
        const int MaxConcurrentUIDispatchesAllowed = 1;
        int numberOfDispatchesActive = 0;
        private Dictionary<string, DispatcherTimer> Timers { get; set; }

        public IronPage()
        {
            InitializeComponent();
            Timers = new Dictionary<string, DispatcherTimer>();
            this.LayoutRoot.Background = BackgroundBrush;
        }

        private int progressVisibilityCount = 0;
        public void ShowProgress()
        {
            lock (this)
            {
                progressVisibilityCount++;
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
            }
        }

        public void HideProgress()
        {
            lock (this)
            {
                progressVisibilityCount--;
                if (progressVisibilityCount <= 0)
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (CurrentItem == null)
                return;

            ShowProgress();

            // start timer just delays the launch - just to allow the screen to update
            startTimer = new DispatcherTimer();
            startTimer.Interval = TimeSpan.FromSeconds(0.5);
            startTimer.Tick += new EventHandler(startTimer_Tick);
            startTimer.Start();
        }

        void startTimer_Tick(object sender, EventArgs e)
        {
            startTimer.Stop();
            InitialiseEngine();
            HideProgress();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                foreach (var t in this.Timers)
                {
                    t.Value.Stop();
                }
            }
            catch
            {
                // mask any errors here
            }
            if (startTimer != null)
            {
                startTimer.Stop();
                startTimer = null;
            }
            foreach (var t in Timers)
            {
                t.Value.Stop();
            }
            if (monitoringCompositionCallback != null)
            {
                CompositionTarget.Rendering -= new EventHandler(CompositionTarget_Rendering);
                monitoringCompositionCallback = null;
                monitoringCompositionHint = null;
            }
            if (geoCoordinateWatcher != null)
            {
                geoCoordinateWatcher.Stop();
                geoCoordinateWatcher = null;
            }
            if (accelerometer != null)
            {
                accelerometer.Stop();
                accelerometer = null;
            }
            if (engine != null)
            {
                engine = null;
            }
            if (scope != null)
            {
                scope = null;
            }
            base.OnNavigatingFrom(e);
        }

        #region IIronScriptHost implementation

        private string monitoringCompositionCallback = null;
        private object monitoringCompositionHint = null;

        public void MonitorComposition(object hint, string callback)
        {
            if (monitoringCompositionCallback != null)
                return;

            monitoringCompositionHint = hint;
            monitoringCompositionCallback = callback;

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            GenericScriptAction("compositiontarget", monitoringCompositionHint, null, "compositiontarget_rendering", monitoringCompositionCallback);
        }

        private string monitoringTouchCallback = null;
        private object monitoringTouchHint = null;

        public void MonitorTouchPoints(object hint, string callback)
        {
            if (monitoringCompositionCallback != null)
                return;

            monitoringTouchHint = hint;
            monitoringCompositionCallback = callback;

            Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);
        }

        void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            GenericScriptAction("touch", monitoringTouchHint, null, "touch_frame_reported", monitoringTouchCallback);
        }

        public void Vibrate(TimeSpan duration)
        {
            var totalSeconds = duration.TotalSeconds;
            if (totalSeconds<0 || totalSeconds>1)
                return;

            VibrateController vc = VibrateController.Default;
            vc.Start(duration);
        }

        public void StartTimer(string name, TimeSpan interval, string callback)
        {
            if (Timers.ContainsKey(name))
                return;

            if (interval.TotalMilliseconds < 20)
                interval = TimeSpan.FromMilliseconds(20);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = interval;
            timer.Tick += (sender, args) =>
            {
                SafeUIInvoke(() =>
                    {
                        GenericScriptAction("timer", name, null, "timer_tick", callback);
                    },
                    false);
            };
            timer.Start();
        }

        public void ChangeTimer(string name, TimeSpan interval)
        {
            DispatcherTimer timer;
            if (false == Timers.TryGetValue(name, out timer))
                return;

            if (interval.TotalMilliseconds < 20)
                interval = TimeSpan.FromMilliseconds(20);

            timer.Interval = interval;
        }

        public void StopTimer(string name)
        {
            DispatcherTimer timer;
            if (false == Timers.TryGetValue(name, out timer))
                return;

            timer.Stop();
            Timers.Remove(name);
        }

        public void LoadSoundEffect(string name, string url, string callback)
        {
            CallWebServiceInner(name, url, (Stream stream) =>
            {
                var soundEffect = Microsoft.Xna.Framework.Audio.SoundEffect.FromStream(stream);

                SafeUIInvoke(() =>
                {
                    try
                    {
                        GenericScriptAction("soundeffect", name, new SoundEffectPlayer(soundEffect), "sound_effect_loaded", callback);
                    }
                    catch (Exception exc)
                    {
                        ShowMessageBox("Error seen processing sound effect " + exc.Message);
                    }
                    finally
                    {
                        HideProgress();
                    }
                },
                true);
            });
        }

        public void MonitorControl(object hint, object element, string callback)
        {
            if (element is Button)
            {
                (element as Button).Click += (sender, args) =>
                {
                    GenericScriptAction("uielement", hint, element, "button_clicked", callback);
                };
            }
            if (element is Pivot)
            {
                var pivot = element as Pivot;
                pivot.LoadedPivotItem += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Pivot_item_event_args"] = args;
                        GenericScriptAction("uielement", hint, element, "pivot_loaded_pivot_item", callback, dict);
                    };
                pivot.LoadingPivotItem += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Pivot_item_event_args"] = args;
                        GenericScriptAction("uielement", hint, element, "pivot_loading_pivot_item", callback, dict);
                    };
                pivot.SelectionChanged += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Selection_changed_event_args"] = args;
                        GenericScriptAction("uielement", hint, element, "pivot_selection_changed", callback, dict);
                    };
                pivot.UnloadedPivotItem += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Pivot_item_event_args"] = args;
                        GenericScriptAction("uielement", hint, element, "pivot_unloaded_pivot_item", callback, dict);
                    };
                pivot.UnloadingPivotItem += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Pivot_item_event_args"] = args;
                        GenericScriptAction("uielement", hint, element, "pivot_unloading_pivot_item", callback, dict);
                    };
            }
            if (element is Panorama)
            {
                var panorama = element as Panorama;
                panorama.SelectionChanged += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Selection_changed_event_args"] = args;
                        GenericScriptAction("uielement", hint, element, "panorama_selection_changed", callback, dict);
                    };
            }
            if (element is MediaElement)
            {
                var media_element = element as MediaElement;
                media_element.BufferingProgressChanged += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "media_element_buffering_progress_changed", callback);
                    };
                media_element.CurrentStateChanged += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "media_element_current_state_changed", callback);
                    };
                media_element.DownloadProgressChanged += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "media_element_download_progress_changed", callback);
                    };
                media_element.MediaOpened += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "media_element_media_opened", callback);
                    };
                media_element.MediaFailed += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Error_exception"] = args.ErrorException;
                        GenericScriptAction("uielement", hint, element, "media_element_media_failed", callback, dict);
                    };
                media_element.MediaEnded += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "media_element_media_ended", callback);
                    };
            }
            else if (element is Storyboard)
            {
                (element as Storyboard).Completed += (sender, args) =>
                {
                    GenericScriptAction("uielement", hint, element, "storyboard_completed", callback);
                };
            }
            else if (element is TextBox)
            {
                (element as TextBox).TextChanged += (sender, args) =>
                {
                    GenericScriptAction("uielement", hint, element, "text_changed", callback);
                };
            }
            else if (element is Map)
            {
                var map = element as Map;
                map.MapPan += (sender, args) =>
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["MapDrag"] = args;
                        GenericScriptAction("uielement", hint, element, "map_panned", callback);
                    };
                map.MapZoom += (sender, args) =>
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["MapZoom"] = args;
                        GenericScriptAction("uielement", hint, element, "map_zoomed", callback);
                    };
            }
            else if (element is ListBox)
            {
                var lb = element as ListBox;
                lb.SelectionChanged += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "list_box_selection_changed", callback);
                    };
            }
            else if (element is ComboBox)
            {
                var cb = element as ComboBox;
                cb.SelectionChanged += (sender, args) =>
                {
                    GenericScriptAction("uielement", hint, element, "combo_box_selection_changed", callback);
                };
            }
            else if (element is CheckBox)
            {
                var cb = element as CheckBox;
                cb.Checked += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "check_box_checked", callback);
                    };
                cb.Unchecked += (sender, args) =>
                {
                    GenericScriptAction("uielement", hint, element, "check_box_unchecked", callback);
                };
            }
            else if (element is Slider)
            {
                var slider = element as Slider;
                slider.ValueChanged += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "slider_value_changed", callback);
                    };
            }
            else if (element is MediaElement)
            {
                var me = element as MediaElement;
                me.CurrentStateChanged += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "media_element_current_state_changed", callback);
                    };
            }
            else if (element is RadioButton)
            {
                var rb = element as RadioButton;
                rb.Checked += (sender, args) =>
                {
                    GenericScriptAction("uielement", hint, element, "radio_button_checked", callback);
                };
            }
            else if (element is PasswordBox)
            {
                var pb = element as PasswordBox;
                pb.PasswordChanged += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "password_box_password_changed", callback);
                    };
            }
            else if (element is HyperlinkButton)
            {
                (element as HyperlinkButton).Click += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "hyperlink_button_clicked", callback);
                    };
            }
            else if (element is Image)
            {
                var image = element as Image;
                image.ImageFailed += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["Error_exception"] = args.ErrorException;
                        GenericScriptAction("uielement", hint, element, "image_failed", callback, dict);
                    };
                image.ImageOpened += (sender, args) =>
                    {
                        GenericScriptAction("uielement", hint, element, "image_opened", callback);
                    };
            }
            else if (element is WebBrowser)
            {
                var wb = element as WebBrowser;
                wb.Navigating += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["WebBrowserNavigating"] = args;
                        GenericScriptAction("uielement", hint, element, "web_browser_navigating", callback);
                    };
                wb.Navigated += (sender, args) =>
                    {
                        var dict = new Dictionary<string, object>();
                        dict["WebBrowserNavigation"] = args;
                        GenericScriptAction("uielement", hint, element, "web_browser_navigated", callback, dict);
                    };
            }
            

            if (ShouldRespondToMouse(element))
            {
                UIElement uielement = (element as UIElement);
                uielement.MouseLeftButtonDown += (sender, args) =>
                {
                    var mouseArgs = GetMouseArgs(uielement, args);
                    GenericScriptAction("uielement", hint, element, "mouse_left_button_down", callback, mouseArgs);
                    args.Handled = true;
                };

                uielement.MouseLeftButtonUp += (sender, args) =>
                {
                    var mouseArgs = GetMouseArgs(uielement, args);
                    GenericScriptAction("uielement", hint, element, "mouse_left_button_up", callback, mouseArgs);
                    args.Handled = true;
                };

                uielement.MouseMove += (sender, args) =>
                {
                    var mouseArgs = GetMouseArgs(uielement, args);
                    GenericScriptAction("uielement", hint, element, "mouse_move", callback, mouseArgs);
                };
            }
        }

        private bool ShouldRespondToMouse(object element)
        {
            if (element is Canvas)
                return true;

            if (element is Image)
                return true;

            if (element is Shape)
                return true;

            return false;
        }

        private static Dictionary<string, object> GetMouseArgs(UIElement element, MouseEventArgs args)
        {
            var mouseArgs = new Dictionary<string, object>();
            mouseArgs["Mouse_x"] = args.GetPosition(element).X;
            mouseArgs["Mouse_y"] = args.GetPosition(element).Y;
            return mouseArgs;
        }

        public Grid ContentHolder
        {
            get
            {
                return ContentGrid;
            }
        }

        private GeoCoordinateWatcher geoCoordinateWatcher;

        public void StartGeoCoordinateWatcher(string statusCallback, string readingCallback)
        {
            StartGeoCoordinateWatcher(string.Empty, statusCallback, readingCallback);
        }

        public void StartGeoCoordinateWatcher(string needHigh, string statusCallback, string readingCallback)
        {
            if (geoCoordinateWatcher != null)
                return;

            if (false == GetGPSPermission())
                return;

            GeoPositionAccuracy accuracy = GeoPositionAccuracy.Default;
            if (needHigh == "high")
                accuracy = GeoPositionAccuracy.High;

            geoCoordinateWatcher = new GeoCoordinateWatcher(accuracy);
            geoCoordinateWatcher.StatusChanged += (sender, args) =>
                {
                    var dict = new Dictionary<string, object>();
                    dict["GeoStatusChange"] = (int)args.Status;

                    SafeUIInvoke(() =>
                    {
                        GenericScriptAction("geoCoordinateWatcher", "geoCoordinateWatcher", null, "status_changed", statusCallback, dict);
                    }, true);
                };

            geoCoordinateWatcher.PositionChanged += (sender, args) =>
                {
                    var dict = new Dictionary<string, object>();
                    dict["GeoPositionChange"] = args;

                    SafeUIInvoke(() =>
                    {
                        GenericScriptAction("geoCoordinateWatcher", "geoCoordinateWatcher", null, "position_changed", readingCallback, dict);
                    },
                    false);
                };
            geoCoordinateWatcher.Start();
        }

        private bool GetGPSPermission()
        {
            if (CurrentItem == null)
                return false;

            if (CurrentItem.HasGPSPermission)
                return true;

            if (MessageBox.Show("Allow this script to access your position?", CurrentItem.Title, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                return false;
            }

            CurrentItem.HasGPSPermission = true;
            return true;
        }

        private bool GetWebPermission()
        {
            if (CurrentItem == null)
                return false;

            if (CurrentItem.HasWebPermission)
                return true;

            if (MessageBox.Show("Allow this script to access Internet based content?", CurrentItem.Title, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                return false;
            }

            CurrentItem.HasWebPermission = true;
            return true;
        }

        private void SafeUIInvoke(Action toInvoke, bool isImportant = false)
        {
            // we need to keep the number of dispatches to a safe level - otherwise the UI can crawl to a halt
            // - e.g. if too many accelerometer reading
            if (false == isImportant
                && numberOfDispatchesActive >= MaxConcurrentUIDispatchesAllowed)
                return;

            System.Threading.Interlocked.Increment(ref numberOfDispatchesActive);
            Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        toInvoke();
                    }
                    finally
                    {
                        System.Threading.Interlocked.Decrement(ref numberOfDispatchesActive);
                    }
                });
        }

        private Accelerometer accelerometer;
        DateTime timeLastAccelerometerReadingSent = DateTime.MinValue;
        TimeSpan timeBetweenReadings;

        public void StartAccelerometer(double secondsBetweenReadings, string readingCallback)
        {
            if (accelerometer != null)
                return;

            if (secondsBetweenReadings < 0.01)
                secondsBetweenReadings = 0.01;

            timeBetweenReadings = TimeSpan.FromSeconds(secondsBetweenReadings);

            if (accelerometer != null)
                return;

            accelerometer = new Accelerometer();
            accelerometer.Start();
            accelerometer.ReadingChanged += (object sender, AccelerometerReadingEventArgs e) =>
                {
                    // we have to rate limit because the script is too slow
                    DateTime now = DateTime.Now;
                    if (now - timeLastAccelerometerReadingSent < timeBetweenReadings)
                        return;

                    timeLastAccelerometerReadingSent = now;
                    var dict = new Dictionary<string, object>();
                    dict["AccelerometerReading"] = e;

                    SafeUIInvoke(() =>
                    {
                        dict["Orientation"] = Orientation;
                        GenericScriptAction("accelerometer", "accelerometer", null, "reading_changed", readingCallback, dict);
                    }, false);
                };
        }

        public void FixOrientationLandscape()
        {
            this.SupportedOrientations = SupportedPageOrientation.Landscape;
        }

        public void FixOrientationPortrait()
        {
            this.SupportedOrientations = SupportedPageOrientation.Portrait;
        }

        public void CallJsonService(string sender, string url, string callback)
        {
            CallWebServiceInner(sender, url, (string s) =>
                {
                    var dict = new Dictionary<string, object>();
                    dict["Web_response"] = s;
                    dict["Json_response"] = JsonConvert.DeserializeObject(s); // would be nice to use DynamicJson.ConvertJsonStringToObject(s);
                    GenericScriptAction("json_service", sender, null, url, callback, dict);
                });
        }

        public void CallTextWebService(string sender, string url, string callback)
        {
            CallWebServiceInner(sender, url, (string s) =>
                {
                    var dict = new Dictionary<string, object>();
                    dict["Web_response"] = s;
                    GenericScriptAction("web_service", sender, null, url, callback, dict);
                });
        }

        private void CallWebServiceInner(string sender, string url, Action<string> onResponse)
        {
            CallWebServiceInner(sender, url, (Stream stream) =>
                {
                    using (var sr = new StreamReader(stream))
                    {
                        string response = sr.ReadToEnd();
                        SafeUIInvoke(() =>
                        {
                            try
                            {
                                onResponse(response);
                            }
                            catch (Exception exc)
                            {
                                ShowMessageBox("Error seen processing web response " + exc.Message);
                            }
                            finally
                            {
                                HideProgress();
                            }
                        },
                        true);
                    }
                });
        }

        private void CallWebServiceInner(string sender, string url, Action<Stream> onResponse)
        {
            if (false == GetWebPermission())
                return;

            ShowProgress();

            var webRequest = WebRequest.Create(url);
            try
            {
                webRequest.BeginGetResponse((firstResult) =>
                {
                    try
                    {
                        var webResponse = webRequest.EndGetResponse(firstResult);
                        using (var s = webResponse.GetResponseStream())
                        {
                            onResponse(s);
                        }
                    }
                    catch (Exception exc)
                    {
                        ShowMessageBox("Error seen in web response " + exc.Message);
                        SafeUIInvoke(() => { HideProgress(); }
                            , true);
                    }
                }, null);
            }
            catch (Exception exc)
            {
                ShowMessageBox("Error seen in web request " + exc.Message);
                HideProgress();
            }
        }

        #endregion

        #region UIHelper

        private void ShowMessageBox(string message, bool useDispatcher=true)
        {
            if (useDispatcher)
            {
                SafeUIInvoke(() =>
                {
                    MessageBox.Show(message, Constants.Title, MessageBoxButton.OK);
                },
                true);
            }
            else
            {
                MessageBox.Show(message, Constants.Title, MessageBoxButton.OK);
            }
        }

        #endregion

        #region Script Hookup

        private void InitialiseEngine()
        {
            // Allow both portrait and landscape orientations
            SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;

            // Create an IronRuby engine and prevent compilation
            //var setup = new ScriptRuntimeSetup();
            //setup.DebugMode = true;
            //setup.AddRubySetup();
            //var rt = Ruby.CreateRuntime(setup);
            engine = Ruby.CreateEngine((ls) =>
                {
                    ls.ExceptionDetail = true;
                });

            // Load the System.Windows.Media assembly to the IronRuby context
            engine.Runtime.LoadAssembly(typeof(Color).Assembly);
            // Load the Bing Maps assembly to the IronRuby context
            engine.Runtime.LoadAssembly(typeof(Map).Assembly);
            // Load the Phone controls assembly to the IronRuby context
            engine.Runtime.LoadAssembly(typeof(Pivot).Assembly);
            // Load the Phone Toolkit controls assembly to the IronRuby context
            engine.Runtime.LoadAssembly(typeof(Microsoft.Phone.Controls.ToggleSwitch).Assembly);
            // Load the GeoCoordinate assembly - useful if you want to handplot maps
            engine.Runtime.LoadAssembly(typeof(GeoCoordinate).Assembly);

            // TODO - in future need to look at XML and JSON
            // Allow xml parsing
            // engine.Runtime.LoadAssembly(typeof(System.Xml.Linq.XDocument).Assembly);
            // Add a global constant named Phone, which will allow access to this class
            engine.Runtime.Globals.SetVariable("Host", new IronScriptHostProxy(this));

            // Read the IronRuby code
            Assembly execAssembly = Assembly.GetExecutingAssembly();

            // Create the IronRuby scope
            scope = engine.CreateScope();

            GenericScriptAction(string.Empty, null, null, string.Empty, CurrentItem.Code);
        }

        #endregion

        #region the script!

        ScriptEngine engine;
        ScriptScope scope;
        bool executionStopped;

        private void GenericScriptAction(string method, object hint, object sender, string eventName, string ruby, IDictionary<string, object> extraVariables = null)
        {
            if (engine == null)
                return;

            if (executionStopped)
                return;

            engine.Runtime.Globals.SetVariable("Calling_hint", hint);
            engine.Runtime.Globals.SetVariable("Calling_method", method);
            engine.Runtime.Globals.SetVariable("Calling_sender", sender);            
            engine.Runtime.Globals.SetVariable("Calling_event", eventName);

            if (extraVariables != null)
            {
                foreach (var kvp in extraVariables)
                {
                    engine.Runtime.Globals.SetVariable(kvp.Key, kvp.Value);
                }
            }
            try
            {
                engine.Execute(ruby, scope);
            }
            catch (SyntaxErrorException exc)
            {
                var exceptionHelper = new RubySyntaxExceptionHelper(exc);
                ShowScriptErrorMessage(exceptionHelper);
            }
            catch (Exception exc)
            {
                RubyExceptionHelper exceptionHelper = new RubyExceptionHelper(engine, exc);
                ShowScriptErrorMessage(exceptionHelper);
            }
            CheckForSneakyWebControl();
        }

        private void ShowScriptErrorMessage(RubyExceptionHelperBase exceptionHelper)
        {
            var toShow = exceptionHelper.LongErrorText(this.CurrentItem.Code);
            if (MessageBox.Show(toShow, Constants.Title, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                executionStopped = true;
            }
        }

        #endregion

        private void CheckForSneakyWebControl()
        {
            if (true == CurrentItem.HasWebPermission)
                return;

            var qryAllBrowsers = this.LayoutRoot.Descendents().OfType<WebBrowser>();
            if (qryAllBrowsers.Count() == 0)
                return;

            if (false == GetWebPermission())
            {
                executionStopped = true;
                NavigationService.GoBack();
            }
        }
    }
}