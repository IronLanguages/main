module Autotest::AutoUpdate
  @@sleep_time, @@update_cmd, @@updater = 60, "svn up", nil

  def self.sleep_time= o
    @@sleep_time = o
  end

  def self.update_cmd= o
    @@update_cmd = o
  end

  Autotest.add_hook :run_command do  |at|
    @@updater.kill if @@updater
  end

  Autotest.add_hook :ran_command do  |at|
    @@updater = Thread.start do
      loop do
        puts "# Waiting for #{@@sleep_time} seconds before updating"
        sleep @@sleep_time
        puts "# Running #{@@update_cmd}"
        system @@update_cmd
      end
    end
  end
end
