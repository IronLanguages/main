p $:
require 'thread' unless defined? IRONRUBY_VERSION

m = Mutex.new

x = m.synchronize { |*args|
  p args
  p self
  # error -> yield semantics: return 2121
  break 'x'
  #123
}

p x
puts '---'
p m.try_lock
p m.try_lock
p m.try_lock
p m.try_lock
puts '---'
p m.unlock
p m.lock
puts '---'

t1 = Thread.new {
  p m.unlock unless defined? IRONRUBY_VERSION
}

t1.join

puts '---'
p m.lock
p m.locked?

# non-recursive
p m.lock if defined? IRONRUBY_VERSION




