/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IronRuby;
using Microsoft.Phone.Controls;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using System.Runtime.CompilerServices;

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