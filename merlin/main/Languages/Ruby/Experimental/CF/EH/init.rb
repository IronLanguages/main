# TODO: X.new nil doesn't work for Ruby exceptions

[
Exception,
Errno::EDOM,
Errno::EINVAL,
Errno::ENOENT,
Errno::ENOTDIR,
Errno::EACCES,
Errno::EEXIST,
NoMemoryError,
ScriptError,
LoadError,
NotImplementedError,
SyntaxError,
# not supported: SignalException,
# not supported: Interrupt,
StandardError,
ArgumentError,
IOError,
EOFError,
IndexError,
LocalJumpError,
NameError,
NoMethodError,
RangeError,
FloatDomainError,
RegexpError,
RuntimeError,
SecurityError,
SystemCallError,
ThreadError,
TypeError,
ZeroDivisionError,
SystemExit,
SystemStackError,
].each { |c|
	puts c.name
	begin
		x = c.new		
	rescue 
		puts "None: Init Error: #{$!}"
	else
	    puts "None: #{x.message}"
	end
	
	begin
		x = c.new nil
	rescue 
		puts "Nil: Init Error: #{$!}"
	else
		puts "Nil: #{x.message}"
	end
	
	begin
	    x = c.new "foo"
	rescue 
		puts "One: Init Error: #{$!}"
	else
		puts "One: #{x.message}"
	end

	puts
}