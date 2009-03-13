# :include: log4r/rdoc/log4r
#
# == Other Info
#
# Author::      Leon Torres
# Version::     $Id: log4r.rb,v 1.10 2002/08/20 04:15:04 cepheus Exp $

require "log4r/outputter/fileoutputter"
require "log4r/outputter/consoleoutputters"
require "log4r/outputter/staticoutputter"
require "log4r/outputter/rollingfileoutputter"
require "log4r/formatter/patternformatter"
require "log4r/loggerfactory"

module Log4r
  Log4rVersion = [1, 0, 1].join '.'
end
