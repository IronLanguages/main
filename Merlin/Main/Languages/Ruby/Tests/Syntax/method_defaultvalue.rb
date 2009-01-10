def method a=
end 
# Scenario: without parenthesis, without default value
# Default: syntax error

def method a=
	puts 
end 
method
# Scenario: without parenthesis, without default value (valid??)
# Default: pass

def method a=
	puts "hello"
end 
method
# Scenario: without parenthesis, without default value
# Default: syntax error

def method a=3
	1
end 
method
method(10)
# Scenario: without parenthesis
# Default: pass

def method a = 3
end 
method
method(10)
# Scenario: without parenthesis (but space)
# Default: pass

def method a=3 end
# Scenario: without parenthesis, "end" in the same line as "def"
# Default: syntax error

def method a=
	3
	1
end 
method
method(10)
# Scenario: without parenthesis, default value in different line
# Default: pass

def method a=3,
end 
# Scenario: without parenthesis, with extra comma
# Default: syntax error

def method(a=)
end 
# Scenario: with parentheis, without default value
# Default: syntax error

def method(a==3)
end 
# Scenario: with parentheis, with ==
# Default: syntax error

def method(a=3)
  a
end 
method
method(10)
# Scenario: with parenthesis
# Default: pass

def method(a
	=3)
  a
end 
# Scenario: with parenthesis, = in different line
# Default: syntax error

def method(a=
	3)
end 
method
method(10)
# Scenario: with parenthesis, default value in different line
# Default: pass

## TWO+ PARAMETERS
def method a,
	b = 2
end 
method(1)
method(2, 3)
method(a=4)
method(a=4, b=a * 2)
# Scenario: without parenthesis, parameters in different lines
# Default: pass

def method a=3, b
end 
# Scenario: without parenthesis, default value before
# Default: syntax error

def method a=3,
	b
end 
# Scenario: without parenthesis, parameters in different lines
# Default: syntax error

def method a=3, b=4
end 
method
method(1)
method(1,2)
# Scenario: without parenthesis, two default values
# Default: pass

def method (a,
	b = 2)
end 
method(1)
method(1, 10)
# Scenario: with parenthesis, parameters in different lines
# Default: pass

def method (a=3, b)
end 
# Scenario: with parenthesis, default value before
# Default: syntax error

def method (a=3,
	b)
end 
# Scenario: with parenthesis, parameters in different lines
# Default: syntax error

def method (a=3, b=4)
end 
method
method(1)
method(1,2)
# Scenario: without parenthesis, two default values
# Default: pass

#* calling
def method(a=-3, b=-4)
	a + b
end 

method
method(-2)
method(-1, 0)
method(a = 1)
method(b = 2)
method(a = 3, b = 4)
method(b = 5, a = 6)

method(7, a = 8)
method(a = 9, 10)
method(11, b = 12)
method(b=13, 14)
# Scenario: both have default values
# Default: pass

def method(a, b=-4)
	a + b
end 

method(-2)
method(-1, 0)
method(a = 1)
method(a = 3, b = 4)
method(b = 5, a = 6)

method(7, a = 8)
method(a = 9, 10)
method(11, b = 12)
method(b=13, 14)
# Scenario: the second one has default value (positive)
# Default: pass

def method(a, b=-4)
	a + b
end 
method
# Scenario: the second one has default value (negative 1): in `method': wrong number of arguments (0 for 1)
# Default: ArgumentError
# ParseOnly: pass

def method(a, b=-4)
	a + b
end 
method(b = 2)  # returns 2+(-4) = -2
# Scenario: the second one has default value (negative 2 ??)
# Default: pass

#* value
# value can be any expression?

def method(c="string", d = 1.to_s()) 
	c + d
end 
method
method(1, 2)
# Scenario: valid: string, expression
# Default: pass

def method(c=c)
end 
method(10)
# Scenario: value is itself
# Default: pass

def method(c=-1, d=c)
	puts c, d
	c + d
end 
method
method(1)
method(2, 3)
method(c=4)
method(d=5)
method(c=6, d=7)
method(d=8, c=9)
# Scenario: default value is another parameter
# Default: pass

def method(c=-1, d=c+5)
	c + d
end 
method
method(1)
method(2, 3)
method(c=4)
method(d=5)
method(c=6, d=7)
method(d=8, c=9)
# Scenario: default value is expression with another parameter
# Default: pass

$a = 10
def method(c=-1, d=$a)
	puts c, d
	c + d
end 
method
method(1)
method(2, 3)
method(c=4)
method(d=5)
method(c=6, d=7)
method(d=8, c=9)
# what is "a" now?
# Scenario: default value is another variable
# Default: pass

def method(c=d, d=4)
	c+d
end 
method(3)
method(6, 7)
# Scenario: ok to define: parameter order
# Default: pass

def method(c=d, d=4)
	c+d
end 
method
# Scenario: undefined local variable or method `d' for main:Object
# Default: NameError
# ParseOnly: pass
