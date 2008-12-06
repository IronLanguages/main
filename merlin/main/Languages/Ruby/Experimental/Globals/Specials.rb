alias $! $foo

$foo = 1
p $!

begin
  raise
rescue Exception => e 
  p e.class
end


=begin

($! = 'boo') rescue puts 'Error'

alias $! $foo

begin  
  raise Exception, "foo", ["f", "g", "h"]
rescue Exception => $e
  p $e
end
  


=end