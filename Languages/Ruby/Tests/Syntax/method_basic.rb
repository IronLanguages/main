def method end
# Scenario: "end" in the same line as "def"
# Default: syntax error

def 
  method end
# Scenario: "def " in different line as method name
# Default: syntax error

def 
  method
end
method
# Scenario: method name in the different line as "def"
# Default: pass

def 
  method 
    a
  end 
method
# Scenario: "a" as method body here
# Default: NameError
# ParseOnly: pass

$a = 10
def 
  method 
    $a
  end 
method
# Scenario: "a" as method body here
# Default: pass

def 
  method 
    a
  end 
method 1  
# Scenario: "a" as method body here
# Default: ArgumentError
# ParseOnly: pass

def method 1 end 
# Scenario: body/"end" in the "def" line
# Default: syntax error

def method a 1
end
# Scenario: body/parameter in the "def" line
# Default: syntax error

def method() end
method
# Scenario: "end" in the "def" line, parameters specified by parenthesis, no body
# Default: pass

def method() 1 end
method

def method ( ) 1 end # space between the name and the parenthesis
method

def method(a)  1 end
method 1

def method(a)  1
end 
method 1
# Scenario: body/"end" in the "def" line, parameters specified by parenthesis
# Default: pass

def "method"
  1
end 
# Scenario: "string" after def
# Default: syntax error

def []
  1
end 
# Scenario: empty array (operator) after def
# Default: pass

def [1, 2]
  1
end 
# Scenario: non-empty array after def
# Default: syntax error

def [i, j]
  1
end 
# Scenario: non-empty array after def
# Default: syntax error

def 9method
end 
# Scenario: method name starting with digit
# Default: syntax error

def me1thod
end
me1thod
# Scenario: method name embeded with digit
# Default: pass

def Method 
  1
end 
Method

# uppercase in the middle of the method name
def mEthod
  1
end 
mEthod

def mETHOD
  1
end 
mETHOD
# Scenario: method name begins with Uppercase
# Default: pass

def _method
  1
end 
_method

def me_thod
  1
end
me_thod
# Scenario: method name begins with _, or _ in
# Default: pass

def .method
  1
end 
# Scenario: method name begins with .
# Default: syntax error

def me.thod
end
# Scenario: method name with . inside: undefined local variable or method `me' for main:Object (NameError)
# Default: NameError
# ParseOnly: pass

me = "hello"
def me.thod
  1
end 
me.thod
# Scenario: method name with . inside (works)
# Default: pass

def me?thod
end
me? 1
# Scenario: method name with "?" inside
# Default: pass

def me?thod
end
me?thod
# Scenario: method name with "?" inside, and call it
# Default: NameError
# ParseOnly: pass

def method?
  1
end
method?
# Scenario: method name ends with "?"
# Default: pass

def <
  1
end 
# Scenario: method name "<"
# Default: pass

def <
end
<
# Scenario: method name "<", and call it
# Default: syntax error

# Scenario: method name "<<"
# Default: pass
def <<
end 

def method<
end 
# Scenario: method name ends with "<"
# Default: syntax error

def meth<od
end 
# Scenario: method name has "<"
# Default: syntax error

def method?!
end
# Scenario: method name ends with "?!"
# Default: syntax error

def method!?
end 
# Scenario: method name ends with "!?"
# Default: syntax error

def method=?
end 
# Scenario: method name ends with "=?"
# Default: syntax error

def method!_
	"!_"
end 
# Scenario: unknown
# Default: pass

def method!_
	"!_"
end 
method!_
# Scenario: undefined local variable or method `_' for main:Object, _
# Default: NameError
# ParseOnly: pass
