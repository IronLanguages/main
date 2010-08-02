from SL_util import *
import time

sl = SilverlightDLRTest(2)
def verify_1st():
    sl.verify_exact('frac([123,1]) + frac([45,65])', HtmlPage.Document.formattedExpression.innerHTML)
    sl.verify_exact('None', HtmlPage.Document.result.innerHTML)
    sl.verify_exact('frac([123,1]) + frac([45,65])', HtmlPage.Document.evalExpression.innerHTML)
    sl.verify_exact('None', HtmlPage.Document.evalException.innerHTML)
    sl.log_response()
    
def verify_2nd():
    sl.verify_exact('123  ÷   456  ÷   0', HtmlPage.Document.formattedExpression.innerHTML)
    sl.verify_exact('n/a', HtmlPage.Document.result.innerHTML)
    sl.verify_exact('frac([123,1]) / frac([456,1]) / frac([0,1])', HtmlPage.Document.evalExpression.innerHTML)
    sl.verify_exact('Attempted to divide by zero.', HtmlPage.Document.evalException.innerHTML)
    sl.log_response()

sl.log_scenario('Try a valid expression')
#wait for signal
while not get_signal() == '1st Done':
    time.sleep(1)
verify_1st()

sl.log_scenario('Try devided by zero')
#wait for signal
while not get_signal() == '2nd Done':
    time.sleep(1)
verify_2nd()

sl.log_done()