class end
# Scenario: no name
# Default: syntax error
	
class 
end 
# Scenario: no name
# Default: syntax error

class C end
# Scenario: same line
# Default: syntax error

class C
end 

class # name in different line 
	C
end 

class D
	
	end 

class E;	#semi-comma
end 
# Scenario: unknown
# Default: pass

class C,
end 
# Scenario: comma after name
# Default: syntax error

class MyClass	
end 

class CLASS	# all uppercase
end

class My_Class # underscore
end 
# Scenario: allowed:
# Default: pass

class myclass
end 
# Scenario: all lowercased
# Default: class/module name must be CONSTANT
# ParseOnly: Class/module name must be a constant

class myClass
end 
# Scenario: starting with lowercase
# Default: class/module name must be CONSTANT
# ParseOnly: Class/module name must be a constant

class _Myclass
end 
# Scenario: starting with underscore
# Default: class/module name must be CONSTANT
# ParseOnly: Class/module name must be a constant

class Class 	
end 
# Scenario: special class "Class"
# Default: pass

class "string"
end 
# Scenario: string as name
# Default: syntax error

class < "string"
end 
# Scenario: "<"
# Default: syntax error

class My
	def m
	end 
end 

class My.m
end 
# Scenario: "." in the name
# Default: syntax error

class << "string"
end 
# Scenario: singleton
# Default: pass
