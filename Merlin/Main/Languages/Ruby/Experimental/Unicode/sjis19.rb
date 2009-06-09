# encoding: SJIS

eval('# encoding: BINARY
p __ENCODING__
')

puts "- e/d/i -"

puts "\202\240".encoding rescue 0
puts "\202\240".dump
puts "\202\240".inspect

puts "- e/d/i -"

puts "\316".encoding rescue 0
puts "\316".dump
puts "\316".inspect

puts "- e/d/i -"

begin
  eval <<-'EOT'
    s = "\u{03a3}"
  EOT
  puts s.encoding rescue p $!
  puts s.dump
  puts s.inspect
rescue Exception
  p $!
end
  
puts "- e/d/i -"

begin
  eval <<-'EOT'
    s = "\x80\u{03a3}"
  EOT
rescue Exception
  p $!
end

puts "- e/d/i -"

begin
  eval <<-'EOT'
    s = "あ" "\u{03a3}"
  EOT
rescue Exception
  p $!
end

puts "- e/d/i -"




