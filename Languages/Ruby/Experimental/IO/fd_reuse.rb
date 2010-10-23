f = File.new("a.txt", "w")
p f.to_i

g = File.new("b.txt", "w")
p g.to_i

h = File.new("c.txt", "w")
p h.to_i

g.close

i = File.new("d.txt", "w")
p i.to_i

p g.to_i rescue p $!
