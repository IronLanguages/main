def call0():
    for x in 1: print x

for x in range(1, 1001):    
    exec("def call%s(): call%s()" % (x, x - 1))

import unittest

class TypeErrorRegression(unittest.TestCase):
    pass

  # This is a negative test that tested the UI error msg -- should be ported to JS
  #def test_typeerror_thrown_for_iterating(self):
  #  try:
  #    call100()
  #  except TypeError, e:
  #    # TODO verify that the exception matches these conditions:
  #    exp = {
  #        'type': 'TypeError',
  #        'exp.error_message': "TypeError: iteration over non-sequence of type int",
  #        'exp.error_source_file': "test.py",
  #        'exp.error_source_line': 4,
  #        'exp.error_stack': [
  #            'at call0 in test.py, line 4'] + [
  #            'at call%d in <string>, line 1' % x for x in range(1, 100)
  #        ]
  #    }
  #    self.fail()

