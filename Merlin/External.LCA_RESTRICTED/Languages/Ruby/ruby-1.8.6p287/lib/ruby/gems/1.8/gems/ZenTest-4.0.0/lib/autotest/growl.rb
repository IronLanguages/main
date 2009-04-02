# -*- ruby -*-

module Autotest::Growl
  def self.growl title, msg, pri = 0, img = nil
    title += " in #{Dir.pwd.split(/\//).last(3).join("/")}"
    msg += " at #{Time.now.strftime('%Y-%m-%d %H:%M:%S')}"
    # TODO: parameterize default image
    img ||= "/Applications/Mail.app/Contents/Resources/Caution.tiff"
    cmd = "growlnotify -w -n autotest --image #{img} -p #{pri} -m #{msg.inspect} #{title}"
    system cmd
    nil
  end

  Autotest.add_hook :initialize do  |at|
    growl "autotest running", "Started"
  end

  Autotest.add_hook :red do |at|
    growl "Tests Failed", "#{at.files_to_test.size} tests failed", 2
  end

  Autotest.add_hook :green do |at|
    growl "Tests Passed", "Tests passed", -2 if at.tainted
  end

  Autotest.add_hook :all_good do |at|
    growl "Tests Passed", "All tests passed", -2 if at.tainted
  end
end
