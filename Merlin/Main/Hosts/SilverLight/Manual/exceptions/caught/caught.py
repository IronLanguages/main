
def raiseException():
    raise AssertionError

def sayHello(sender, args):
    try:
        raiseException()
    except:
        text1.Text = "First exception caught\n"

    text1.Text += "Pass\n"

    try:
        raise "string"
    except:
        text1.Text += "Second exception caught\n"


