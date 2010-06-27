##
## $Release: 2.6.5 $
## copyright(c) 2006-2009 kuwata-lab.com all rights reserved.
##

module Erubis


  ##
  ## base error class
  ##
  class ErubisError < StandardError
  end


  ##
  ## raised when method or function is not supported
  ##
  class NotSupportedError < ErubisError
  end


end
