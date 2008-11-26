class << self
  $SM = self
end

$sm = self

#C = 'TOP'

def foom
  print 'read(C) in instance foom: '
  (p C) rescue puts $!

  print 'read(::C) in instance foom: '
  (p ::C) rescue puts $!
end

module Kernel
  def dump m
    p m
    p m.constants  
    p m.private_instance_methods(false)
  end
end

load 'Load_2.rb', true

puts '----'
dump $G2

puts '----'

p $SM.object_id == $S2.object_id
p $sm.object_id == $s2.object_id

p $sm
p $s2

puts '---'

dump $SM

puts '---'

dump $S2


