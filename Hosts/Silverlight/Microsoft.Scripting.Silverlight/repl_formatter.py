from Microsoft.Scripting.Silverlight import Repl, IReplFormatter

class PythonReplFormatter(IReplFormatter):
    def __init__(self, repl, formatter):
        self.defaultFormatter = formatter

    def PromptElement(self, element):
        element.SetStyleAttribute("color", "yellow")
   
    def PromptHtml(self):
        pass

    def SubPromptHtml(self):
        pass

    def Format(self, obj):
        if obj is None:
            return None
        return "%s" % repr(obj)

def create_repl_formatter(repl, formatter):
    return PythonReplFormatter(repl, formatter)
