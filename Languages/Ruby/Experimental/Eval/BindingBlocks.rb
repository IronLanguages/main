def foo
  $b0 = binding
  x,y = 1,1
  $b1 = binding
  
  1.times { |a;x| 
    x = 2
    z = 1
    $b2 = binding
    
    1.times { |a| 
      eval('x = 4')
      eval('u = 1')
      $b4 = binding
    }
  }
  
  1.times { |a;x| 
    x = 3
    $b3 = binding
  } 
end

foo

puts 'x:'
eval('p x',$b0)
eval('p x',$b1)
eval('p x',$b2)
eval('p x',$b3)
eval('p x',$b4)
puts

puts 'y:'
eval('p y',$b0)
eval('p y',$b1)
eval('p y',$b2)
eval('p y',$b3)
eval('p y',$b4)
puts

puts 'z:'
eval('p z rescue puts $!',$b0)
eval('p z rescue puts $!',$b1)
eval('p z',$b2)
eval('p z rescue puts $!',$b3)
eval('p z rescue puts $!',$b4)
puts

puts 'u:'
eval('p u rescue puts $!',$b0)
eval('p u rescue puts $!',$b1)
eval('p u rescue puts $!',$b2)
eval('p u rescue puts $!',$b3)
eval('p u rescue puts $!',$b4)
puts

