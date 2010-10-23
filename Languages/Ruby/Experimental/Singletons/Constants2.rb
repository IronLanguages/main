# contrary to class variables, constatns are lexically defined on singleton classes

$x = "x"
module M
  class << $x
    $Sx = self
    C = 1
  end
end

p M.constants    # []
p $Sx.constants  # ["C"]