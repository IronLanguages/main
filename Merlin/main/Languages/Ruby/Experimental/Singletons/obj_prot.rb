class C
end

x = C.new

class << x
  ::OP = self
end

p OP.instance_methods(false)
p OP.private_methods(false)
p OP.protected_methods(false)
p OP.singleton_methods(false)
p OP.ancestors
