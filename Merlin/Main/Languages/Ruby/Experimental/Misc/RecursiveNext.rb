def foo
  puts 'foo-begin'
  bar { puts 'block'; next }
  puts 'foo-end'
end

def bar &p
  puts 'bar-begin'
  # $g = p 
  baz &p
  yield
  puts 'bar-end'
end

def baz &p
  puts 'baz-begin'
  yield
  puts 'baz-end'
end

foo
# baz &$g