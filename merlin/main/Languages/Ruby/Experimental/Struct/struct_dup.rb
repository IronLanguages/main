p S = Struct.new(:foo)
p S.dup
p S.hash == S.dup.hash

puts '---'

p iS = S['f']
p diS = iS.dup
p iS.hash == diS.hash

puts '---'

p iS = S['f']
p cis = iS.clone
p iS.hash == cis.hash


