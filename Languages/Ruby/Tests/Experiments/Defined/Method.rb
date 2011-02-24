# defined? foo behaves like method :foo, not like undef foo

module M
  def foo; end
end

module N
  def foo_defined?
     p defined? foo    #foo is not in lexical scope, but still this returns "method" 
     undef foo rescue p $! # erorr       
     p defined? foo    # still defined
  end
end

class C
  include M,N
end

C.new.foo_defined?

puts '---'

def foo
  p 'foo'
end

p defined? foo.bar