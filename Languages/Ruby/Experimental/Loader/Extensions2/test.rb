$:.clear
$: << '.'
$: << 'A'

# .rb files take precedence over .dll/.so
$".clear
require 'fcntl'

puts Fcntl.constants

$".clear
require 'fcntl'
