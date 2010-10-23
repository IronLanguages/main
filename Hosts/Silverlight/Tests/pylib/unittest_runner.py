try:
  import Microsoft.Scripting.Silverlight
  Silverlight = True
except:
  Silverlight = False

import sys
sys.path.append("pylib")

if Silverlight:
  from System.Windows import Application, Deployment
  from System.Windows.Browser import HtmlPage, HttpUtility
  from System.Windows.Controls import UserControl
  from Microsoft.Scripting.Silverlight import DynamicApplication, Repl

  repl = None

  def start_repl():
    global repl
    engine = DynamicApplication.Current.Engine.Runtime.GetEngine("python")
    repl = Repl.Show(engine, engine.CreateScope())
    sys.stdout = repl.OutputBuffer
    sys.stderr = repl.OutputBuffer

  d = Deployment.Current.Dispatcher
  if d.CheckAccess():
    start_repl()
  else:
    d.BeginInvoke(start_repl)

  Application.Current.RootVisual = UserControl()

import unittest

from System.Collections.Generic import Dictionary
results = None

class UnitTestResults(object):
  def __init__(self, unittest_results):
    global results
    results = unittest_results
    self.unittest_results = unittest_results
    self.__runtime = DynamicApplication.Current.Runtime
    self.__runtime.UseFile('rblib/test_results.rb')
    self.test_results = self.__runtime.Globals.TestResults
  
  def broadcast(self):
    r = self.get_results()
    self.test_results.broadcast(r, self.passed(r), self.output())

  def get_results(self):
    r = { 'all': self.unittest_results.testsRun,
          'errors': len(self.unittest_results.errors),
          'failures': len(self.unittest_results.failures) }
    d = Dictionary[str, object]()
    for kv in r: d.Add(kv, r[kv])
    return d
  
  def passed(self, results):
    return self.unittest_results.wasSuccessful()
    
  def output(self):
    global repl
    output_element = repl.Output
    if output_element is not None:
      return output_element.innerHTML

def report_results(results):
  UnitTestResults(results).broadcast()

def run(test_module):
  unittest.main(test_module)
  #suite = unittest.TestLoader().loadTestsFromTestCase(test_sequence_functions.TestSequenceFunctions)
  #unittest.TextTestRunner(verbosity=2).run(suite)

def get_testcase(common_module, module_name, class_name):
  return __import__("%s.%s" % (common_module, module_name)).__dict__[module_name].__dict__[class_name]

def load_testcases(module_name, testcases):
  return [unittest.TestLoader().loadTestsFromTestCase(get_testcase(module_name, *m)) for m in testcases]

def run_testcases(module_name, testcases):
  results = run_suites(load_testcases(module_name, testcases))
  report_results(results)
  return results

def run_suites(all_suites):
  return unittest.TextTestRunner(verbosity=2).run(unittest.TestSuite(all_suites))
  