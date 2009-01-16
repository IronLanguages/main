
def sayHello(sender, args):
    text1.Text = "Move mouse over me, it should throw"
    text1.MouseEnter += handleMouseEnter
    text1.MouseLeave += handleMouseLeave

def handleMouseEnter(sender, args, arg2):
    text1.Text = "Entered"

def handleMouseLeave(sender, args):
    text1.Text = "I just left without trouble. "
