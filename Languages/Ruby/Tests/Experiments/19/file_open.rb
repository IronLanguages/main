class C
  def respond_to? name
    puts "C: #{name}"
    false
  end
end

class NilClass
  def respond_to? name
    puts "nil: #{name}"
    false
  end
end

File.open rescue p $!
puts '-1-'
File.open(C.new) rescue p $!
File.open(nil) rescue p $!
puts '-2-'
File.open("a.txt", C.new) rescue p $!
File.open("a.txt", nil) rescue p $!
puts '-3-'
File.open("a.txt", "r", C.new) rescue p $!
File.open("a.txt", "r", nil) rescue p $!
puts '-4-'
File.open("a.txt", "r", 0, C.new) rescue p $!

puts '==='

class D
  def respond_to? name
    puts "D: #{name}"
    super

  end
  
  def to_int
    0
  end
end

File.open("a.txt", C.new, nil) rescue p $!
