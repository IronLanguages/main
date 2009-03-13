module Autotest::RCov
  @@command, @@pattern = "rcov", "test/*.rb"

  def self.command= o
    @@command = o
  end

  def self.pattern= o
    @@pattern = o
  end

  Autotest.add_hook :all_good do |at|
    system "rake #{@@command} PATTERN=#{@@pattern}"
  end

  Autotest.add_hook :initialize do |at|
    at.add_exception 'coverage'
    at.add_exception 'coverage.info'
    false
  end
end

