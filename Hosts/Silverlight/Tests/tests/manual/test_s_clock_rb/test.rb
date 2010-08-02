require 'app'

# Below are added for testing purpose

load_assembly 'System.Windows.Browser'
include System::Windows::Browser
s = HtmlPage.Document.GetElementById('slTestSignal')
s.SetProperty('innerHTML', 'Done')