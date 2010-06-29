import unittest

from System.Windows.Browser.HtmlPage import Document as d
from System.Windows.Browser.HtmlPage import BrowserInformation as browser

class HtmlBridgeRegression(unittest.TestCase):

    def setUp(self):
        self.div = d.CreateElement('div')
        self.div.id = 'testDiv'
        a = d.CreateElement('a')
        a.id = 'a1'
        a.innerHTML = "Link should be enabled..."
        self.div.AppendChild(a)
        span = d.CreateElement('span')
        span.id = 'h3'
        span.innerHTML = 'Needs to be updated'
        span.SetStyleAttribute('background-color', 'pink')
        self.div.AppendChild(span)
        d.Body.AppendChild(self.div)

    def tearDown(self):
        d.Body.RemoveChild(self.div)
        self.div = None

    def test_GetElementById(self):
        elements = ['testDiv', "a1", "h3"]
        for e in elements:
            self.assertFalse(d.GetElementById(e) is None)
    
    def test_GetStyleAttribute(self):
        uas = browser.UserAgent.ToString()
        color = 'rgb(255, 192, 203)' if uas.Contains('AppleWebKit') else 'pink'
        self.assertEqual(
            d.GetElementById('h3').GetStyleAttribute('background-color'),
            color)

    def test_CreateElement(self):
        self.assert_(d.CreateElement('div') != None)
        
    def test_SetAttribute(self):
        new_ctl = d.CreateElement('div')
        new_ctl.SetAttribute("id", "new_ctl")
        self.assertEqual(new_ctl.GetAttribute('id'), "new_ctl")

    def test_SetProperty(self):
        new_ctl = d.CreateElement('div')
        new_value = "This is added by Merlin SL Test!"
        new_ctl.SetProperty("innerHTML", new_value)
        self.assertEqual(new_ctl.GetProperty("innerHTML"), new_value)

    def test_AppendChild(self):
        old_cnt = self.div.Children.Count
        new_ctl = d.CreateElement('div')
        self.div.AppendChild(new_ctl)
        self.assertEqual(self.div.Children.Count, old_cnt + 1)
    
    def test_ChildrenCollection(self):
        self.div.AppendChild(d.CreateElement("div"))
        self.assertEqual(self.div.Children.Count, 3)

