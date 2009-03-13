# Now, for something more complicted
# Let's pretend this is the global config file for our app

$: << File.join('..','src')
require "log4r"

include Log4r                   # include Log4r to make things simple

Logger.root.level = DEBUG       # global level DEBUG

# suppose we want to have loggers for a Server and a Client class
# furthermore, we want the client gui to have its own logger. (You'll want
# one logger per class or so.)
# When the loggers are created, they are stored in a repository for further
# retreival at any point using a hash method call: Logger['name']

# server is stable, so only log ERROR and FATAL
Logger.new("server", ERROR)
# let's say we don't need the DEBUG junk for client logs
Logger.new("client", INFO)
# but we're still debugging the gui
debugger = Logger.new("client::gui", DEBUG)
debugger.trace = true     # we want to see where the log method was called

# Guilog is a child of client. In this case, any log events to the gui
# logger will also be logged to the client outputters. We can change
# that behavior by setting guilogger's 'additive' to false, but not yet.

# let's create the outputters
FileOutputter.new('server', :filename=>'logs/server.log', :trunc => false)
FileOutputter.new('client', :filename=>'logs/client.log')
FileOutputter.new('gui', :filename=>'logs/guidebug.log')
# additionally, we want ERROR and FATAL messages to go to stderr
StderrOutputter.new('console', :level=>ERROR)

# add the outputters
Logger['server'].add 'server', 'console'
Logger['client'].add 'client', 'console'
Logger['client::gui'].add 'gui'  # gui will also write to client's outputters

# That's it for config. Now let's use the loggers:

def do_logging(log)
  log.debug "debugging"
  log.info "a piece of info"
  log.warn "Danger, Will Robinson, danger!"
  log.error "I dropped my Wookie! :(" 
  log.fatal "kaboom!"
end

Logger.each_logger{|logger| do_logging(logger) }

# You can dynamically change levels and turn off tracing:
Logger['client'].level = OFF
Logger['client::gui'].trace = false

puts 'Only server should show Dynamic Change onscreen:'
Logger.each_logger{|logger| logger.fatal "Dynamic change." }
# logs/client.log file should not show "Dynamic change."
# logs/guidebug.log should not show the trace at "Dynamic change."

# we can also set our outputter to log only specified levels:

Outputter['console'].only_at ERROR
puts "Should only see ERROR next:"
do_logging Logger['server']
