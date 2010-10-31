#* define

def method & block 
	1
end 
method
method { 1 }
# Scenario: space between & and parameter name
# Default: pass

def method &block 
	1
end
method 
method { 1 }
# Scenario: without parenthesis, only arg
# Default: pass

def method a, &block
	1
end 
method(1)
method(2) { 2 }
# Scenario: without parenthsis, two args
# Default: pass

def method a, *b, &block
	1
end
method(1)
method(1,2) 
method(1, 2, 3)
method(1, 2, 3, 4) {5}
# Scenario: without parenthesis, 3 args
# Default: pass

def method &block, a
	1
end 
# Scenario: without parenthsis, two args, & not last
# Default: syntax error

def method a, &block, *c
	1
end 
# Scenario: without parenthesis, 3 args
# Default: syntax error

def method a, &block, 
	
end 
# Scenario: without parenthesis, comma as last
# Default: syntax error

def method &a, &b
	1
end 
# Scenario: what about two block params
# Default: syntax error

#* calling

def method
	1
end 
method
method { 2 }
# Scenario: can pass block no matter what
# Default: pass

def method &block
	1
end 
method
method { 3 }
# Scenario: ok to have no block passed in : block is optional
# Default: pass

def method &block
	1
end	
method({1}) 
# Scenario: can i add parenthesis around the block
# Default: odd number list for Hash

def method a, &b
	1
end 
method 1
method(2)
method(3) {4}  # parenthesis is needed for normal args
# Scenario: two args
# Default: pass

def method a, &b
	1
end 
method 1 {2} 
# Scenario: without parenthesis
# Default: syntax error

def method a, &b
	1
end
method(1), {2}  # odd number list for Hash
# Scenario: No comma between arg and block
# Default: syntax error

def method a, &b
	1
end 
method {2} 

# Scenario: no normal args passed in: wrong number of arguments (0 for 1)
# Default: ArgumentError
# ParseOnly: pass

def method c
end 

method(1)
method(1) do 
    1
end 
method(1) { 1 } 
method 1 do
    2
end
# Scenario: paraenthesis or do-end, related to arguments
# Default: pass

def method c
end 
method 1  { 2 } 
# Scenario: paraenthesis or do-end, related to arguments
# Default: syntax error


def method
end 
p = lambda {}
method &p, 1
# Scenario: pass proc as without ()
# Default: syntax error
