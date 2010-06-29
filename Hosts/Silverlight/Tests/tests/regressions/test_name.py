import unittest

class NameRegression(unittest.TestCase):

  def test___name__(self):
      self.assertEqual(__name__, 'regressions.test_name')

  def test___name__inmodule(self):
      from fixtures.child1 import child1_name
      self.assertNotEqual(child1_name(), '__main__')

