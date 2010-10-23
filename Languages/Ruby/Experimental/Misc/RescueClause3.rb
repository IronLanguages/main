class Module
  alias old ===

  def ===(other)
    puts "compared: #{self} (T) === #{$!} ($!)"
    
    puts 'replacing $!'
    $! = MyExc2.new
        
    old other
  end
end

class MyExc1 < Exception
end

class MyExc2 < Exception
end

def foo()
  raise MyExc1
rescue MyExc1 => e
  puts "rescued: $! = #{$!}, e = #{e}"
end

foo