module Autotest::Notify
  def self.notify(title, message, priority='critical')
    icon = if priority == 'critical'
      'dialog-error'
    else
      'dialog-information'
    end
    system "notify-send -u #{priority} -t 10000 -i #{icon} '#{title}' '#{message.inspect}'"
  end

  Autotest.add_hook :red do |at|
    tests = 0
    assertions = 0
    failures = 0
    errors = 0
    at.results.scan(/(\d+) tests, (\d+) assertions, (\d+) failures, (\d+) errors/) do |t, a, f, e|
      tests += t.to_i
      assertions += a.to_i
      failures += f.to_i
      errors += e.to_i
    end
    message = "%d tests, %d assertions, %d failures, %d errors" % 
      [tests, assertions, failures, errors]
    notify("Tests Failed", message)
  end

  Autotest.add_hook :green do |at|
    notify("Tests Passed", "Outstanding tests passed", 'low') if at.tainted
  end

  Autotest.add_hook :all do |at|_hook
    notify("autotest", "Tests have fully passed", 'low')
  end
end
