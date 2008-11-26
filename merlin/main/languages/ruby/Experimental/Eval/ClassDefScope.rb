z = 1
class C
  x = 1
  eval("y = 1")
  puts x
  eval("puts y")
  #puts z # error - doesn't close over
end

#puts x #error
#eval("puts y") #error
puts z