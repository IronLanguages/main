$".clear

class C
  def to_str
    "x.rb"
  end
end

$" << C.new
puts '1:'
require "x.rb"
puts '2:'
require "X.rb"           # case sensitive
puts '3:'
require 'A\..\x.rb'
puts '4:'

p $"

$".clear
$" << nil
require "x.rb" rescue puts $!


# $" set after successful require

puts '5:'
$".clear
p $"
begin
  require "p_loaded_files.rb"
rescue
  p $"  
  p 'error'
end  
p $"