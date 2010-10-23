import sys
from Microsoft.Scripting.Silverlight import Repl
from System.Windows.Browser import HtmlPage

repl, replDiv  = Repl.Create()
HtmlPage.Document.Body.AppendChild(replDiv)
repl.Start()

sys.stdout = repl.OutputBuffer
