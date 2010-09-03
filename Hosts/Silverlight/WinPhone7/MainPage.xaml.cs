using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using IronRuby;
using Microsoft.Scripting.Hosting;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using System;
using Microsoft.Scripting.Hosting.Providers;
using IronRuby.Runtime;
using System.Diagnostics;

namespace PhoneScripter {
    public partial class MainPage : PhoneApplicationPage {
        private readonly ScriptEngine _engine;

        public MainPage() {
            InitializeComponent();
            SupportedOrientations = SupportedPageOrientation.Portrait;

            _engine = Ruby.CreateEngine();
            _engine.Runtime.LoadAssembly(typeof(Color).Assembly);
            _engine.Runtime.Globals.SetVariable("Phone", this);

            Input.Text = 
@"include System::Windows::Media

color = Color.from_argb(0xff, 0xa0, 0, 0)

Phone.output_box.background = 
   SolidColorBrush.new(color)

def fact n
  if n <= 1 then 1 else n * fact(n-1) end
end

10.times { |i| puts fact(i) }
";
        }

        public TextBox OutputBox {
            get { return Output; }
        }

        private void Run_Click(object sender, RoutedEventArgs e) {
            MemoryStream stream = new MemoryStream();            
            _engine.Runtime.IO.SetOutput(stream, Encoding.UTF8);
                        
            try {
                try {
                    _engine.Execute(Input.Text);
                } finally {
                    byte[] bytes = stream.ToArray();
                    Output.Text += Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                }
            } catch (Exception ex) {
                Output.Text += ex.Message;
            }
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e) {
            Output.Text = "";
            Output.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));
        }
    }
}