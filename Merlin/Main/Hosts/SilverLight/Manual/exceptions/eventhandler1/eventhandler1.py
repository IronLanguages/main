
def sayHello(sender, args):
    text1.Text = "Move mouse over me, it should throw"
    text1.MouseEnter += handleMouseEnter
    text1.MouseLeave += handleMouseLeave

def handleMouseEnter(sender, args):
    # there is no "Anything"
    text1.FontSize = "30"
    #text1.FontSize = text1.Anything

def handleMouseLeave(sender, args):
    text1.Text = "I just left without trouble"
