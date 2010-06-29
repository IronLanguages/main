from SL_util import *
QueryString = HtmlPage.Document.QueryString

def run_tests(tests):
    test_count = len(tests)
    sl = SilverlightDLRTest(test_count)

    class ipy_test_ouput_reader:
        def write(self, text):
            #ipy tests has lots of empty lines output, ignore them
            if not text.strip() == '':
                sl.log_msg_in_color(text, 'black')

    sys.stdout = ipy_test_ouput_reader()
    sys.stderr = ipy_test_ouput_reader()
    for t in tests:
        sl.log_scenario("Running %s ..." % t)
            
        #Import the test module - i.e., run the test
        if t[-3:].lower() == '.py':    
            t = t[:-3]

        try:
            __import__(t)
            sl.log_pass("Done.")
            
        except SystemExit, ex:
            if ex.code == 0:
                sl.log_pass("Done (test called sys.exit with status 0)")
            else:
                sl.log_error("Test exited with status %d" % ex.code)
        except Exception, e:
            sl.log_exception(e)
    sl.log_done()
    
# test name(s) is query string 'test', separated by ';':
if not QueryString.ContainsKey('test') or QueryString['test'] == '':
    sl = SilverlightDLRTest(1)
    sl.log_error("No test name given!")
    sl.log_done()
else:
    test_names = QueryString['test']
    if test_names[0]==';': test_names = test_names[1:]
    run_tests(test_names.split(';'))
    

