# This is like moderateconfig.rb, but using an XML config
# please look at moderate.xml

$: << '../src'

require 'log4r'
require 'log4r/configurator'
include Log4r

# set any runtime XML variables
Configurator['logpath'] = './logs'
# Load up the config file
Configurator.load_xml_file('./moderate.xml')

# now repeat what moderateconfig.rb does
def do_logging(log)
log.debug "debugging"
log.info "a piece of info"
log.warn "Danger, Will Robinson, danger!"
log.error "I dropped my Wookie! :("
log.fatal "kaboom!"
end

Logger.each_logger{|logger| do_logging(logger) }
# stop here
