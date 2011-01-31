import sympl
s = sympl.Sympl()

feo = s.ExecuteFile(r"..\examples\test.sympl")

print "ExecuteExpr ... ",
s.ExecuteExpr("(print 5)", feo)

import sys

if "norepl" in sys.argv: sys.exit(0)

### Quicky REPL
###
import clr
from System import Console
from System.IO import StringReader

input = None
exprstr = ""
prompt = ">>> "
print "\n"*3
print "Enter expressions.  Enter blank line to abort input."
print "Enter 'exit (the symbol) to exit."
print "\n"
while True:
    print prompt,
    input = Console.ReadLine()
    if (input == ""):
        exprstr = ""
        prompt = ">>> "
        continue
    else:
        exprstr = exprstr + " " + input
    ## See if we have complete input.
    try:
        sympl.parser.ParseExpr(StringReader(exprstr))
    except Exception, e:
        prompt = "... "
        continue
    ## We do, so execute.
    try:
        res = s.ExecuteExpr(exprstr, feo)
        exprstr = ""
        prompt = ">>> "
        if res is s.MakeSymbol("exit"): break
        print res
    except Exception, e:
        exprstr = ""
        prompt = ">>> "
        Console.Write("ERROR: ");
        Console.WriteLine(e);
