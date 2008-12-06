proc = lambda { puts 'lambda' }
def block 
  yield
end

# proc is ignored, prints "foo"
block &proc { puts 'foo' }

# lambda
block &proc 

# foo
block { print 'foo' }
