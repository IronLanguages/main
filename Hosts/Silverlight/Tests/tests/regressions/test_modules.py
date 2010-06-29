import unittest
import sys

class ModulesRegression(unittest.TestCase):

    def test_initial(self):
        #Rowan Work Item 227455
        self.assert_('unittest' in sys.modules, "sys.modules not extended")
        self.assertEqual(sys.modules['unittest'], unittest, "sys.modules not extended correctly")

    def test_importstar(self):
        from regressions.fixtures.mod3 import *
        #Make sure mod3 works from this scope
        self.assert_('regressions.fixtures.mod3' in sys.modules, "sys.modules not extended")
        self.assertEquals(proof(), "MOD3", "Rowan Work Item 224943")
        self.assertEquals(proofMod3(), "MOD3", "Rowan Work Item 224943")
    
    def test_import(self):
        import regressions.fixtures.mod1
        self.assertEqual(regressions.fixtures.mod1.proof(), "MOD1")
        self.assertEqual(regressions.fixtures.mod1.proofMod1(), "MOD1")
        self.assert_('regressions.fixtures.mod1' in sys.modules, "sys.modules not extended")
        self.assertEqual(sys.modules['regressions.fixtures.mod1'], regressions.fixtures.mod1)

    def test_fromimportsingle(self):
        from regressions.fixtures.mod2 import proof
        from regressions.fixtures.mod2 import proofMod2
        self.assertEqual(proof(), "MOD2")
        self.assertEqual(proofMod2(), "MOD2")
        self.assert_('regressions.fixtures.mod2' in sys.modules, "sys.modules not extended")

    def test_fromimportmany(self):
        from regressions.fixtures.mod1 import proof, proofMod1
        self.assertEqual(proof(), "MOD1")
        self.assertEqual(proofMod1(), "MOD1")

    def test_fromimportas(self):
        from regressions.fixtures.mod4 import proof as newProof
        self.assertEqual(newProof(), "MOD4")
        self.assert_('regressions.fixtures.mod4' in sys.modules, "sys.modules not extended")
        from regressions.fixtures.mod1 import proof as newProof
        self.assertEqual(newProof(), "MOD1")

