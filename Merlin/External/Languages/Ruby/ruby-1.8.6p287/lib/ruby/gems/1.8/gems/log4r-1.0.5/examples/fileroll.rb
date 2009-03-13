# How to use RollingFileOutputter

$: << "../src"
require 'log4r'
include Log4r

puts "this will take a while"

# example of log file being split by time constraint 'maxtime'
config = {
  "filename" => "logs/TestTime.log",
  "maxtime" => 10,
  "trunc" => true
}
timeLog = Logger.new 'WbExplorer'
timeLog.outputters = RollingFileOutputter.new("WbExplorer", config)
timeLog.level = DEBUG

# log something once a second for 100 seconds
100.times { |t|
  timeLog.info "blah #{t}"
  sleep(1.0)
}

# example of log file being split by space constraint 'maxsize'
config = {
  "filename" => "logs/TestSize.log",
  "maxsize" => 16000,
  "trunc" => true
}
sizeLog = Logger.new 'WbExplorer'
sizeLog.outputters = RollingFileOutputter.new("WbExplorer", config)
sizeLog.level = DEBUG

# log a large number of times
100000.times { |t|
  sizeLog.info "blah #{t}"
}

puts "done! check the two sets of log files in logs/ (TestTime and TestSize)"
