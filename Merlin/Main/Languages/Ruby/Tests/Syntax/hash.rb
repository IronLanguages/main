## created by placing a list of key/value pairs between braces
## with either a comma or the sequence => between the key and 
## the value

h = {}
h = {               }
# Scenario: empty
# Default: pass

h = { , }
# Scenario: trialling comma
# Default: syntax error

h = { "str", 100 }
h = { "str", 100, }
h = { "str" => 100 }
h = { "str" => 200, }
# Scenario: one pair
# Default: pass

h = {
}
h = 
{ }
# Scenario: empty
# Default: pass

h 
= {}
# Scenario: 
# Default: syntax error

h = { 
	100 
	=> 
	"str" 
	}
# Scenario: newline
# Default: syntax error

h = { 100 
	=> "str" }
# Scenario: newline
# Default: syntax error
	
h = { 
	200
	, 
	"str" 
	}
# Scenario: newline
# Default: syntax error

h = { 200
	, "str" }
# Scenario: newline
# Default: syntax error

h = { 200,
	"str" } 
h = { 100 => 
	"str"}
# Scenario: new line
# Default: pass

{ 10 }
# Scenario: odd number
# Default: odd number list for Hash

{"a"=> 10, 'b'}
# Scenario: odd number
# Default: syntax error

{"a"=> 10, 'b', "c" => 10}
# Scenario: odd number
# Default: syntax error

{ 1, 2, 3 }
# Scenario: odd number
# Default: odd number list for Hash

{'a', 10}
# Scenario: seperator ","
# Default: pass

{10:20}
# Scenario: seperator ":"
# Default: syntax error

{ 10 -> 20} 
# Scenario: seperator "->"
# Default: syntax error

{ "a" => 10, "b", 20}
# Scenario: mixed seperator
# Default: syntax error

{ "a", 10, "b" => 20}
# Scenario: mixed seperator
# Default: syntax error
