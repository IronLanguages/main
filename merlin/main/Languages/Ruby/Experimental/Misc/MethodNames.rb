def Foo()
  'method Foo'
end

def Goo()
  'method Goo'
end

Foo = 'constant Foo'
  
puts(Foo)    # constant Foo
puts(Foo())  # method Foo
# puts(Goo)    # uninitialized constant
