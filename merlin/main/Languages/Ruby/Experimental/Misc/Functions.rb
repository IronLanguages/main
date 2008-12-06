# functions are private methods of Object
def foo() 1 end

puts Object.private_methods.include?("foo")     # true