# TODO: port this test to System.Net.HttpWebRequest
from SL_util import *
sl = SilverlightDLRTest(0)
sl.log_done()

# old test
"""
    from SL_util import *
    from System.Windows.Browser.Net import XBrowserHttpWebRequest
    from System.IO import StreamReader
    
    TEST_NET_CONTENT = '[[[CONTENT OF TEST_NET.TXT]]]'
    
    sl = SilverlightDLRTest(3)
    
    def CallNet(url_in, expected_work):
        try:
            sl.log_info('constructing uri for %s' % url_in)
            addr = Uri(url_in)
    
            sl.log_info('constructing BrowserHttpWebRequest')
            req = XBrowserHttpWebRequest(addr)
    
            sl.log_info('calling GetResponse')
            resp = req.GetResponse()
    
            sl.log_info('reading response stream')
            stream = resp.GetResponseStream()
            result = StreamReader(stream).ReadToEnd()
    
            if not expected_work:
                sl.log_error('*** Did not get exception ***')
                sl.log_info('Got response: %s' % result)
            else:
                sl.log_info('result is ready...')
                sl.verify_exact(TEST_NET_CONTENT, result)
    
        except Exception, e:
            if expected_work:
                sl.log_error('*** Did not get response *** <pre>' + str(e) + '</pre>')
            else:
                sl.verify_exception('Cross domain calls are not supported by BrowserHttpWebRequest.', e)
    
    sl.log_scenario('url on app on different hosts, fails in any case ***')
    CallNet('http://ironpython/silverlight-samples/test_net.txt', False)
    
    sl.log_scenario('url to same app, expect to work on http request')
    CallNet(Uri(HtmlPage.Document.DocumentUri, 'test_net.txt').AbsoluteUri, True)
    
    sl.log_scenario('pysical path to same app, only works on file share ***')
    CallNet('file:///C:/inetpub/wwwroot/silverlighttestapp/test_net/test_net.txt', False)
    
    #sl.log_info(dir(HtmlPage.DocumentUri))
    #CallNet(HtmlPage.DocumentUri.get_LocalPath(), False)
    
    #sl.log_scenario('url to child app on same server, works for http  ***')
    #CallNet('http://ironpython/networkingservice/test_net.txt')
    
    #sl.log_scenario('url to different site on same server, works for http  ***')
    #CallNet('http://localhost/files/test_net.txt')
    
    sl.log_done()
"""