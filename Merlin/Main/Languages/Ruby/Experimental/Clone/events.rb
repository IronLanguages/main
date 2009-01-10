# no events fired on duplication

class Class
  def inherited other
    puts "I: #{self} #{other}"
  end 
end

class Module
  def method_added name
    puts "M: #{name}"
  end  
end

class B
end

class C < B
  def foo
  end
end

C1 = C.dup
C2 = C.clone

p C1.ancestors
p C2.ancestors