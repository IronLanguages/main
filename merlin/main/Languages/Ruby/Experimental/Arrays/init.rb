require 'mock'

class Array
  def init *a, &b
    initialize *a, &b
  end
end

puts '-- new ----------------------------------------'

class C
end

p Array.new()
p Array.new(I.new(1))
p Array.new(A.new([1,2]))
p Array.new(AI.new([1,2], 1))

puts '----'

p Array.new(I.new(5), C.new)
p Array.new(A.new([1,2]), C.new) rescue p $!

puts '----'

p Array.new(I.new(5)) { |x| x + 10 }
p Array.new(A.new([1,2,3])) { |x| raise }
p Array.new(AI.new([1,2,3], 10)) { |x| raise }

puts '----'

p Array.new(5) { break 'break result' }

[false, true].each do |$freeze|

    puts "-- initialize (frozen = #$freeze) -------"

	def test_initialize *a, &b
	  array = ['o','r','i','g','i','n','a','l']
	  
	  if $freeze
	    array.freeze 
	    (r = array.init(*a, &b)) rescue p $!
	  else
	    r = array.init(*a, &b)
	  end  
	  p r, array
	end
	
	test_initialize()
	test_initialize(I.new(1))
	test_initialize(A.new([1,2]))
	test_initialize(AI.new([1,2], 1))
	
	puts '----'
	
	test_initialize(I.new(5), C.new)
	test_initialize(A.new([1,2]), C.new) rescue p $!
	
	puts '----'
	
	test_initialize(I.new(5)) { |x| x + 10 }
	test_initialize(A.new([1,2,3])) { |x| raise }
	test_initialize(AI.new([1,2,3], 10)) { |x| raise }
	
	puts '----'
	
	p test_initialize(5) { break 'break result' }

end

puts '-----------------------------------------------'

class Class
  alias xnew new
  alias xallocate allocate
  
  def new *a, &b
    puts 'Class#new'
    p a,b
    xnew *a, &b   
  end
  
  def allocate *a, &b
    puts 'Class#allocate'
    p a,b
    xallocate *a, &b   
  end
end

class MA < Array
end

p a = MA[1,2,3]

class Class
  alias new xnew
  alias allocate xallocate
end
