

from System.Windows import *

def sayHello(sender, args):
    textInput = Browser.HtmlPage.Document.GetElementByID("textInput")
    text1.Text = textInput.GetAttribute("value")
    text2.Text = textInput.GetAttribute("value2")

