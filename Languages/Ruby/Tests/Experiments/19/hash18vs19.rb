begin
eval('
x = {1, 2, 3, 4}
p x
')
rescue SyntaxError
  p $!
end