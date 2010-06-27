p $"    # []

require "Require.1.rb" # Loaded: 1

p $"

$"[0] = 1
p $"

require "Require.1.rb" rescue p $!

$"[0] = nil
p $"

require "Require.1.rb" rescue p $!

$"[0] = "Require.1.rb"
p $"

require "Require.1.rb" # Loaded: 1

class S
  def to_s; "Require.1.rb"; end
end

$"[0] = S.new
p $"

puts '-' * 20

require "Require.1.rb" rescue p $!

$"[0] = "Require.1.rb"
p $"

puts '-' * 20

$foo = ["Require.2.rb"]

alias $old_q $"
alias $" $foo 

p $"
require "Require.2.rb"

alias $" $old_q

puts '-' * 20

($" = []) rescue p $!

$".delete_at 0
$".delete_at 0
p $"

require "Require.1"
p $"