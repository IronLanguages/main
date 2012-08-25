using IronRuby;
using Microsoft.Scripting.Hosting;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IronApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ScriptEngine engine;
        private ScriptScope scope;

        public MainPage()
        {
            this.InitializeComponent();

            var setup = new ScriptRuntimeSetup();
            setup.HostType = typeof(DlrHost);
            setup.AddRubySetup();

            var runtime = Ruby.CreateRuntime(setup);
            this.engine = Ruby.GetEngine(runtime);
            this.scope = engine.CreateScope();
            this.scope.SetVariable("Main", this);

            runtime.LoadAssembly(typeof(object).GetTypeInfo().Assembly);
            runtime.LoadAssembly(typeof(TextSetOptions).GetTypeInfo().Assembly);
            runtime.LoadAssembly(typeof(TextAlignment).GetTypeInfo().Assembly);

            string outputText = @"
box = main.find_name('OutputBox')
p box.text_alignment
box.text_alignment = Windows::UI::Xaml::TextAlignment.Right
p box.text_alignment
";
            InputBox.IsSpellCheckEnabled = false;
            OutputBox.IsSpellCheckEnabled = false;
            InputBox.Document.SetText(TextSetOptions.None, outputText);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string text;
            string outputText;
            InputBox.Document.GetText(TextGetOptions.UseCrlf, out text);
            object result;
            using (var stream = new MemoryStream())
            {
                engine.Runtime.IO.SetOutput(stream, Encoding.UTF8);
                try
                {
                    result = engine.Execute<object>(text, scope);
                }
                catch (Exception ex)
                {
                    result = ex;
                }

                var a = stream.ToArray();
                outputText = Encoding.UTF8.GetString(a, 0, a.Length);
            }

            if (result != null)
            {
                outputText += result.ToString();
            }

            OutputBox.Document.SetText(TextSetOptions.None, outputText);
        }
    }
}
