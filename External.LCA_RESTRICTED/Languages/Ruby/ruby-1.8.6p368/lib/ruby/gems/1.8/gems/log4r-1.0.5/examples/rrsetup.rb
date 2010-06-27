# This is a real config file used by a game that I'm working on
# The XML config file is called rrconfig.xml

$: << File.join('..','src')
require 'log4r'
require 'log4r/configurator'
include Log4r

# How to format component data - low noise
class CompFormatter < Formatter
  def format(event)
    buff = event.name + "> "
    if event.data.kind_of?(String) then buff += event.data
    else buff += event.data.inspect end
    return buff + "\n"
  end
end

# Set the logpath. Eventually, this will be determined from the environment.
Configurator['logpath'] = './logs'
Configurator.load_xml_file('rrconfig.xml')

# the rest is an example

Robot = {"name"=>"twonky", "row"=>"3", "col"=>"4"}

def do_logging(log)
log.comp3  Robot
log.comp2 Robot
log.comp1 Robot
log.data "this is a piece of data".split
log.debug "debugging"
log.info "a piece of info"
log.warn "Danger, Will Robinson, danger!"
log.error "I dropped my Wookie! :(" 
log.fatal "kaboom!"
end

Logger.each_logger {|logger| do_logging(logger)}

# you can see the results onscreen and in logs/game.log
# logs/data.log and logs/component.log
