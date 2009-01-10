p i = :foo.to_i
Struct.new(i)

p Struct.new("S1", :foo,:foo)
p Struct::S1.instance_methods(false)


class C
  def to_str #
    "Bar"
  end
  
  def to_sym
    :Baz
  end
end

class D
  def to_str #
    "one"
  end
end

# TODO:
class MyStruct < Struct
end


p Struct.new(C.new, D.new, :two) { |*args|
  p args
  p self
  break 'foo'
}

p Struct.constants
p Struct::Bar.instance_methods(false)
