import sys
sys.path.append('tests/regressions/fixtures/')

def f1():
    from test2 import *

f1()
