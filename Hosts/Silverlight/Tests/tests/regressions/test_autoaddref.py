import unittest

class AutoAddRefRegression(unittest.TestCase):
    
    def test_referencecount(self):
        import clr
        self.assert_(len(clr.References) >= 6)

