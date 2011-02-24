class C
  def !(*args)
    puts "C: ! #{args}"
    true
  end
  
  def !=(*args)
    puts "C: != #{args}" 
    nil
  end
  
  def foo
    puts '---'
    p not()    
    
    puts '---'
    p not(C.new)
  end
end

class NilClass
  def !(*args)
    puts "nil: !#{args}"
  end
end

c = C.new

if not c
  puts 'true'
else
  puts 'false'
end

if !c
  puts 'true'
else
  puts 'false'
end

if c != 1
  puts 'true'
else
  puts 'false'
end

c.foo