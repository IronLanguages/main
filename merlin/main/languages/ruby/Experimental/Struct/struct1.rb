class C
  def to_str 
    "CCC" 
  end
end

class D
  def to_sym
    :DDD
  end
end

puts '-- anonymous --'

p Struct.new(:Foo)["f"]
p Struct.new(:Foo, :bar)["f", "g"]
p Struct.new(nil, :bar)["f"]
p Struct.new(:X.to_i)["f"]
p Struct.new(:X.to_i, :bar)["f", "g"]

puts '-- named --'

p Struct.new("Foo")
p Struct.new("Foo", :Foo)["f"]
p Struct.new("Foo", nil) rescue p $!
p Struct.new(C.new, :Foo)["f"]
p Struct.new(D.new, :Foo)["f"] rescue p $!

puts '-- on derived struct --'

class S1 < Struct  
end

class S < S1
end

p S.new(:Foo)["f"]

p S.new(:Foo, :bar)["f", "g"]
p S.new(nil, :bar)["f"]
p S.new(:X.to_i)["f"]
p S.new(:X.to_i, :bar)["f", "g"]

p S.new("Foo")
p S.new("Foo", :Foo)["f"]
p S.new("Foo", nil) rescue p $!
p S.new(C.new, :Foo)["f"]

puts '-' * 20

Customer = Struct.new(:name)
p Customer["joe"].class
p Customer["joe"].class.superclass
