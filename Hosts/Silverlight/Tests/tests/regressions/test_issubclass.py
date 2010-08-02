import unittest

import clr
import System

class IsSubClassRegression(unittest.TestCase):
    
    def x(self):
        return 2
    def y(self):
        return 'abc'

    def test_same_class(self):
        self.assertFalse(self.x().GetType().IsSubclassOf(System.Int32))
        self.assertFalse(self.y().GetType().IsSubclassOf(System.String))

    def test_nonparent_class(self):
        self.assertFalse(self.y().GetType().IsSubclassOf(System.Array))
        self.assertFalse(self.y().GetType().IsSubclassOf(System.Int32))

    def test_ancestor(self):
        self.assert_(self.y().GetType().IsSubclassOf(System.Object))
        self.assert_(self.y().GetType().IsSubclassOf(System.Object))

