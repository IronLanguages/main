puts "self = #{self}"
puts private.inspect

class Module
  def my_private *a
    puts 'private'
    private *a
  end
end

module M
  def M.foo
    puts "self = #{self}"
    puts private.inspect
  end
  
  foo
  
  def a1
  end

  my_private
  
  def a2
  end
  
  private
  
  def a3
  end
  
  public
  
  def a4
  end

  my_private :a4
  
  # a3 only, 
  # scope dependent, LanguageContext needs to encode the flag
  puts private_instance_methods.sort.inspect  
end

puts '---'

module M2
  def a
  end
  
  puts private_instance_methods.sort.inspect 
end

puts '---'

class C2
  def a
  end
  
  puts private_instance_methods.sort.inspect 
end

