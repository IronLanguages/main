class L
  def to_str
    puts 'to_str'
    if $x == 1
      $x = 2
      p E.new      # triggers assembly resolve event looking for "a3.dll"
    end
    "."
  end
end

$: << L.new

$x = 0
require 'a1.dll'    # directly loads the assembly

$x = 1
p C.new             # triggers assembly resolve event looking for "a2.dll"
