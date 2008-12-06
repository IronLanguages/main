def foo
  eval <<-A
    x = 1
    eval <<-B
      y = 2
      puts x
      x = 3
    B
  A

  eval('puts x,y');  
end

foo