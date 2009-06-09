require 'enumerator'

class C
  def to_int
    3
  end
end

p [1,2,3,4].enum_cons(C.new).entries
p [1,2,3,4].enum_slice(C.new).entries
p [1,2,3,4].enum_with_index().entries