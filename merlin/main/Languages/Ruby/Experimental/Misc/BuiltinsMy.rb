module MyKernel
  private
  def my_puts(x)
    MyKernel.my_puts
  end
  
  public
  def MyKernel.my_puts(x)
    puts "my_#{x}" 
  end
end

class MyObject
  include MyKernel
end

foo = MyObject.new
#foo.my_puts 'bar'
#foo.puts 'bar'

require 'Builtins.rb'
dump(MyKernel)