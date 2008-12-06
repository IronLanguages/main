require 'mock'

def foo
  bar
end

def bar
  e = SystemCallError.new(S.new('foo'))
  p e.message
  raise e
end

begin
  foo
rescue
  p $! 
  p $@
end