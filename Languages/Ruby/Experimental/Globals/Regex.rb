alias $X $~

"prefix-abcdefghijk-suffix" =~ /(a)(b)(c)(d)(e)(f)(g)(h)(i)(j)(k)/

puts 'NTHREF:'
puts "$1 = " + $1.inspect
puts "$2 = " + $2.inspect
puts "$3 = " + $3.inspect
puts "$4 = " + $4.inspect
puts "$5 = " + $5.inspect
puts "$6 = " + $6.inspect
puts "$7 = " + $7.inspect
puts "$8 = " + $8.inspect
puts "$9 = " + $9.inspect
puts "$10 = " + $10.inspect
puts "$11 = " + $11.inspect
puts "$12 = " + $12.inspect
puts "$13 = " + $13.inspect

puts 'BACKREF:'
puts "$& = " + $&.inspect
puts "$` = " + $`.inspect
puts "$' = " + $'.inspect
puts "$+ = " + $+.inspect

puts "$= = " + $=.inspect
puts "$~ = " + $~.inspect
puts "$X = " + $X.inspect

puts '-' * 100

"PREFIX-ABC-SUFFIX" =~ /(A)(B)(C)/

puts 'NTHREF:'
puts "$1 = " + $1.inspect
puts "$2 = " + $2.inspect
puts "$3 = " + $3.inspect
puts "$4 = " + $4.inspect
puts "$5 = " + $5.inspect
puts "$6 = " + $6.inspect
puts "$7 = " + $7.inspect
puts "$8 = " + $8.inspect
puts "$9 = " + $9.inspect
puts "$10 = " + $10.inspect
puts "$11 = " + $11.inspect
puts "$12 = " + $12.inspect
puts "$13 = " + $13.inspect

puts 'BACKREF:'
puts "$& = " + $&.inspect
puts "$` = " + $`.inspect
puts "$' = " + $'.inspect
puts "$+ = " + $+.inspect

puts "$= = " + $=.inspect
puts "$~ = " + $~.inspect
puts "$X = " + $X.inspect

puts '-' * 100

puts "$~ = " + $~.inspect
puts "$X = " + $X.inspect

puts "$~ = " + $~.inspect
puts "$X = " + $X.inspect

puts '-' * 100

def foo
  puts "$~ = " + $~.inspect
  puts "$X = " + $X.inspect
end

foo

puts '-'*100

1.times {
  "hello" =~ /hello/
}

puts "$~ = " + $~.inspect
puts "$X = " + $X.inspect



