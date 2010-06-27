$:.clear
$: << '.'

# .rb files take precendce
require 'fcntl'

puts Fcntl.constants