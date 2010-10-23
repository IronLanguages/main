from System import Uri, AsyncCallback
from System.IO import StreamReader
from System.Net import WebRequest
from System.Windows.Browser import HtmlPage
import time
import unittest

from System.Windows.Threading import Dispatcher
from System.Threading import ManualResetEvent, Thread, ThreadStart
are = ManualResetEvent(False)

TEST_NET_CONTENT = '[[[CONTENT OF TEST_NET.TXT]]]'

def puts(*a):
    print(a)

def web_request(obj, uri_string, func):
    req = WebRequest.Create(Uri(uri_string))
    req.BeginGetResponse(AsyncCallback(web_complete(obj, func)), req)

def web_complete(obj, func):
    def __web_complete(a):
        req = a.AsyncState
        res = req.EndGetResponse(a)
        content = StreamReader(res.GetResponseStream()).ReadToEnd()
        func(content)
        are.Set()
    return __web_complete

class NetRegression(unittest.TestCase):

    def test_net_local(self):
        thread = Thread(ThreadStart(self.do_test_net_local))
        thread.Start()
        are.WaitOne(5000)

    def do_test_net_local(self):
        HtmlPage.Dispatcher.BeginInvoke(
          lambda: web_request(self, Uri(HtmlPage.Document.DocumentUri, 'tests/regressions/fixtures/test_net.txt').ToString(), self.verify_test_net_local)
        )

    def verify_test_net_local(self, result):
        self.assertEqual(result, TEST_NET_CONTENT)

#sl.log_scenario('url on app on different hosts, fails in any case ***')
#CallNet('http://ironpython/silverlight-samples/test_net.txt', False)
#
#sl.log_scenario('url to same app, expect to work on http request')
#CallNet(Uri(HtmlPage.Document.DocumentUri, 'test_net.txt').AbsoluteUri, True)
#
#sl.log_scenario('pysical path to same app, only works on file share ***')
#CallNet('file:///C:/inetpub/wwwroot/silverlighttestapp/test_net/test_net.txt', False)
#
##sl.log_info(dir(HtmlPage.DocumentUri))
##CallNet(HtmlPage.DocumentUri.get_LocalPath(), False)
#
##sl.log_scenario('url to child app on same server, works for http  ***')
##CallNet('http://ironpython/networkingservice/test_net.txt')
#
##sl.log_scenario('url to different site on same server, works for http  ***')
##CallNet('http://localhost/files/test_net.txt')
#
#sl.log_done()
#
