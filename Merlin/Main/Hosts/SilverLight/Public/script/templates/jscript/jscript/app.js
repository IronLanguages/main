Import("System.Windows.Application")
Import("System.Windows.Controls.UserControl")

App = function() {
  root = Application.Current.LoadRootVisual(new UserControl, "app.xaml")
  root.Message.Text = "Welcome to Managed JScript and Silverlight!"
}

new App
