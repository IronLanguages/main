$KCODE = "UTF-8"
u = eval("'Σ'")

$KCODE = "NONE"
b = eval("'Σ'")

$KCODE = "UTF-8"
p b

$KCODE = "NONE"
p u

puts '-----'

$KCODE = "UTF-8"
eval("
Σ = 1
p defined?(Σ)
")

puts '-----'

$KCODE = "NONE"
begin
  eval("xΣx = 1")
rescue SyntaxError
  p $!
end  


puts '-----'

# invalid multi-byte character:
$KCODE = "UTF-8"
p u2 = eval("'\xce' #xxx '")

# regex:
$KCODE = "UTF-8"
p eval("/hello/")
p eval("/hΣllo/")
p /hello/
p /hΣllo/
p r1 = Regexp.new("hello")
p r2 = Regexp.new("hΣllo")

$KCODE = "NONE"
p r1,r2



