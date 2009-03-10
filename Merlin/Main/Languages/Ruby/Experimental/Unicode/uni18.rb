# MRI 1.8: doesn't handle incomplete characters

$KCODE = "UTF-8"

eval <<-EOF

puts "Σ".dump
#puts "Σ\xce".dump
puts "\xe2\x85\x9c".dump
puts "\xe2\x85\x9c".inspect

EOF

puts '---'

begin
  eval <<-EOF
    a = "\xce" 
  EOF
rescue Exception 
  p $!
end

puts '---'

b = "\xa3" 

puts b.dump

puts '---'

puts "\xce#{b}".dump

puts '---'

puts "\xce" "\xa3".dump
puts "abΣ\xce\xa3".dump


