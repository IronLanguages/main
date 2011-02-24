puts "z(#$loop_count): #{$".inspect}"

if $loop_count > 0 then
  $loop_count -= 1
  require_special('x')
  puts "q.pop x: #{$".inspect}"
else
  puts "Terminating recursion"
  
  # one more, try to add an extension:  
  $".clear
  require('x.rb')
  puts "q.pop x: #{$".inspect}"
end  

