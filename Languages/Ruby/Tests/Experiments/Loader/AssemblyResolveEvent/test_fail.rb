class L
  def to_str
    puts 'to_str'
    p C.new if $x == 1  # recursively triggers assembly resolve event looking for "a2.dll"
    "."
  end
end

$: << L.new

$x = 0
require 'a1.dll'    # directly loads the assembly

$x = 1
p C.new             # triggers assembly resolve event looking for "a2.dll"
