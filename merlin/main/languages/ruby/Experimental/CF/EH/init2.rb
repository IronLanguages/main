begin
  raise IOError.new "foo"
rescue
  puts $!.backtrace.inspect
  puts $!.message.inspect
  
  puts 'initialize'
  $!.send :initialize
  
  puts $!.backtrace.inspect
  puts $!.message.inspect  
end
