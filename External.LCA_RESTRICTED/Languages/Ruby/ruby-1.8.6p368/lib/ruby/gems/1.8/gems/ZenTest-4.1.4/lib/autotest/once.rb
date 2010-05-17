##
# this is for autotest plugin developers only...

module Autotest::Once
  Autotest.add_hook :ran_command do |at|
    exit 0
  end
end

