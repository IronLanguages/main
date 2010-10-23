f = File.new("a.txt", "w")
g = File.new("b.txt", "w")

x = IO.new(3, "w")
p x 

# reinitialization destroys descriptor 3:
x.send(:initialize, -1, "w") rescue p $!
p x

p f.closed?
p x.closed?  # true in 1.8, false in 1.9

# not valid desc:
f.close rescue p $!

puts '---'
10.times { |x| puts "#{x}: #{IO.new(x)}" rescue p $!}
puts '---'
