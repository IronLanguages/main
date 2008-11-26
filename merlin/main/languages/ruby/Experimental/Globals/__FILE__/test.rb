$:.clear
$: << '.\a'

puts "$0 = #{$0}"
puts "__FILE__ = #{__FILE__}"
puts __FILE__ == $0

require 'y'