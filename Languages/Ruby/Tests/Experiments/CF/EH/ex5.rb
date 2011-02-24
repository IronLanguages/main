require 'socket'

p Exception.allocate(*[])

Exception.allocate("foo") rescue p $!

p Exception.new
p Exception.new(nil)
p Exception.new(Object.new)
p Exception.new(123)
p Exception.new("foo")  
 
[
Exception, 
NoMemoryError,
EOFError,
FloatDomainError,
RuntimeError,
ThreadError,
SystemExit,
SignalException,
Interrupt,
LocalJumpError,
ScriptError,
NotImplementedError,
LoadError,
RegexpError,
SyntaxError,
SystemStackError,
StandardError,
ArgumentError,
IOError,
IndexError,
RangeError,
NameError,
NoMethodError,
SecurityError,
TypeError,
ZeroDivisionError,
SystemCallError
].each do |e|
  puts "- #{e} ----------------------"
  
  p e.new.message rescue p $!
  p e.new(nil).message rescue p $!
  p e.new("foo").message rescue p $!
  p e.new(Object.new).message rescue p $!
end

