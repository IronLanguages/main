# encoding: utf-8

f = File.open("a.txt", "r")
i = f.to_i

class C
  def method_missing name
    puts name
    raise NoMethodError
  end
end

#1: to_hash, to_int
#2: to_hash, to_int, to_str
#3: to_hash

p IO.new(mode: "r") rescue p $!
p IO.new(i, C.new) rescue p $!
p IO.new(i, "r:utf-8")
p IO.new(i, mode: "r:utf-8")
p IO.new(i, mdoe: "r:utf-8", encoding: "utf-8") rescue p $!

class Iµ < IO
end

p :Iµ.encoding
p Iµ.name.encoding
p "<#{Iµ.name}: fd 10>".encoding
p Iµ.new(1).inspect.encoding

puts '---'

p IO.new(1)
puts STDIN.to_s
puts STDIN.inspect
p $stdout
p $stderr
p $stderr.dup
