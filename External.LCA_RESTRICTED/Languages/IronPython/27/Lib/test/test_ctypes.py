import unittest

from test.test_support import run_unittest, import_module, due_to_ironpython_bug
#Skip tests if _ctypes module does not exist
import_module('_ctypes')

if due_to_ironpython_bug("http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22393"):
    import System, nt
    try:
        System.IO.File.Copy(nt.getcwd() + r"\..\Python27.dll", nt.getcwd() + r"\Python27.dll", True)
    except:
        pass


IRONPYTHON_DISABLED_LIST = [
                    #ERROR
                    "ctypes.test.test_as_parameter.AsParamPropertyWrapperTestCase.test_callbacks",
                    "ctypes.test.test_byteswap.Test.test_endian_double",
                    "ctypes.test.test_byteswap.Test.test_endian_float",
                    "ctypes.test.test_byteswap.Test.test_endian_int",
                    "ctypes.test.test_byteswap.Test.test_endian_longlong",
                    "ctypes.test.test_byteswap.Test.test_endian_short",
                    "ctypes.test.test_byteswap.Test.test_struct_fields_1",
                    "ctypes.test.test_byteswap.Test.test_struct_fields_2",
                    "ctypes.test.test_byteswap.Test.test_unaligned_nonnative_struct_fields",
                    "ctypes.test.test_errno.Test.test_GetLastError",
                    "ctypes.test.test_errno.Test.test_open",
                    "ctypes.test.test_numbers.NumberTestCase.test_init",
                    "ctypes.test.test_parameters.SimpleTypesTestCase.test_noctypes_argtype",
                    "ctypes.test.test_pep3118.Test.test_endian_types",
                    "ctypes.test.test_pickling.PickleTest.test_simple",
                    "ctypes.test.test_pickling.PickleTest.test_struct",
                    "ctypes.test.test_pickling.PickleTest.test_unpickable",
                    "ctypes.test.test_pickling.PickleTest.test_wchar",
                    "ctypes.test.test_pickling.PickleTest_1.test_simple",
                    "ctypes.test.test_pickling.PickleTest_1.test_struct",
                    "ctypes.test.test_pickling.PickleTest_1.test_unpickable",
                    "ctypes.test.test_pickling.PickleTest_1.test_wchar",
                    "ctypes.test.test_pickling.PickleTest_2.test_simple",
                    "ctypes.test.test_pickling.PickleTest_2.test_wchar",
                    "ctypes.test.test_pointers.PointersTestCase.test_c_void_p",
                    "ctypes.test.test_prototypes.CharPointersTestCase.test_POINTER_c_char_arg",
                    "ctypes.test.test_prototypes.CharPointersTestCase.test_c_char_p_arg",
                    "ctypes.test.test_prototypes.CharPointersTestCase.test_instance",
                    "ctypes.test.test_prototypes.CharPointersTestCase.test_int_pointer_arg",
                    "ctypes.test.test_prototypes.CharPointersTestCase.test_paramflags",
                    "ctypes.test.test_prototypes.WCharPointersTestCase.test_POINTER_c_wchar_arg",
                    "ctypes.test.test_slicing.SlicesTestCase.test_char_ptr",
                    "ctypes.test.test_slicing.SlicesTestCase.test_char_ptr_with_free",
                    "ctypes.test.test_slicing.SlicesTestCase.test_wchar_ptr",
                    #FAIL
                    "ctypes.test.test_parameters.SimpleTypesTestCase.test_byref_pointer",
                    "ctypes.test.test_parameters.SimpleTypesTestCase.test_byref_pointerpointer",
                    "ctypes.test.test_parameters.SimpleTypesTestCase.test_cstrings",
                    "ctypes.test.test_parameters.SimpleTypesTestCase.test_cw_strings",
                    "ctypes.test.test_pickling.PickleTest_2.test_struct",
                    "ctypes.test.test_pickling.PickleTest_2.test_unpickable",
                    "ctypes.test.test_pointers.PointersTestCase.test_pointer_crash",
                    "ctypes.test.test_prototypes.CharPointersTestCase.test_c_void_p_arg",
                    "ctypes.test.test_prototypes.WCharPointersTestCase.test_c_wchar_p_arg",
                    "ctypes.test.test_unicode.StringTestCase.test_ascii_ignore",
                    "ctypes.test.test_unicode.StringTestCase.test_ascii_replace",
                    "ctypes.test.test_unicode.StringTestCase.test_ascii_strict",
                    "ctypes.test.test_unicode.StringTestCase.test_buffers",
                    "ctypes.test.test_unicode.UnicodeTestCase.test_ascii_ignore",
                    "ctypes.test.test_unicode.UnicodeTestCase.test_ascii_strict",
                    "ctypes.test.test_unicode.UnicodeTestCase.test_buffers",
                    ]

def test_main():
    import ctypes.test
    skipped, testcases = ctypes.test.get_tests(ctypes.test, "test_*.py", verbosity=0)
    suites = [unittest.makeSuite(t) for t in testcases]
    
    if due_to_ironpython_bug("http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=374"):
        for suite in suites:
            length = len(suite._tests)
            i = 0
            while i < length:
                if suite._tests[i].id() in IRONPYTHON_DISABLED_LIST:
                    suite._tests.pop(i)
                    i -= 1
                    length -= 1
                i += 1
    
    try:
        run_unittest(unittest.TestSuite(suites))
    finally:
        if due_to_ironpython_bug("http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22393"):
            try:
                System.IO.File.Delete(nt.getcwd() + r"\Python26.dll")
            except:
                pass
            print "%d of these test cases were disabled under IronPython." % len(IRONPYTHON_DISABLED_LIST)

if __name__ == "__main__":
    test_main()
