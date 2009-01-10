class C < Object
end 

class D < C
end 
# Scenario: normal
# Default: pass

class C : Object
end 
# Scenario: use :
# Default: syntax error

class C(Object)
end 
# Scenario: use ()
# Default: syntax error

class C()
end 
# Scenario: use () again
# Default: syntax error

class C < C
end 
# Scenario: superclass mismatch for class C
# Default: NameError
# ParseOnly: pass

class D < 1
end 
# Scenario: superclass must be a Class (Fixnum given)
# Default: TypeError
# ParseOnly: pass

class D < Fixnum
end 
# Scenario: Fixnum directly
# Default: pass

class 
	C	< # indicator for superclass
	Object
end	
# Scenario: unknown
# Default: pass

class 
	C
	< 
	Object
end	
# Scenario: unknown
# Default: syntax error

class
	C
	< Object
end
# Scenario: unknown
# Default: syntax error

class C < 
	> Object
end 
# Scenario: unknown
# Default: syntax error

class A 
end 
class B
end 

class C(A, B)
end 
# Scenario: multiple inheritance
# Default: syntax error

class A 
end 
class B
end 

class C < A, B
end 
# Scenario: multiple inheritance
# Default: syntax error

class A 
end 
class B
end 

class C < A < B
end 
# Scenario: superclass must be a Class (NilClass given)
# Default: TypeError
# ParseOnly: pass

class A 
end 
class B
end 

class C : A, B
end 
# Scenario: 
# Default: syntax error
