using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PhoneScripter {
    public partial class App : Application {

        // Constructor
        public App() {
            UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(Application_UnhandledException);

            InitializeComponent();
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            if (System.Diagnostics.Debugger.IsAttached) {
                // An unhandled exception has occurred, break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}