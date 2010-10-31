def try a
  require a
rescue Exception
  puts $!
end

try 'x'
try 'r'
try 'x.rb'
try 'z.rb'
try 'x.dll'
try 'w.dll'
try 'q.dll'
