require 'mock'

p $VERBOSE, $DEBUG

putc I.new(64)

class C
  def initialize open, close
    @out = IO.new(1, "w")
    @open = open
    @close = close
  end

  def write x
    @out << @open
    @out << x
    @out << @close
    @out.flush
  end
end

$out = $stdout
$stderr = C.new '<', '>'
$stdout = C.new '[', ']'

def f *a; end
        
puts 'bar'

# syntax warnings:
eval('f f 1')
eval('f (1)')
eval('f ()')
begin
  eval("?\n")
rescue SyntaxError
end  

# runtime warnings:
warn S.new("string warning")
ENV.indices
"".type

$VERBOSE = false
warn "verbose = #$VERBOSE"

$VERBOSE = true
warn "verbose = #$VERBOSE"

$VERBOSE = nil
warn "verbose = #$VERBOSE"  # no warning reported

$XXX = true
alias $VERBOSE $XXX
puts $VERBOSE
warn "verbose = #$VERBOSE"  # no warning reported

