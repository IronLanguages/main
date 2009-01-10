
def method(a, b)
end 
method((1,2))
# Scenario: double parenthesis around args
# Default: syntax error

def method(a, b, c)
	return a, b, c
end 
method(*[1, 2, 3])
method(4, *[5, 6])
method(7, 8, *[9])
method(10, 11, 12, *[])
# Scenario: where can it be put?
# Default: pass

def method(a, b, c)
	return a, b, c
end 
method(*[1,2], 3)
# Scenario: must be the last one: first
# Default: syntax error

def method(a, b, c)
	return a, b, c
end 
method(1, *[2], 3)
# Scenario: must be the last one: middle one
# Default: syntax error

def method(a, b, c, d, e)
end 
method(1, *[2, 3], *[4, 5])
# Scenario: two expanding
# Default: syntax error

def method(a, b, c, d, e)
end 
method(1, *[2, *[3, 4], 5])
# Scenario: nested expanding
# Default: syntax error

#* hashargs

def method(a)
	return a
end 

method({ 
	'language' => 'ruby',
	'framework' => '.net'
	})

method( 
	'language' => 'ruby',
	'framework' => '.net'
	)

method({ 
	:language => 'ruby',
	:framework => '.net'
	})

method( 
	:language => 'ruby',
	:framework => '.net'
	)
# Scenario: one arg
# Default: pass

def method(a)
	return a
end 

method { 
	'language' => 'ruby',
	'framework' => '.net'
	}	
# Scenario: one arg: without parenthesis, use hash, different line
# Default: syntax error

def method(a)
	return a
end 

method { 'language' => 'ruby',	'framework' => '.net' }	
# Scenario: one arg: without parenthesis, use hash, same line
# Default: syntax error


def method(a)
	return a
end 

method :language => 'ruby', :framework => '.net'
# Scenario: one arg: without parenthesis, use Symbol
# Default: pass


def method(a)
	return a
end 

method :language => 'ruby', 
	:framework => '.net'
# Scenario: one arg: without parenthesis, use Symbol
# Default: pass

def method(a, b)
	return a, b
end 	

method({ 
	'language' => 'ruby',
	'framework' => '.net'
	}, 8)

method(9, { 
	'language' => 'ruby',
	'framework' => '.net'
	})

method(12, 'framework' => '.net')
method(34, 'language' => 'ruby', 'framework' => '.net')
# Scenario: two args (postive)
# Default: pass

def method(a, b)
	return a, b
end
method('language' => 'ruby', 'framework' => '.net')
# Scenario: two args: wrong number of arguments (1 for 2)
# Default: ArgumentError
# ParseOnly: pass

def method(a, b)
	return a, b
end
method('language' => 'ruby', 34)
# Scenario: two args: hash arg must be the last
# Default: syntax error

def method(a, b)
	return a, b
end
method('language' => 'ruby', 2, 'framework' => '.net')
# Scenario: two args
# Default: syntax error
