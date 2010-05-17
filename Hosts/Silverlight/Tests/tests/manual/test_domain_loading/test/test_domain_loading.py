from System.Windows.Browser import HtmlPage
from System.Windows.Threading import DispatcherTimer
from System import TimeSpan

# Important variables
TESTCOUNTER = 8 # number of SL instances to be added on page
if HtmlPage.Document.QueryString.ContainsKey('total'):
	try:
		TESTCOUNTER = int(HtmlPage.Document.QueryString['total'])
	except:
		pass

MAXTIMERCOUNT = TESTCOUNTER * 5 # max iteration allowed by timer
TimePerSL = 10 # in sec, 
TimerInterval = 0.5 # in sec

TotalDuration = TimePerSL * TESTCOUNTER

tests = ['test%d' % x for x in range(1, TESTCOUNTER + 1)]
remaining_tests = list(tests) 
start_time = None
iframes = ''

for t in tests:
	# TODO delete any existing files in IsoStore from this test
	
	iframes += '<iframe width="25%%" height="100" id=%s src="sub.html?test=%s"></iframe>' % (t, t)


sl_ctl = HtmlPage.Document.CreateElement("span")
sl_ctl.Id = "test-domain-loading"
sl_ctl.SetProperty("innerHTML", iframes)
HtmlPage.Document.Body.AppendChild(sl_ctl)

ht = DispatcherTimer()
ht.Interval = TimeSpan.FromSeconds(TimerInterval)
timer_counter = 0

def CheckForDonFile(sender, args):
	# check for exception and signal
	global timer_counter
	global tests

	if remaining_tests:
		if timer_counter > MAXTIMERCOUNT - 1:
			print "Time out! %d tests didn't finish." % len(remaining_tests)
			# stop timer
			ht.Stop()
			
		else:
			timer_counter += 1
			for test in tests: 
				# TODO get test file from Iso store
				done_file = None
				if False:
					lines = File.ReadAllLines(done_file)
					done_code = int(lines[0].strip())
					remaining_tests.remove(test)
					if done_code == 0:
						sl.log_info(test + ' done.')
					else:
						sl.log_error(test + ' failed.')

	# are we done?
	if remaining_tests:
		tests = list(remaining_tests)
	else:
		# stop timer
		ht.Stop()

ht.Tick += CheckForDonFile
ht.Start()
HtmlPage.Plugin.SetStyleAttribute("width", "1px")
HtmlPage.Plugin.SetStyleAttribute("height", "1px")