import unittest
from System.Windows.Controls import TextBlock, Canvas
from System.Windows.Browser.HttpUtility import HtmlEncode
from System.Windows.Browser.HtmlPage import Document, BrowserInformation
from System.Threading.Thread import CurrentThread as thread
from System.Windows.Application import Current as app

class UTF8Regression(unittest.TestCase):

    def setUp(self):
        self.new_ctl = Document.CreateElement('foo')
        self.new_ctl.SetAttribute("id", "newctl")
        self.utf8_string = "aBc 西-雅-图的冬天AbC-123"
        # htmlElement tagName casing varies by browser
        if (BrowserInformation.Name == 'Microsoft Internet Explorer'):
            self.tag = "<B>%s</B>"
        else:
		    self.tag = "<b>%s</b>"
        self.new_value = self.tag % self.utf8_string
        self.new_ctl.innerHTML = self.new_value
        body = Document.Body
        body.AppendChild(self.new_ctl)

        app.RootVisual = Canvas()
        self.tb = TextBlock()
        app.RootVisual.Children.Add(self.tb)
    
    def tearDown(self):
        Document.Body.RemoveChild(self.new_ctl)
        self.new_value = None

        app.RootVisual.Children.Remove(self.tb)
        self.tb = None

    def test_uiculture(self):
        self.assert_(True, 'Current UICulture is "%s"' % thread.CurrentUICulture.DisplayName)

    def test_date(self):
        import datetime
        self.assert_(True, 'Today is "%s"' % datetime.date.today().ctime())

    def test_htmlelement(self):
        self.assert_(True, '完成。')

    def test_children(self):
        self.assertNotEqual(Document.newctl, None, 'Html control is not added')
        actual = Document.newctl.innerHTML
        self.assert_(actual == self.tag % self.utf8_string)
        self.assertEqual(HtmlEncode(self.new_value), HtmlEncode(actual))

    def test_xaml(self):
        '''Create an XAML element that has Unicode string'''
        self.tb.Text = self.new_value
        self.assert_(True, '完成。')

    def test_xaml_children(self):
        '''Verify Children collection returns correctly'''
        self.tb.Text = self.new_value
        tb2 = app.RootVisual.Children[app.RootVisual.Children.Count - 1]
        self.assertEqual(self.new_value, tb2.Text)

