def d s
  a = []
  if !s.nil? then 
    s.each_byte { |b| a << b.to_s(16) } 
  end  
  p a
end

aleph = "\xd7\x90"

puts '-- kcode not used by literal string ctor'

$KCODE = "u"
u = aleph

$KCODE = "n"
a = aleph

/(.)/ =~ u
d $1

/(.)/ =~ a
d $1

puts '-- kcode not used by string ctor'

$KCODE = "u"
u = String.new(aleph)

$KCODE = "n"
a = String.new(aleph)

/(.)/ =~ u
d $1

/(.)/ =~ a
d $1

puts '-- kcode not used by regex ctor'

x = aleph

$KCODE = "u"
ru = /(.)/

$KCODE = "n"
ra = /(.)/

ru =~ x
d $1

ra =~ x
d $1

puts '-- kcode read by regex match'

x = aleph

$KCODE = "u"
/(.)/ =~ x
d $1

$KCODE = "n"
/(.)/ =~ x
d $1

puts '-- regex option --'

/(.)/u =~ x rescue puts $!
d $1

/(.)/n =~ x
d $1

puts '-- kcode read directly, not using globals table'

x = aleph

$k = "u"
alias $old_KCODE $KCODE
alias $KCODE $k

p $old_KCODE
p $KCODE

/(.)/ =~ x
d $1

alias $KCODE $old_KCODE  # restore

puts '-- assignment --'
class C
  def to_s
    "C.new"
  end
  
  def to_str
    "u"
  end
end

def try_set value
  $KCODE = value
rescue 
  puts "#{value.inspect} -> error: #{$!}"  
else
  puts "#{value.inspect} -> #{$KCODE.inspect}"  
end

try_set 1
try_set nil
try_set C.new
try_set ""
try_set "foo"
try_set "a"
try_set "A"
try_set "n"
try_set "N"
try_set "s"
try_set "S"
try_set "e"
try_set "E"
try_set "u"
try_set "U"
try_set "Uxx"
try_set "Exx"
try_set "Sxx"

puts '-- mutability --'
$KCODE = "u"
p $KCODE
($KCODE[0] = 'x') rescue puts $!
p $KCODE

puts '-- KCODE usage --'
x = aleph

$KCODE = "a"
p x.inspect.length
p x.length

$KCODE = "u"
p x.inspect.length
p x.length
