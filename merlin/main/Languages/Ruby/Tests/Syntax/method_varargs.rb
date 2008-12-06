#* syntax

def method *
	a
end 	
method
# Scenario: parameter name in different line with * (w/o parenthesis)
# Default: pass

def method(*
	a)
end 
method
# Scenario: parameter name in different line with * (w/ parenthesis)
# Default: pass

def method(*a=10)
end 
# Scenario: with default value
# Default: syntax error

def method(**a)
end 
# Scenario: with double star
# Default: syntax error

def method(*a)
	a
end 
method
method(1)
method(2, 3)
method([])
method(*[])
method([4,5])
method(*[6,7])
method([8], 9)
# method(*[10, 11], 12) ??
# Scenario: only one parameter, which is vararg
# Default: pass

def method(*a, b)
end 
# Scenario: two parameters, vararg first
# Default: syntax error

def method(a, *b)
	b
end 
method(1)
method(1, 2)
method(1, 3, 4)
method(1, [5, 6])
method(1, [7], 8)
# Scenario: two parameters, vararg last
# Default: pass

def method(*a, b=2)
end 
# Scenario: two parameters, vararg first, then default value
# Default: syntax error

def method(a=2, *b)
	b
end 
method(1)
method(1, 2)
method(1, 3, 4)
method(1, [5, 6])
method(1, [7], 8)
# Scenario: two parameters, vararg last
# Default: pass

def method(a=2, b, *c)
	return a, b, c
end 
# Scenario: three parameters: b have been first?
# Default: syntax error

def method(a, b=-2, *c)
end 
method 
# Scenario: three parameters: in `method': wrong number of arguments (0 for 1)
# Default: ArgumentError
# ParseOnly: pass

def method(a, b=-2, *c)
	return a, b, c
end 
method(1)
method(2, 3)
method(4, 5, 6)
method(c=[8, 9], a=1)
method(1, 2, c=[10])
# Scenario: three parameters (positive)
# Default: pass
