def f1; return 1,2,3; end
def f2; return 1 => 2, 2 => 3, 3 => 4; end
def f3; return 1, 2, 3, 1 => 2, 2 => 3, 3 => 4; end
def f4; return 1, 2, 3, 1 => 2, 2 => 3, 3 => 4, *['a', 'b', 'c']; end

def defb &b
  b
end

def f5
  c = defb { |*| puts 'foo' }
  b = defb &c
  return 1,2,3, 10=>20, 30=>40, *['a','b'] &['X','Y','Z'] # TODO? semanticS?
end

# ERROR: block argument should not be given
#def f; return 1 => 2, 2 => 3, 3 => 4, &1 end
#def f; return 1,2,3, &1 end
#def f; return 1,2,3, 4 => 5, &1 end

# & is an operator applied on the preceding array
def f6; return *['a','b'] &['a','Y','Z'] end

puts f1.inspect
puts f2.inspect
puts f3.inspect
puts f4.inspect
puts f5.inspect
puts f6.inspect


