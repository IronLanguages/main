class C
  def respond_to? name
    puts "?" + name.to_s
    false
  end
end

p IO.new(1)
p IO.new(1, "r")
p IO.new(1, 1)
IO.new() rescue p $!
IO.new(1, nil) rescue p $!
IO.new(1, "r", 2) rescue p $!
IO.new(1, C.new) rescue p $!
IO.new(C.new) rescue p $!

puts '---'

x1 = IO.new(1)
x2 = IO.new(1)
p x1.object_id == x2.object_id

5.times { |d|
  p IO.new(d - 1) rescue p $!
}

f = File.new("a.txt", "w")
g = File.new("b.txt", "w")
p f.to_i
f.send(:initialize, 2) rescue p $!
p f.to_i 
x = IO.new(f.to_i, "r")
p x.to_i
#x.send(:initialize, g.to_i)
p x.to_i

f.puts 'f'
g.puts 'g'
#x.puts 'x' rescue p $!

puts '---'

5.times { |d|
  p IO.new(d) rescue p $!
}


