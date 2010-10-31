def foo
  x = 1
  eval("y=2")
  eval("z = 3; 1.times { puts x,y,z }")
  eval("puts x,y,z")  
end

def bar  
  a = 0
  eval <<-A
    x = 1
  
    eval <<-B
      y = 2
      
      puts a,x,y
    B
    
    $b = binding
    
    # puts y # error: at the time the eval was compiled 'y' wasn't known to be a local
  A
  
  eval <<-A
    puts a, x, y
  A
end

def baz 
  eval <<-A, $b
    puts a, x, y
  A
end

def g
  1.times {
    x = 1
    eval("y = 1")   # y goes to dynamic dictionary on the block's local scope
  }
  
  #puts x #error
  eval("puts y") rescue p $! # error
end

foo
puts '-' * 20
bar
puts '-' * 20
baz
puts '-' * 20
g