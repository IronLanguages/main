import sys
import unittest

class SysPathRegression(unittest.TestCase):

    def test_dotnetimport(self):
        sys.path.append("bogus")
        import System
        self.assert_(True)

