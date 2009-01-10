# load doesn't check the current directory, it only checks $:

def try_load path
  load(path) 
rescue Exception
  puts "load('#{path}') -> '#{$!}'"
else
  puts "load('#{path}') -> OK"
end

puts '-----'

$:.clear
p $:
p $"

try_load('..\x.rb') 
try_load('\temp\load\x.rb')
try_load('/temp\load\x.rb')
try_load('C:\temp\load\x.rb')

puts '-----'

$: << 'bbb'
p $:
p $"

try_load('..\x.rb') 
try_load('/temp\load\x.rb')
try_load('C:\temp\load\x.rb')
