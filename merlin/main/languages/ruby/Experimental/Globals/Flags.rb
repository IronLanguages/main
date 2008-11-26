class C
end

$DEBUG = C.new
p $DEBUG

p $VERBOSE

$VERBOSE = C.new
p $VERBOSE

$VERBOSE = 0
p $VERBOSE

$VERBOSE = false
p $VERBOSE

$VERBOSE = true
p $VERBOSE

$VERBOSE = nil
p $VERBOSE