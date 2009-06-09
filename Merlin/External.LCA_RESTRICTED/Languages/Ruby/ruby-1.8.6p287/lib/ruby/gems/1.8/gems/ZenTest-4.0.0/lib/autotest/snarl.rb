# special thanks to: Patrick Hurley <phurley@gmail.com>
# requires the ruby-snarl gem.

begin require 'rubygems'; rescue LoadError; end
require 'snarl'

module Autotest::Snarl  
  def self.icon
    # icons from http://www.famfamfam.com/lab/icons/silk/
    path = File.join(File.dirname(__FILE__), "/../icons")
    {
      :green => "#{path}/accept.png",
      :red    => "#{path}/exclamation.png",
      :info   => "#{path}/information.png"
    }
  end
  
  def self.snarl title, msg, ico = nil
    Snarl.show_message(title, msg, icon[ico])
  end

  Autotest.add_hook :run do  |at|
    snarl "Run", "Run" unless $TESTING
  end

  Autotest.add_hook :red do |at|
    failed_tests = at.files_to_test.inject(0){ |s,a| k,v = a;  s + v.size}
    snarl "Tests Failed", "#{failed_tests} tests failed", :red
  end

  Autotest.add_hook :green do |at|
    snarl "Tests Passed", "All tests passed", :green #if at.tainted 
  end

  Autotest.add_hook :run do |at|
    snarl "autotest", "autotest was started", :info unless $TESTING
  end

  Autotest.add_hook :interrupt do |at|
    snarl "autotest", "autotest was reset", :info unless $TESTING
  end

  Autotest.add_hook :quit do |at|
    snarl "autotest", "autotest is exiting", :info unless $TESTING
  end

  Autotest.add_hook :all do |at|_hook
    snarl "autotest", "Tests have fully passed", :green unless $TESTING
  end

end
