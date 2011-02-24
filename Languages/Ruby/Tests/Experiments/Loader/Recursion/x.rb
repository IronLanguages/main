puts "x: #{$".inspect}"
require_special('y')
puts "x.pop y: #{$".inspect}"

$".clear
require 'q'
puts "x.pop q: #{$".inspect}"
