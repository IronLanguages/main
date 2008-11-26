$j = 0

T = true
F = false
x = nil

Foo = [T,x,F,F,F]
Bar = [x,F,F,F,F]

def foo
  r = Foo[$j]
  puts "#{$j}: foo #{$j} -> #{r.inspect}"
  $j += 1
  r
end

def bar 
  r = Bar[$j]
  puts "#{$j}: bar #{$j} -> #{r.inspect}"
  $j += 1
  r
end

def test1
  $j = 0
  
  puts foo..bar ? 'true' : 'false'
  puts foo..bar ? 'true' : 'false'
  puts foo..bar ? 'true' : 'false'
  puts foo..bar ? 'true' : 'false'  
end	

def y; yield; end

def test2
  $j = 0
  
  $p = lambda {
    puts foo..bar ? 'true' : 'false'
  }
  
  y &$p  
  y &$p
  y &$p
  y &$p
end

def t(b)
  eval("
    puts x
    puts foo..bar ? 'true' : 'false'
  ", b)
end

def test3
  $j = 0
  x = 1
  b = binding
  t b
  x += 1
  t b
  x += 1
  t b
  x += 1
  t b
end

puts '--- test1 ---'
test1
puts '--- test2 ---'
test2
puts '--- test3 ---'
test3
