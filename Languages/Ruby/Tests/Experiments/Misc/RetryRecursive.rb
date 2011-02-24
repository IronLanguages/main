$i = 0

def zoo
  puts 'zoo'
   1
end

def foo
  puts 'foo'
  bar(zoo) { puts 'block'; retry }
end

def bar a, &p
  puts 'bar'
  # $g = p 
  baz &p
end

def baz &p
  puts 'baz'
  $i += 1
  if $i < 10 then
    yield
  end  
end

foo
# baz &$g