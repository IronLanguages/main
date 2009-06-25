#
# ruby-debug-like-debugger call for things that support 
# System::Diagnostics::Debugger, like Visual Studio
#
def debugger
  require 'mscorlib'
  System::Diagnostics::Debugger.break if System::Diagnostics::Debugger.launch
end