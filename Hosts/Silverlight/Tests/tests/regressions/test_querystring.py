import unittest

class QueryStringRegression(unittest.TestCase):
    
    def test_querystring(self):
        # ?a=one&b=silverlight%20test&a=2&abc&=three&=four
        expected_dic = {'a':2, 'b':'silverlight test', 'abc':'', '':'four'}
        for p in HtmlPage.Document.QueryString:
            self.assert_(expected_dic.has_key(p.Key))
            self.assertEqual(str(expected_dic[p.Key]), str(p.Value))
        self.assertEqual(len(expected_dic), HtmlPage.Document.QueryString.Count)

