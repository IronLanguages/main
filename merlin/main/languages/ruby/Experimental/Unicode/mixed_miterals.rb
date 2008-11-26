#coding: UTF-8

#p <<Σ
#foo
#Σ

#Σ = 1
#puts Σ

puts '-- 0.0 --'

x = "\200\201"
p x.encoding
p x[0],x[1],x[2]

puts '-- 0.1 --'

x = "\200Σ\201"
p x.encoding
p x[0],x[1],x[2]

puts '-- 0.2 --'

x = "\xCE\xA3"
p x.encoding
p x[0],x[1],x[2]

puts '-- 0.3 --'

x = "Σ\xCE\xA3Σ"
p x.encoding
p x[0],x[1],x[2]

puts '-- 0.4 --'

x = "Σ"
p x.encoding
p x[0],x[1],x[2]

puts '-- 1 --'

begin
  eval('"\200\u0343\201"')
rescue Exception
  p $!
end

puts '-- 2 --'

begin
  eval('"\x81\u0343\x82"') 
rescue Exception
  p $!
end

puts '-- 3 --'

begin
  eval('"\x81" "\u0343" "\x82"') 
rescue Exception
  p $!
end

puts '-- 4 --'

begin
  p "\x81" "\x82"
rescue Exception
  p $!
end

puts '-- 5 --'

a = "\x81"
b = "\x82"
c = "\u0343"

begin
  z = a + b + c
rescue 
  puts $!
end