$: << "LoadOnceDir"
puts "$: = #{$:}"
puts "$\" = #{$"}"
require 'LoadOnce1'
puts "$\" = #{$"}"
