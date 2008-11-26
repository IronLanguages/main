class Class
  def inherited s
    p Object.constants.include?(s.name)

    if s.name == "YAML::Syck::Resolver" then
      raise IOError
    end
    if $raise then
      $sub = s
      puts "raise #{$raise}"
      raise $raise
    end
    
    puts "#{s} < #{self}"
  end
end

class B
  
end

class A < B
end

class << self
  class << self
    class << self
    end
  end  
end

puts '-'*25
puts 'dup'
puts '-'*25

C = A.dup                          # no event

puts '-'*25
puts 'Struct'
puts '-'*25

S = Struct.new :foo, :bar do
  puts self.new.foo
  puts 'bar'
end

puts '-'*25

$raise = 'xxx'
begin
  S = Struct.new :foo, :bar do
    puts self.new.foo
    puts 'bar'
  end
rescue
  p $!
end
p $sub.public_instance_methods(false)
p $sub.private_instance_methods(false)
p $sub.protected_instance_methods(false)
p $sub.singleton_methods(false)
$raise = nil

puts '-'*25
puts 'Class.new'
puts '-'*25

X = Class.new B do
  def bar
  end
  puts 'bar'
end

puts '-'*25

$raise = 'hello'
begin
  X = Class.new B do
    def bar
    end
    puts 'bar'
  end
rescue
  p $!
end
p $sub.instance_methods(false)
$raise = nil

puts '-'*25
puts 'class E < B'
puts '-'*25

$raise = 'hello'
begin
  class E < B
    def bar
    end
    puts 'bar'
  end
rescue
  p $!
end
p $sub.instance_methods(false)
$raise = nil

puts '-'*25
puts 'yaml'
puts '-'*25

begin
  require 'yaml'
rescue
  p $!
end

