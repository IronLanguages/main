# nested method definition

#* syntax

def method def	nested end end 
# Scenario: same line
# Default: syntax error
 
def method def	nested end 
end 
# Scenario: unknown
# Default: syntax error

def method def nested
end end
# Scenario: 
# Default: syntax error

def method def nested
end 
end
# Scenario: 
# Default: syntax error

def method def 
	nested end end
# Scenario: 
# Default: syntax error
 
def method def 
nested end 
end
# Scenario: unknown
# Default: syntax error

def method def 
	nested 
end 
end
# Scenario: 
# Default: syntax error

def method 
	def 
	nested 
end 
end
method 
# Scenario: unknown
# Default: pass

def method 
	def 
	nested 
end 
method
# Scenario: missing end
# Default: syntax error


def method 
	def 
	nested 
end 
end
end 
method 
# Scenario: extra end
# Default: syntax error

def method
	a = 1
	def 
		nested
	end
	nested
end 
method
# Scenario: unknown
# Default: pass
