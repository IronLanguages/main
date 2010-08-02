import unittest
import re

class RegularExpressionRegression(unittest.TestCase):

    def test_basic(self):
        frp = re.compile(r'old')
        self.assertEqual('Something new', frp.sub('new', 'Something old'))

    def test_search(self):
        self.assertEqual(re.search("(abc){1}", ""), None)
        self.assertEqual(str(re.search("(abc){1}", "abcxyz").span()), '(0, 3)')
        
    def test_match(self):
        self.assertEqual(str(re.match("(abc){1}", "abcxyz", flags=re.L).span()), '(0, 3)')
        
    def test_split(self):
        self.assertEqual(str(re.split("(abc){1}", "abcxyz")), "['', 'abc', 'xyz']")
        
    def test_findall(self):
        self.assertEqual(re.findall("(abc){1}", ""), [])
        self.assertEqual(re.findall("(abc){1}", "abcxyz"), ['abc'])
        self.assertEqual(re.findall("(abc){1}", "abcxyz", re.L), ['abc'])
        self.assertEqual(re.findall("(abc){1}", "abcxyz", flags=re.L), ['abc'])
        self.assertEqual(re.findall("(abc){1}", "xyzabcabc"), ['abc', 'abc'])
        
    def test_sub(self):
        self.assertEqual(re.sub("(abc){1}", "9", "abcd"), "9d")
        self.assertEqual(re.sub("(abc){1}", "abcxyz",'abcd'), "abcxyzd")
        self.assertEqual(re.sub("(abc){1}", "1", "abcd", 0), "1d")
        self.assertEqual(re.sub("(abc){1}", "1", "abcd", count=0), "1d")
        self.assertEqual(re.sub("(abc){1}", "1", "abcdabcd", 1), "1dabcd")
        self.assertEqual(re.sub("(abc){1}", "1", "abcdabcd", 2), "1d1d")
        
    def test_escape(self):
        self.assertEqual(re.escape("abc"), "abc")
        self.assertEqual(re.escape(""), "")
        self.assertEqual(re.escape("_"), "\\_")
        self.assertEqual(re.escape("a_c"), "a\\_c")

