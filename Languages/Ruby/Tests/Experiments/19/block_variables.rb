a = 1

l = lambda { |x,a|
  x = 1
  a = 2
  p x,a
}

l.(1,2)

p a