class Module
  alias old ===

  def ===(other)
    puts "compared: #{self} (T) === #{$!} ($!)"
    
    if ($i == 0)
      $i = 1;
      puts 'replacing $!'
      $! = MyExc2.new
    end
    
    old other
  end
end

class MyExc1 < Exception
end

class MyExc2 < Exception
end

def foo(i)
  $i = i
  raise MyExc1
rescue MyExc2  # compares E1 !== E2, sets $! to a MyExc2 instance       
  puts "rescued A: #{$!.class}"
rescue MyExc1
  puts "rescued B: #{$!.class}"
rescue MyExc2
  puts "rescued C: #{$!.class}"
end

def bar(i)
  $i = i
  raise MyExc1
rescue MyExc2,  # compares E1 !== E2, sets $! to a MyExc2 instance
       MyExc2   # catches (uses the updated value of $!)
  
  puts "rescued A: #{$!.class}"
rescue MyExc1
  puts "rescued B: #{$!.class}"
rescue MyExc2
  puts "rescued C: #{$!.class}"
end


foo(0)
puts '-------'
foo(1)
puts '-------'
bar(0)
puts '-------'
bar(1)
puts '-------'
