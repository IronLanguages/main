=begin

  x --> y --> z -(R)-> x
    --> q
  
  z requires x using a different (but equivalent) path each time
  
=end

def require_special(x)
  #
  # This doesn't trigger a recursion,so the internal stack doesn't contain combined paths
  #
  # $:[0] = ('.\\' * $loop_count) + '.'
  # $".clear
  # require x
  
  $".clear
  require ('.\\' * $loop_count) + x
end


$:.clear
$:[0] = '.'
  
$loop_count = 5

puts "test: #{$".inspect}"
require 'x'
puts "test.pop x: #{$".inspect}"
