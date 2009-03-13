#
# Get autotest.el from http://www.emacswiki.org/cgi-bin/wiki/RyanDavis
#

module Autotest::Emacs
  @@client_cmd = 'emacsclient -e'

  def self.command= o
    @@client_cmd = o
  end

  def self.emacs_autotest status
    `#{@@client_cmd} \"(autotest-update '#{status})\"`
    nil
  end

  Autotest.add_hook :run_command do  |at|
    emacs_autotest :running
  end

  Autotest.add_hook :green do  |at|
    emacs_autotest :passed
  end

  Autotest.add_hook :all_good do  |at|
    emacs_autotest :passed
  end

  Autotest.add_hook :red do  |at|
    emacs_autotest :failed
  end

  Autotest.add_hook :quit do  |at|
    emacs_autotest :quit
  end
end
