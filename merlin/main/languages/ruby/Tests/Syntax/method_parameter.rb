#* syntax

#normal
def method a
end
method 1

def method a, b
end 
method 1, 2

def method a,
  b
end
method 1, 2

def method (a, 
  b)
  a+b
end
method 1, 2

def method (
  a, 
  b)
  a+ b
end 
method(1, 2)
# Scenario: parameter position:
# Default: pass

def method a,
end 
# Scenario: extra comma, without parenthesis
# Default: syntax error

def method(a,)
end 
# Scenario: extra comma, with parenthesis
# Default: syntax error

def method a  b
end
# Scenario: missing comma
# Default: syntax error

b = 10
def method a b
end 
# Scenario: missing comma, but the next one is valid symbol
# Default: syntax error

#* paramname
def method("abc")
end 
# Scenario: string literal as param name
# Default: syntax error

def method(1)
end 
# Scenario: digit as param name
# Default: syntax error

#* parentheses
def method         (a, b)
	a+b
end 
# Scenario: space is ok between method name and (
# Default: pass

def method(a, b)
	a + b
end 
method (1, 2) # warning
# Scenario: warning expected
# Default: don't put space before argument parentheses


def method(a, b)
	a + b
end 
method(1,2)
# Scenario: normal with parentheses
# Default: pass

def method a, b
	a + b
end
method(1,2)

def method a 
	a * 2
end 
method(2)
# Scenario: normal without parentheses
# Default: pass

#* samename

def method a, a
	1
end 
# Scenario: same name
# Default: duplicate parameter name

def method a, b, a
	1
end	
# Scenario: same name again
# Default: duplicate parameter name

def method a, b, a=10
	1
end
# Scenario: with default value
# Default: duplicate optional argument name
# ParseOnly: duplicate parameter name

def method a, b, c, *b
	1
end 
# Scenario: with varargs (??)
# Default: pass
# ParseOnly: duplicate parameter name
## COMPAT: duplicate rest argument name

def method a, b, c, d, &c
	1
end 	
# Scenario: with block
# Default: duplicate block argument name
# ParseOnly: duplicate parameter name

def method(method)
	1
end
# Scenario: same as method name (??)
# Default: pass
