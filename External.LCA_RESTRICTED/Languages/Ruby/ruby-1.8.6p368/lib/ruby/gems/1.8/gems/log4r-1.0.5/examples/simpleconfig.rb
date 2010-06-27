# Simple configuration example.
# Where we configure just one logger and make it log to a file and stdout.

# add the path to log4r if it isn't installed in a ruby path
$: << File.join('..','src')
require "log4r"

# First things first, get the root logger and set its level to WARN.
# This makes the global level WARN. Later on, we can turn off all logging
# by setting it to OFF right here (or dynamically if you prefer)
Log4r::Logger.root.level = Log4r::WARN

# Remember: By specifying a level, we are saying "Include this level and
# anything worse." So in this case, we're logging WARN, ERROR and FATAL

# create a logger
log = Log4r::Logger.new("simpleconf")

# We want to log to $stderr and a file ./tmp.log

# Create an outputter for $stderr. It defaults to the root level WARN
Log4r::StderrOutputter.new 'console'
# for the file, we want to log only FATAL and ERROR and don't trunc
Log4r::FileOutputter.new('logfile', 
                         :filename=>'logs/simple.log', 
                         :trunc=>false,
                         :level=>Log4r::FATAL)

# add the outputters (this method accepts outputter names or references)
log.add('console','logfile')

# Now let's try it out:
log.debug "debugging"
log.info "a piece of info"
log.warn "Danger, Will Robinson, danger!"
log.error "I dropped my Wookie! :("
log.fatal "kaboom!"

# now run this and compare output to ./tmp.log
