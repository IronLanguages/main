# How to use RemoteOutputter. See logserver.rb first.

$: << File.join('..','src')
require 'log4r'
require 'log4r/outputter/remoteoutputter'
include Log4r

Logger.new('log4r').add 'stdout'        # to see what's going on inside
RemoteOutputter.new 'remote',           # make a RemoteOutputter
    :uri=>'tcpromp://localhost:9999',   # where our LogServer is
    :buffsize=>10                       # buffer 10 before sending to LogServer
Logger.new('client').add('remote')      # give 'remote' to a 'client' Logger

# we're done with setup, now let's log
def log(l)
  l.debug "debugging"
  l.info "a piece of info"
  l.warn "Danger, Will Robinson, danger!"
  l.error "I dropped by Wookie! :("
  l.fatal "kaboom!"
end

5.times { log(Logger['client']) }      # do a bunch of logging
Logger['client'].info "Bye Bye from client!"
Outputter['remote'].flush              # flush the RemoteOutputter
