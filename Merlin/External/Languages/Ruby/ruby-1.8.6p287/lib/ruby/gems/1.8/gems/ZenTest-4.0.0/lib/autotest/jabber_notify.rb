begin; require 'rubygems'; rescue LoadError; end
require 'xmpp4r-simple'

module Autotest::JabberNotify
  @@recipients = []
  @@account = nil
  @@password = nil
  @@use_svn = true
  @@report_every_run = true

  @@im = nil
  @@last_rev = nil
  @@green = false

  def self.recipients= o
    @@recipients = o
  end

  def self.account= o
    @@account = o
  end

  def self.password= o
    @@password = o
  end

  def self.use_svn= o
    @@use_svn = o
  end

  def self.report_every_run= o
    @@report_every_run = o
  end

  def self.im
    unless @@im
      puts "# creating im client"
      @@im = Jabber::Simple.new(@@account,@@password)
      sleep(2)  # need this or else the first announcement may cause an error
    end
    if !@@im.connected?
      puts "# reconnecting to #{@@account}"
      @@im.reconnect
    end
    @@im
  end

  def self.notify(msg)
    @@recipients.each do |contact|
      self.im.deliver(contact, msg)
    end
  end

  def self.status(status)
    rev = self.svn_release
    status = "#{rev}#{status}"
    self.im.status(:chat, status)
  end

  def self.svn_release
    if @@use_svn
      rev = `svn info`.match(/Revision: (\d+)/)[1]
      return "r#{rev} "
    end
  end

  # hooks

  Autotest.add_hook :run do |at|
    notify "autotest started"
  end

  Autotest.add_hook :run_command do |at|
    status "testing"
  end

  Autotest.add_hook :ran_command do |at|
    rev = self.svn_release
    if @@report_every_run or rev != @@last_rev
      @@last_rev = rev
      output = at.results.join
      failed = output.scan(/^\s+\d+\) ((?:Failure|Error):\n.*?\.*?\))/).flatten
      failed.map! {|f| f.gsub!(/\n/,' '); f.gsub(/^/,'- ') }
      time = output.scan(/Finished in (.*?)\n/)
      if failed.size > 0 then
        notify "Tests Passed\n#{time}\n" if !@@green
        @@green = true # prevent repeat success notifications
      else
        @@green = false
        notify "#{failed.size} Tests Failed\n" + failed.join("\n")
      end
    end
  end

  Autotest.add_hook :green do |at|
    status "Tests Pass"
  end

  Autotest.add_hook :red do |at|
    status "Tests Failed"
  end

  Autotest.add_hook :quit do |at|
    notify "autotest is exiting"
    self.im.disconnect
  end

  Autotest.add_hook :all do |at|_hook
    notify "Tests have fully passed" unless $TESTING
  end
end
