
def call0():
    for x in 1: print x

for x in range(1, 101):    
    exec("def call%s(): call%s()" % (x, x - 1))

def sayHello(sender, args):
    call100()
