import unittest

class MultiImportRegression(unittest.TestCase):

    def test_import_standalone(self):
        import sys

    def test_import_from_method(self):
        def user_function():
            import sys
        user_function()

    def test_import_from_module(self):
        from fixtures.multiimport import *
        self.assert_(is_py_loaded)

