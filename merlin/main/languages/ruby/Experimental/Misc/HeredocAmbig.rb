=begin

IDENTIFIER whitespaceopt1 '<<' whitespaceopt2 (IDENTIFIER | string)
1. not whitespace1 or whitespace2 => left shift
2. IDENTIFIER is local variable => left shift
3. here-doc

see http://seclib.blogspot.com/2005/11/more-on-leftshift-and-heredoc.html

=end

HEREDOC = 1

def function(*a) print "F:"; a end
def both(*a) print "B:"; a end

var = 0
both = 0

# whitespace
print function<<HEREDOC
puts "shift"
HEREDOC

# whitespace
print function << HEREDOC
puts "shift"
HEREDOC

# whitespace
print function<< HEREDOC
puts "shift"
HEREDOC

# "function" not a local
print function <<HEREDOC
puts "heredoc"
HEREDOC

# "var" is a local
print var <<HEREDOC
puts "shift"
HEREDOC

# "both" is a local
print both <<HEREDOC
puts "shift"
HEREDOC
