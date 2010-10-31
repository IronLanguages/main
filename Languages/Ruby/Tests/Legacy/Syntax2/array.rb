
a = []
a = [    ]
# Scenario: empty array 
# Default: pass

a = [ , ]
# Scenario: trialing comma after empty 
# Default: syntax error

a = [()]
a = [{}]
a = [1, ] #  trialing comma
# Scenario: empty array 
# Default: pass

a = [1, ,]
# Scenario: 
# Default: syntax error

a = [.] 
# Scenario: dot
# Default: syntax error

a = [2.]
# Scenario: 
# Default: syntax error

a = [2.,]
# Scenario: 
# Default: syntax error

a = [1, []]
a = [2, "4", ]
a = ["]", 3, ]
a = [4, "[]]", ]
## two element

# Scenario: unknown
# Default: pass

a = [[]]
a = [[], ]
a = [[], []]
# Scenario: unknown
# Default: pass

a = [[] []]
# Scenario: `[]': wrong number of arguments (0 for 1) (ArgumentError)
# Default: ArgumentError
# ParseOnly: pass

a = [

]
a = [1
]
a = [
2]
a = [
3
]
a = [
4,
5
]
a = 
[6]
# Scenario: unknown
# Default: pass

a = [
4
,
5
]
# Scenario: comma position
# Default: syntax error

a = [
4,
5
,
]
# Scenario: comma position
# Default: syntax error

a 
= [7]
# Scenario: 
# Default: syntax error

#a 
#= 
#[7]
## Scenario: unknown
## Default: syntax error

a = %w{  }
a = %W{ }
a = %w()
a = %W( )
# Scenario: empty
# Default: pass

a = %w

# Scenario: (Unterminated string meets end of file)
# Default: Unterminated

a = %W

# Scenario: unknown
# Default: Unterminated


a = %w  (a) 
# Scenario: space (Unterminated string meets end of file)
# Default: Unterminated


a = %W   (bc)

# Scenario: space
# Default: Unterminated


a = % w(def)

# Scenario: space
# Default: syntax error


a = % W(ghij)

# Scenario: space
# Default: syntax error


a = %W 
(kflmn)


# Scenario: space
# Default: Unterminated


a = %w
(opqrst)

# Scenario: space
# Default: Unterminated


a = %w{ a }
a = %W{ a }  
a = %W{ #a }  
a = %W{ #{a} }

a= %w[ %w{} ]
a= %W{ %w{} }

# Scenario: one element
# Default: pass
