# eventhough it is possible to pass array thru splatting,
# MRI creates a new array

def foo(*a)
  puts a.object_id
  puts a.inspect
end

x = [1,2,3]
puts x.object_id
puts x.inspect
foo *x
