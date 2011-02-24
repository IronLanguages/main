module M1
  CM1 = "CM1"
  
  module M2
    CM12 = "CM12"
    
    puts "M1::M2#.cctor (self = #{self.inspect}):"
    puts CM1
    puts CM12
    puts
  end
  
  def bar
    puts "M1#bar (self = #{self.inspect}):"
    puts CA rescue puts "CA - error: #{$!}"
    puts B::CAB rescue puts "B - error: #{$!}"
    puts ::A::CA
    puts ::A::B::CAB    
    puts CM1 rescue puts "CM1 - error: #{$!}"
    puts CM12 rescue puts "CM12 - error: #{$!}"
    puts
  end
  
  def self.c_bar
    puts "M1#c_bar (self = #{self.inspect}):"
    puts CA rescue puts "CA - error: #{$!}"
    puts B::CAB rescue puts "B - error: #{$!}"
    puts ::A::CA
    puts ::A::B::CAB    
    puts CM1 rescue puts "CM1 - error: #{$!}"
    puts CM12 rescue puts "CM12 - error: #{$!}"
    puts
  end
end

class A
  include M1
  CA = "CA"
  
  class B
    CAB = "CAB"
    
    puts "A::B#.cctor (self = #{self.inspect}):"
    puts CA
    puts CAB
    puts
  end
  
  def error
    #parse error (dynamic constant assignment): ABarC = "ABarC"
  end

  def foo
    puts "A#foo (self = #{self.inspect}):"
    puts CA
    puts B::CAB
    puts ::A::CA
    puts ::A::B::CAB    
    puts CM1
    puts CM12 rescue puts "CM12 - error: #{$!}"
    puts
  end
  
  def self.c_foo
    puts "A#c_foo (self = #{self.inspect}):"
    puts CA
    puts B::CAB
    puts ::A::CA
    puts ::A::B::CAB    
    puts CM1
    puts CM12 rescue puts "CM12 - error: #{$!}"
    puts
  end
end

A.new.foo
A.new.bar
A.c_foo
M1.c_bar

