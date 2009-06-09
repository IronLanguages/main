# -*- ruby -*-

module Autotest::Timestamp
  Autotest.add_hook :waiting do
    puts
    puts "# Waiting since #{Time.now.strftime "%Y-%m-%d %H:%M:%S"}"
    puts
  end
end
