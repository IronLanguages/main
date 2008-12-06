def try_require path
  require path
rescue LoadError
  puts $!.class
rescue
  p $!
end

def clear
  $:.clear
  $".clear
end

p $:    # []


puts '-' * 100


clear

try_require "Require.1.rb"     # Error

$:[0] = "."
try_require "Require.1.rb"     # Loaded: 1
clear

$:[0] = 1
try_require "Require.1.rb"     # Error
clear

class S
  def to_s; "Require.1.rb"; end
end

$:[0] = S.new
try_require "Require.1.rb"     # Error
clear

puts '-' * 100

p $:
$foo = []

alias $old_colon $:
alias $: $foo 

p $old_colon
p $:

try_require "Require.1.rb"         # Error -> alias ignored

alias $: $old_colon



