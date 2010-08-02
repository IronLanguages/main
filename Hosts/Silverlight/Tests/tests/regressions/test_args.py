# Verify bug 229053

import unittest

class ArgsRegression(unittest.TestCase):
    
    def test_exec_and_splat_args(self):
        exec 'def f(*args): return repr(args)'
        self.assertEqual('(1, 2, 3, 4)', f(1,2,3,4))

