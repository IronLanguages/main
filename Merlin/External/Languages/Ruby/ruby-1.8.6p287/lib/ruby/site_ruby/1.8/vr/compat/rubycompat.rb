require 'Win32API'

if RUBY_VERSION < "1.6"  # for 1.5 or 1.4
  def Win32API.new(dll,func,args,retval)
    args = args.split(//) if args.is_a?(String)
    super dll,func,args,retval
  end
  class Object
    alias class :type
  end

elsif RUBY_VERSION < "1.7" # for 1.6


#elsif RUBY_VERSION < "1.8" # for 1.7

end

