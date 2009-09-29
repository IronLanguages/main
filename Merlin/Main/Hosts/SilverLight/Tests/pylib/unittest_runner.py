try:
  import Microsoft.Scripting.Silverlight
  Silverlight = True
except:
  Silverlight = False

import sys
sys.path.append("pylib")

if Silverlight:
  from System.Windows import Application
  from System.Windows.Controls import UserControl
  from Microsoft.Scripting.Silverlight import DynamicApplication, Repl

  engine = DynamicApplication.Current.Engine.Runtime.GetEngine("python")
  repl = Repl.Show(engine, engine.CreateScope())

  sys.stdout = repl.OutputBuffer
  sys.stderr = repl.OutputBuffer

  Application.Current.RootVisual = UserControl()

import unittest 

def run(test_module):
  unittest.main(test_module)
  #suite = unittest.TestLoader().loadTestsFromTestCase(test_sequence_functions.TestSequenceFunctions)
  #unittest.TextTestRunner(verbosity=2).run(suite)
