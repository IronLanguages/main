=begin

Method call with array argument vs. array item access
IDENTIFIER whitespaceopt '[' arg ']':
1.	IDENTIFIER is local variable => array item access
2.	whitespace => function call with parameter '[' arg ']'
3.	array item access

=end

def function(*a) print "F:"; [1,2,3] end
def both(*a) print "B:"; [1,2,3] end

var = %w[a b c]
both = %w[a b c]

puts "--"
puts both[1]          # array-item-access(both, 1)
puts "--"
puts both  [1]        # array-item-access(var, 1)
puts "--"
puts var[1]           # array-item-access(var, 1)
puts "--"
puts var  [1]         # array-item-access(var, 1)
puts "--"
puts function[1]      # array-item-access(function-call(function), 1)
puts "--"
puts function  [1]    # function-call(function, [1])
puts "--"
