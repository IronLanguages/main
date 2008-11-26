class << "string"
end 

class 
	<< "string"
end 

class
	<<
	"string"
end 

x = "string"
class << x
end 

class <<
	x
end 
# Scenario: normal
# Default: pass

class < < "string"
end 
# Scenario: space between < <
# Default: syntax error

class << 1
end 
# Scenario: no virtual class for Fixnum
# Default: TypeError
# ParseOnly: pass

