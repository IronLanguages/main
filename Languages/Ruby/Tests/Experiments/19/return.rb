begin
eval('
def f1
  return 1,2,3, a: 2, b: 3, &lambda {}
end
')
rescue SyntaxError
  p $!
end

begin
eval('
def f1
  return a: 2, b: 3
end
')
rescue SyntaxError
  p $!
end

begin
eval('
def f2
  return 1,2,3, a: 2, b: 3
end
')
rescue SyntaxError
  p $!
end

def f3
  a,b,c = [1,2],[3,4],[5]
  return *a,*b,*c
end

def f4
  return 1,2,3, a: 2, b: 3
end

def f5
  return :a => 2, :b => 3
end

def f6
  a = [2,3]
  return 1, *a, :a => 2, :b => 3
end

p f2, f3, f4, f5, f6
