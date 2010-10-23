from System.Windows import Application
from System.Windows.Controls import UserControl

import sys
from Microsoft.Scripting.Silverlight import Repl
repl = Repl.Show()
sys.stdout = repl.OutputBuffer

class App:
  def __init__(self):
    root = Application.Current.LoadRootVisual(UserControl(), "app.xaml")
    root.Message.Text = "Welcome to Python and Silverlight!"

App()