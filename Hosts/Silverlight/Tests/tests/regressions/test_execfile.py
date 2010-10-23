import unittest

class ExecFileRegression(unittest.TestCase):

    # FIXME: NameError: global name 'proof' is not defined
    def test_definemethodFrom_execfile(self):
        execfile('regressions/fixtures/mod1.py')
        #self.assertEqual(proof(), 'MOD1')
        #self.assertEqual(proof1(), 'MOD1')

    def test_definemethodFrom_exec(self):
        exec "def proof2(): return \"MOD2\"\ndef proof(): return \"MOD2\""
        self.assertEqual(proof(), 'MOD2')
        self.assertEqual(proof2(), 'MOD2')

