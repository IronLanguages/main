from SL_util import *
from fractulator import *
import time

sl = SilverlightDLRTest(2)

def verify_scenario(*args):
    global current_tick
    global current_id
    current_tick += 1
    
    if current_tick < 4 or current_id > 2: 
        return
    
    if current_id == 1:
        for x in ['123', '456', '789']:
            sl.verify_instr(x, HtmlPage.Document.formattedExpression.innerHTML)    
        for x in ['123', '152', '263']:
            sl.verify_instr(x, HtmlPage.Document.result.innerHTML)    
        sl.verify_exact('frac([123,1]) + frac([456,789])', HtmlPage.Document.evalExpression.innerHTML)
        sl.verify_exact('None', HtmlPage.Document.evalException.innerHTML)
        #sl.log_response()
        sl.log_scenario('Try devided by zero')
        current_id += 1
        current_tick = 0
        HtmlPage.Document.input.value = '123 / 456 / 0'
        return
        
    if current_id == 2:
        for x in ['123', '456', '0']:
            sl.verify_instr(x, HtmlPage.Document.formattedExpression.innerHTML)    
        sl.verify_exact('n/a', HtmlPage.Document.result.innerHTML)
        sl.verify_exact('frac([123,1]) / frac([456,1]) / frac([0,1])', HtmlPage.Document.evalExpression.innerHTML)
        # only works if debug pack installed
        if found_debugpack():
            sl.verify_exact('Attempted to divide by zero.', HtmlPage.Document.evalException.innerHTML)
        #sl.log_response()
        current_id += 1
        current_tick = 0
        sl.log_done()
        return

sl.log_scenario('Try a valid expression')
current_id = 1
current_tick = 0
HtmlPage.Document.input.value = '123 + 456/789'

#start timer
tt = DispatcherTimer()
tt.Interval = TimeSpan.FromSeconds(1)
tt.Tick += verify_scenario
tt.Start()

