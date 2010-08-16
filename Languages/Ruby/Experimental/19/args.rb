def foo *args
  p args
end

def []=(*args)
  p args
end

a, b = ['a'], ['b']

foo 1,*a,2,*b,*a,*b,3
self[1,*a,2,*b,*a,*b,3] = 2