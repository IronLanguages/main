import unittest
from ctypes import *
from ctypes import wintypes

class TestCase(unittest.TestCase):
    # IronPython added test case
    def test_variant_bool(self):
        # reads 16-bits from memory, anything non-zero is True
        for true_value in (1, 32767, 32768, 65535, 65537):
            true = POINTER(c_int16)(c_int16(true_value))
            value = cast(true, POINTER(wintypes.VARIANT_BOOL))
            self.failUnlessEqual(repr(value.contents), 'VARIANT_BOOL(True)')
        
            vb = wintypes.VARIANT_BOOL()
            self.failUnlessEqual(vb.value, False)
            vb.value = True
            self.failUnlessEqual(vb.value, True)
            vb.value = true_value
            self.failUnlessEqual(vb.value, True)
        
        for false_value in (0, 65536, 262144, 2**33):
            false = POINTER(c_int16)(c_int16(false_value))
            value = cast(false, POINTER(wintypes.VARIANT_BOOL))
            self.failUnlessEqual(repr(value.contents), 'VARIANT_BOOL(False)')

        # allow any bool conversion on assignment to value
        for set_value in (65536, 262144, 2**33):
            vb = wintypes.VARIANT_BOOL()
            vb.value = set_value
            self.failUnlessEqual(vb.value, True)
        
        vb = wintypes.VARIANT_BOOL()
        vb.value = [2,3]
        self.failUnlessEqual(vb.value, True)
        vb.value = []
        self.failUnlessEqual(vb.value, False)
        
if __name__ == "__main__":
    unittest.main()
