# identity (recursively) insensitive
p [].hash == [].hash
p [].eql?([])
p [[[1]]].hash == [[[1]]].hash
p [[[1]]].eql?([[[1]]])

# order sensitive
p [[], 1].hash == [1, []].hash
p [[], 1].eql?([1, []])

class C
  def respond_to? name
    puts "?#{name}"
    false
  end
end

class A < Array  
  def hash
    puts 'hash'
    C.new
  end
end

class B < Array  
  def hash
    puts 'hash'
    1111111111111111111111111111111111111
  end
  
  def eql?(other)
    puts 'eql?'
    super
  end
end

p [[A.new]].hash == [[A.new]].hash rescue p $!
p [[B.new]].hash == [[B.new]].hash rescue p $!

p [[B.new]].eql?([[B.new]]) rescue p $!

puts '- Hash -'

# hash (1.8 different from 1.9):
p {}.hash == {}.hash
h1={}
h1[{}] = {}
h2={}
h2[{}] = {}
p h1.hash == h2.hash
