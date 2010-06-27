def y() 
  puts 'yielding'
  yield
rescue LocalJumpError => e
  puts 'A'
end

def defb(&b)
  $b = b
end

def foo
  defb { 
    puts 'in b'; 
    begin
      return 
    rescue LocalJumpError => e
      puts 'X'
    end
  }
end

def goo
  y &$b
rescue LocalJumpError => e
  puts 'B'
end

foo

begin
  goo
rescue LocalJumpError => e
  puts 'C'
end