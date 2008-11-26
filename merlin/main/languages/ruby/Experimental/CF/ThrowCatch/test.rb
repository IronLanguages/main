class S2
  def to_str
    'x2'
  end
end

def f1
  throw :undefined
rescue Exception
  puts "error: #{$!.inspect}"
end

def f2
  throw S2.new, 'hello'
rescue Exception
  puts "error: #{$!.inspect}"
else
  puts 'else'
ensure
  puts 'ensure'
end

catch :x1 do
  #f1
end

x = catch :x2 do
  f2
  puts 'bar'
end

p x 

puts '-'*10

catch :a do
  catch :b do
    catch :c do
      throw :d rescue p $!
      
      begin
        throw :a
      ensure
        throw :b
      end  
      
      puts 'in c'
    end
    puts 'c'
  end
  puts 'b'
end
puts 'a'

x = catch :foo do |*args|
  p args
  123  
end
p x


