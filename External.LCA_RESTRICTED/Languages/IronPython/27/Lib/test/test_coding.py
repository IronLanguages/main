
import test.test_support, unittest
import os

class CodingTest(unittest.TestCase):
    def test_bad_coding(self):
        module_name = 'bad_coding'
        self.verify_bad_module(module_name)

    def test_bad_coding2(self):
        module_name = 'bad_coding2'
        self.verify_bad_module(module_name)

    def verify_bad_module(self, module_name):
        self.assertRaises(SyntaxError, __import__, 'test.' + module_name)

        if test.test_support.due_to_ironpython_bug("http://www.codeplex.com/IronPython/WorkItem/View.aspx?WorkItemId=15508"):
            return
        path = os.path.dirname(__file__)
        filename = os.path.join(path, module_name + '.py')
        with open(filename) as fp:
            text = fp.read()
        self.assertRaises(SyntaxError, compile, text, filename, 'exec')

    def test_error_from_string(self):
        # See http://bugs.python.org/issue6289
        if test.test_support.due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/28171"):
            return
        input = u"# coding: ascii\n\N{SNOWMAN}".encode('utf-8')
        with self.assertRaises(SyntaxError) as c:
            compile(input, "<string>", "exec")
        expected = "'ascii' codec can't decode byte 0xe2 in position 16: " \
                   "ordinal not in range(128)"
        self.assertTrue(c.exception.args[0].startswith(expected))


def test_main():
    test.test_support.run_unittest(CodingTest)

if __name__ == "__main__":
    test_main()
