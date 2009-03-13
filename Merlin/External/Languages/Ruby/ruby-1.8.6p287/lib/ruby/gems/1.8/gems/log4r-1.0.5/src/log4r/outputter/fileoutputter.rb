# :nodoc:
# Version:: $Id: fileoutputter.rb,v 1.5 2003/09/12 23:55:43 fando Exp $

require "log4r/outputter/iooutputter"
require "log4r/staticlogger"

module Log4r

  # Convenience wrapper for File. Additional hash arguments are:
  #
  # [<tt>:filename</tt>]   Name of the file to log to.
  # [<tt>:trunc</tt>]      Truncate the file?
  class FileOutputter < IOOutputter
    attr_reader :trunc, :filename

    def initialize(_name, hash={})
      super(_name, nil, hash)

      @trunc = Log4rTools.decode_bool(hash, :trunc, true)
      _filename = (hash[:filename] or hash['filename'])

      if _filename.class != String
        raise TypeError, "Argument 'filename' must be a String", caller
      end

      # file validation
      if FileTest.exist?( _filename )
        if not FileTest.file?( _filename )
          raise StandardError, "'#{_filename}' is not a regular file", caller
        elsif not FileTest.writable?( _filename )
          raise StandardError, "'#{_filename}' is not writable!", caller
        end
      else # ensure directory is writable
        dir = File.dirname( _filename )
        if not FileTest.writable?( dir )
          raise StandardError, "'#{dir}' is not writable!"
        end
      end

      @filename = _filename
      @out = File.new(@filename, (@trunc ? "w" : "a")) 
      Logger.log_internal {
        "FileOutputter '#{@name}' writing to #{@filename}"
      }
    end

  end
  
end
