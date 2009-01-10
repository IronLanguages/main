def test1
  p self

  /(b)/ =~ "b"
  puts "group: #$1"
  
  str = 'abc'
  result = str.gsub!(/(.)/) { |*args|
    puts '-- in block --'
    puts "args: #{args.inspect}"
    puts "self: #{self.inspect}"
    puts "group: #{$1.inspect}"  
    
    'Z'.gsub!(/(.)/) { 
       puts "inner group: #{$1.inspect}"       
    }
    
    puts "group: #{$1.inspect}"  
    
    #break 'goo'
  }
  
  puts '--------------'
  
  puts "result: #{result.inspect}"
  puts "str: #{str.inspect}"
  puts "group: #{$1.inspect}"
  
end  

def owner
  /(x)/ =~ "x"
  
  $p = Proc.new {
    p $1    
  }
  
  $q = Proc.new {
    p $1    
  } 
  
end

# $~ is set in the current scope, not in the block's scope
def test2
   owner
   'y'.gsub!(/(.)/, &$p)   
   $q.call
   p $1
end

def test3
   owner
   p 'y'.gsub!(/(.)/) { break 'foo' }   
   p $1
   z = 'z'
   
   # doesn't check frozen strings before return:
   
   z.freeze
   p z.gsub!(/(.)/) { puts 'goo'; break 'foo' }   
   p $1
   
   # saves $~ on unsuccessful match:
   "x".gsub!(/(y)/) { puts 'unreachable'; }   
   p $~
end

def test4   
   # returns nil on no match:
   p "x".gsub!(/(x)/) { 'x' }
   p "y".gsub!(/(x)/) { 'x' }
end

puts '-1-'
test1
puts '-2-'
test2
puts '-3-'
test3
puts '-4-'
test4
puts '---'

def def_lambda
  $lambda = lambda { |*| return 'ok' }
end
def_lambda

"a".gsub!(/(a)/, &$lambda) rescue puts $!
