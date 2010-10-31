puts
puts '----'

$G2 = Module.nesting[0]

class << self
  $S2 = self
end

$s2 = self

C = 'G2'

def foo2(fcaller)  
  print 'read(C) in instance foo2: '
  (p C) rescue puts $!

  print 'read(::C) in instance foo2: '
  (p ::C) rescue puts $!
end


foo2 "top-level G2"

puts '----'

print 'read(C) in G2 top-level: '
(p C) rescue puts $!

print 'read(::C) in G2 top-level: '
(p ::C) rescue puts $!

puts '----'

foom


