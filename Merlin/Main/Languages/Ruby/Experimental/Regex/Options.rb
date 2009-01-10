def try_eval str
  puts '-'*20
  puts str
  x = eval str
  p x
  p x.options
rescue SyntaxError
  puts '-'*20
  p $!
end

try_eval '/foo/9'
try_eval '/foo/wiww09'
try_eval '/foo/esnui'
try_eval '/foo/esuiiimmmxxxooo'

puts '-- all possible options --'

def try_option x
  eval "/foo/" + x
rescue SyntaxError
else
  print x
end

('a'..'z').each { |x| try_option x }
('A'..'Z').each { |x| try_option x }

puts
puts '-- to_s --'

r = /foo/eimnosux
p r.to_s
p r.inspect
p r.to_str rescue puts $!

r = /foo/imx
p r.to_s
p r.inspect


p s = /xx#{r}xx#{r}xx/i.to_s
p t = /yy#{s}yy/.to_s

