import sys
import thread
import unittest

class ThreadRegression(unittest.TestCase):

    def runthread(self, function, *args):
        import time
        
        thread_info = [None, None]
        
        def testthread(*args2):
            thread_info[1] = function(*args2)
            thread_info[0] = thread.get_ident()
        
        thread.start_new_thread(testthread, args)
        
        start = time.time()
        end = start + 10 # seconds
        while not thread_info[0] and time.time() < end:
            pass
        
        threadid = thread_info[0]
        result = thread_info[1]
        if not result or not threadid:
            self.fail('thread timed out without returning a result')

        # verify that we didn't execute on the UI thread
        sl.verify_not_exact(thread.get_ident(), threadid)

        return result

    def raise_exception(self):
        raise StandardError("this is an exception that should be swallowed")
    
    def background_import(self):
        if 'System' in sys.modules: sys.modules.pop('System')
        from System import Math
        return Math.Floor(4.4)
    
    def test_backgroundthreadthrow(self):
        self.runthread(self.raise_exception)
        self.assert_(True)
    
    def test_import(self):
        self.assertEqual(4.0, runthread(self.background_import))

