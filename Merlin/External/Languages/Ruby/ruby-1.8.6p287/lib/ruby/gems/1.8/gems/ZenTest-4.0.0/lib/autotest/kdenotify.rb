# -*- ruby -*-

# stolen (with permission) from http://www.snailbyte.com/2006/08/24/rails-autotest-growl-notification-using-dcop-and-knotify

module Autotest::KDENotify
  def self.knotify title, msg
    system "dcop knotify default notify " +
           "eventname \'#{title}\' \'#{msg}\' '' '' 16 2"
  end

  Autotest.add_hook :red do |at|
    knotify "Tests failed", "#{at.files_to_test.size} tests failed"
  end
end
