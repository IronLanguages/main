require 'rubygems'
require 'rake'
require 'rake/testtask'
require 'echoe'

Echoe.new('activerecord-sqlserver-adapter','2.2.19') do |p|
  p.summary               = "SQL Server 2000, 2005 and 2008 Adapter For Rails."
  p.description           = "SQL Server 2000, 2005 and 2008 Adapter For Rails."
  p.author                = ["Ken Collins","Murray Steele","Shawn Balestracci","Joe Rafaniello","Tom Ward"]
  p.email                 = "ken@metaskills.net"
  p.url                   = "http://github.com/rails-sqlserver"
  p.runtime_dependencies  = ["dbi =0.4.1","dbd-odbc =0.2.4"]
  p.include_gemspec       = false
  p.ignore_pattern        = ["autotest/*","*.gemspec","lib/rails-sqlserver-2000-2005-adapter.rb"]
  p.project               = 'arsqlserver'
end

Dir["#{File.dirname(__FILE__)}/tasks/*.rake"].sort.each { |ext| load(ext) }
