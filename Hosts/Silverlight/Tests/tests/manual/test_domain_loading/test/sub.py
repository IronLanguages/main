from System.Windows.Browser import HtmlPage
test_name = "test1"
if HtmlPage.Document.QueryString.ContainsKey('test'):
	test_name = HtmlPage.Document.QueryString["test"]
e = HtmlPage.Document.CreateElement("h1")
e.innerHTML = test_name
HtmlPage.Document.Body.AppendChild(e)
HtmlPage.Plugin.SetStyleAttribute("width", "1px")
HtmlPage.Plugin.SetStyleAttribute("height", "1px")