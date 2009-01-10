def try_eval str
  puts '-'*20
  puts str
  p eval str
rescue SyntaxError
  puts '-'*20
  p $!
end

#terminators must not be alphanumeric:
try_eval '%! !'
try_eval '%__'
try_eval '%_foo bar_'

#alphanumeric terminators:

