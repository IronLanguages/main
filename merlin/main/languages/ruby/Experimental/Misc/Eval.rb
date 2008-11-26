module Kernel
  alias old_eval eval
end

x = 1

old_eval("puts x")

