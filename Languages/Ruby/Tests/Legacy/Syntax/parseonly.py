import clr
import sys
import System

if System.Environment.Version.Major >=4:
    clr.AddReference("System.Dynamic")
else:
    clr.AddReference("Microsoft.Scripting.Core")
clr.AddReference("Microsoft.Scripting")
clr.AddReference("IronRuby")

from System.IO import File, Directory, Path
from System import Console, String
from Microsoft.Scripting.Hosting import ErrorListener, ScriptRuntime, ScriptRuntimeSetup

def all_files(root):
    for x in Directory.GetFiles(root):
        yield Path.Combine(root, x)
        
    dirs = Directory.GetDirectories(root)
    for d in dirs:
        for x in all_files(d):
            yield x

class Logger(ErrorListener):
    def __init__(self):
        self.error = ""
        
    def ErrorReported(self, source, message, span, errorCode, severity):        
        print self.error
        self.error += String.Format("{0}({1}:{2}): {3}: RB{4}: {5}", source.Path, span.Start.Line, span.Start.Column, severity, errorCode, message)

rb = ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration()).GetEngine("rb")

import re
def extract_expected(content):
    lines = content.split("\n")
    pattern = re.compile(r"^#(.+):(.+)")
    rules = {}
    for l in lines:
        mo = pattern.match(l)
        if mo:
            rules[mo.group(1).strip().lower()] = mo.group(2).strip()
    if rules.has_key("parseonly"):
        return rules["parseonly"]
    return rules["default"]

failures = 0
skips = 0

for f in all_files(sys.argv[1]):
    log = Logger()
    content = File.ReadAllText(f)
    
    if "merlin_bug" in content:
        sys.stdout.write("\ns( %s )" % f)
        skips += 1
        continue 
    
    source = rb.CreateScriptSourceFromString(content)
    try:
		source.Compile(log)
    except:
        failures += 1
        sys.stdout.write("\nC( %s )" % f)
    else:
        actual = log.error
        expected = extract_expected(content)
        
        if expected == "pass":
            if actual == "":
                sys.stdout.write("+")
            else:
                failures += 1
                sys.stdout.write("\nX( %s )" % f)
                sys.stdout.write("\n>> %s | %s" % (expected, actual ))
        else:
            if expected in actual: 
                sys.stdout.write("-")
            else: 
                failures += 1
                sys.stdout.write("\n/( %s )" % f)
                sys.stdout.write("\n>> %s | %s" % (expected, actual ))

print "\n\nfailures: %s, skips: %s \n" % (failures, skips)
sys.exit(failures)


