class Class
  def new1() 
    puts 'new1'
  end
end

class C
  def a1()  
    puts '1'
  end  
  def C.a2()  
    puts '2'
  end 
  def self.a3()  
    puts '3'
  end
  def C.new1()  
    puts 'new1 override'
  end
end

def C.a4()
  puts '4'
end

puts "---> C.instance_methods:"
puts C.instance_methods.sort                # a1

puts "---> C.methods:"
puts C.methods.sort                         # a2, a3, a4

puts "---> C.class.instance_methods:"
puts C.class.instance_methods.sort          

puts "---> C.class.methods:"
puts C.class.methods.sort

C.new.a1()    # 1
# C.new.a2()  # undefined
# C.new.a3()  # undefined
# C.new.a4()  # undefined

# C.a1() # undefined 
C.a2()   # 2
C.a3()   # 3
C.a4()   # 4

Class.new1 #new1
C.new1     #new1 override

