class P
  def to_str
    #puts 'converting'
    "."
  end
end

$:.clear
$: << P.new

puts '---'

require 'a'
p $"

require 'a.rb'
p $"

puts '---'

begin
  require 'b'
rescue Exception
  puts $!
end
p $"

puts '---'

$".clear
$" << "a.rb"

x = 'a'
require x
p $"

puts '---'

x = 'a.rb'
require x
p $"


