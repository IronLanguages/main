=begin
def foo
  return 1,2,
end

def bar *args, &b
  p args, b
end

x = -> {} 

bar foo, not(), not(1), &x

=end

def foo
  x = (1 ? return : return; 2)
end

foo