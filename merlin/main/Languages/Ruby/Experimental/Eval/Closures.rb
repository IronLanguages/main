def foo
  if (false)
    x = 1
  end
  
  1.times { 
    y = 2
    1.times {
      z = 3
      p x,y,z
    }
  }
end

foo