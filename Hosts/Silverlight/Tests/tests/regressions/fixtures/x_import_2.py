import sys
sys.path.append('tests/regressions/fixtures/')
# expect exception
from module_with_syntaxerror import *
