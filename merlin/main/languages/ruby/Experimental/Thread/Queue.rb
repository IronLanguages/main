require 'thread' unless defined? IRONRUBY_VERSION

q = Queue.new
p q.enq(1)
p q.push(2)
p q << 3

p q.size
puts '---'

p q.deq
p q.pop
p q.shift

p q.size
p q.enq 1
p q.clear
p q.empty?

puts '---'

class Queue
  p private_instance_methods(false).include? "initialize"
  p ancestors
end

puts '---'

class C
  def to_int
    5
  end
end

q = SizedQueue.new C.new
5.times { |i| q.enq i }

# deadlock: q.enq 'x'

s = Queue.new
q = Queue.new
t = Thread.new { s.enq nil; q.deq nil; puts 'randezvous'; s.enq nil; q.deq; }
sleep(1)
s.deq
q.enq nil
puts 'randezvous'
sleep(1)
s.deq
q.enq nil
t.join

class SizedQueue
  p ancestors
  #p methods(false)
  p singleton_methods(false)
  p public_instance_methods(false)
  p private_instance_methods(false)
  p protected_instance_methods(false)  
end

class Q < SizedQueue  
  def initialize a
    p super a
  end
  
  def init a
    initialize a
  end
end

q = Q.new 4
q.enq 1
q.enq 2
q.init 6
p q.size
p q.deq
p q.deq

puts '--- max ---'

p q.max = C.new
p q.max

puts '---'

#10.times { |i| print i; q.enq i }
#puts


