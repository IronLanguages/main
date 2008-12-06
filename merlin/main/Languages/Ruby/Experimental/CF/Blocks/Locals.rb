# Ruby 1.9 breaking changes

# Locals defined in a block are not visible outside the block.
def foo1
  a = 1
  b = 2
  y = 3  
  1.times { |a,b,x|            # b,x are fresh locals here with the scope of the block; outer a is shadowed
    puts '|a,b,x|'
    puts "inside"
    puts "a = #{a}"
    puts "b = #{b}"
    puts "x = #{x}"
    puts "y = #{y}"
    puts
    
    z = 4
    x = 5
    b = 6
  }
  
  puts "after"
  puts "a = #{a}"
  puts "b = #{b}"
  puts "x = #{x}" rescue puts "ERROR(x)"
  puts "y = #{y}"
  puts "z = #{z}" rescue puts "ERROR(z)"
  puts
end

foo1

# only locals can be used in block parameters
class C
  def errors
    begin
      eval('1.times { |$x| }') 
    rescue Exception
      puts $!.message
    end

    begin
      eval('1.times { |X| }') 
    rescue Exception
      puts $!.message
    end

    begin
      eval('1.times { |@x| }') 
    rescue Exception
      puts $!.message
    end

    begin
      eval('1.times { |@@x| }') 
    rescue Exception
      puts $!.message
    end
  end
end

C.new.errors 
