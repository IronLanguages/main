s = "\xce\xa3\x1F\xF0\x92\x8D\x85\xCE"

$KCODE = "UTF8"
puts s.inspect
puts s.dump

puts '---'

$KCODE = "SJIS"
puts s.inspect
puts s.dump

puts '---'

s = eval("\"\xce\\xa3b\\xa3\"")
#[s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7]].each { |x| print format("%.2X ", x) }
puts

puts s.inspect
puts s.dump

puts '---'

s = eval("\"\\ucea3\"")

puts s.inspect
puts s.dump

puts '---'

$KCODE = "NONE"
s = eval("\"a\\xce\\xa3b\"")

puts s.inspect
puts s.dump

puts '---'

$KCODE = "UTF8"
begin
  s = eval("\"\\x21\\\"")
rescue Exception
  p $!
end

puts s.inspect
puts s.dump


