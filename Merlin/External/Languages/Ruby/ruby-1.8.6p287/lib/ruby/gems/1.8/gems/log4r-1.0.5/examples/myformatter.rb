# try out a custom formatter

$: << '../src'

require "log4r"

class MyFormatter <  Log4r::Formatter
  def format(event)
    buff = "The level is #{event.level} and has "
    buff += "name '#{Log4r::LNAMES[event.level]}'\n"
    buff += "The logger is '#{event.name}' "
    buff += "and the data type is #{event.data.class}\n"
    buff += "Let's inspect the data:\n"
    buff += event.data.inspect + "\n"
    buff += "We were called at #{event.tracer[0]}\n\n"
  end
end

log = Log4r::Logger.new('custom formatter')
log.trace = true
log.add Log4r::StdoutOutputter.new('stdout', :formatter=>MyFormatter)
log.info [1, 2, 3, 4]
log.error "A log statement"
