begin
eval('
def f1
  return 1,2,3, 4=>5, *[10,11]
end

p f1
')
rescue SyntaxError
  p $!
end


def f6
  a = [1]
  p a.object_id
  return *a
end

def f7
  a = []
  return *a
end


p x = f6, x.object_id, f7