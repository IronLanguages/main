
class C: 
    def __init__(self, sender, args):
        text1.Text = "Hello world from __init__"

class D:
    def __call__(self, sender, args):
        text2.Text = "Hello world from __call__"

d = D()
