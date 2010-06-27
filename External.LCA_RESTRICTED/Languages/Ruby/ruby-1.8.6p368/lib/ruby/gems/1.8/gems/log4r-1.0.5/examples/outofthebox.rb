# Here's how to start using log4r right away
$: << File.join('..','src')                   # path if log4r not installed
require "log4r"

Log = Log4r::Logger.new("outofthebox")        # create a logger
Log.add Log4r::Outputter.stderr               # which logs to stdout

# do some logging
def do_logging
 Log.debug "debugging"
 Log.info "a piece of info"
 Log.warn "Danger, Will Robinson, danger!"
 Log.error "I dropped my Wookie! :("
 Log.fatal "kaboom!"
end
do_logging

# now let's filter anything below WARN level (DEBUG and INFO)
puts "-= Changing level to WARN =-"
Log.level = Log4r::WARN
do_logging
