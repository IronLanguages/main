require 'x.y'
begin
  require 'z.rb' 
rescue Exception
  puts $! 
end  
