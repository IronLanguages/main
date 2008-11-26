def try_eval str
  puts str
  eval(str)
rescue SyntaxError
  p $!
end

try_eval 'def foo?; end'
try_eval 'def foo?=; end'
try_eval 'def foo!; end'
try_eval 'def foo!=; end'

def foo? *a 
end

def foo! *a
end

try_eval 'foo?1'
#try_eval 'foo!=1'