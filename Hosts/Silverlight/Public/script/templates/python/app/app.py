from System.Windows import Application
from System.Windows.Controls import UserControl

class App:
  def __init__(self):
    root = Application.Current.LoadRootVisual(UserControl(), "app.xaml")
    root.Message.Text = "Welcome to Python and Silverlight!"

App()