# -*- ruby -*-

# special thanks to Pat Eyler, Sean Carley, and Rob Sanheim
# and to Peter Havens for rspec patches
module Autotest::RedGreen
  BAR       = "=" * 78
  REDCODE   = 31
  GREENCODE = 32

  Autotest.add_hook :ran_command do |at|
    green = case at.results.last
            when /^.* (\d+) failures, (\d+) errors$/   # Test::Unit
              ($1 == "0" and $2 == "0")
            when /^\d+\s+examples?,\s+(\d+)\s+failure/ # RSpec
              ($1 == "0")
            end

    code = green ? GREENCODE : REDCODE
    puts "\e[#{ code }m#{ BAR }\e[0m\n\n" unless green.nil?
  end
end
