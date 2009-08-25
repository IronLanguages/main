# = DateFileOutputter
#
# Subclass of FileOutputter that changes the log file daily. When a new
# day begins, a new file is created with the date included in the name.
#
# == Usage
#
#   df_out = DateFileOutputter.new('name',
#              :dirname="/tmp", :date_pattern=>"%m-%d" 
#            )
#
# == Rate of Change
#
# A new logfile is created whenever the current time as formatted by the date
# pattern no longer matches the previous time. (This is a simple String 
# comparison.) So, in order to change the frequency of the rollover, just
# alter the date pattern to match how fast the files should be generated. 
# For instance, to generate files by the minute,
#
#   df_out.date_pattern = "%M"
#
# This causes the following files to show up one minute apart, asuming the
# script starts at the 4th minute of the hour:
#
#   file_04.rb
#   file_05.rb
#   file_06.rb
#   ...
#
# The only limitation of this approach is that the precise time cannot be
# recorded as the smallest time interval equals the rollover period for this
# system.

require "log4r/outputter/fileoutputter"
require "log4r/staticlogger"

module Log4r

  # Additional hash arguments are:
  #
  # [<tt>:dirname</tt>]         Directory of the log file
  # [<tt>:date_pattern</tt>]    Time.strftime format string (default is "%Y-%m-%d")

  class DateFileOutputter < FileOutputter
    DEFAULT_DATE_FMT = "%Y-%m-%d"

    def initialize(_name, hash={})
      @DatePattern = (hash[:date_pattern] or hash['date_pattern'] or
                      DEFAULT_DATE_FMT)
      @DateStamp = Time.now.strftime( @DatePattern);
      _dirname = (hash[:dirname] or hash['dirname'])
      # hash[:dirname] masks hash[:filename]
      if _dirname
        if not FileTest.directory?( _dirname)
          raise StandardError, "'#{_dirname}' must be a valid directory", caller
        end
        @filebase = File.basename( $0, '.rb') + ".log"
      else
        @filebase = File.basename((hash[:filename] or hash['filename'] or ""))
      end
      hash['filename'] = File.join(_dirname,
                    @filebase.sub(/(\.\w*)$/, "_#{@DateStamp}" + '\1'))
      super(_name, hash)
    end

    #######
    private
    #######

    # perform the write
    def write(data)
      change if requiresChange
      super
    end

    # construct a new filename from the DateStamp
    def makeNewFilename
        @DateStamp = Time.now.strftime( @DatePattern);
        @filename = File.join(File.dirname(@filename),
                    @filebase.sub(/(\.\w*)$/, "_#{@DateStamp}" + '\1'))
    end

    # does the file require a change?
    def requiresChange
      _DateStamp = Time.now.strftime( @DatePattern);
      if not _DateStamp == @DateStamp
        @DateStamp = _DateStamp
        return true
      end
      false
    end

    # change the file 
    def change
      begin
        @out.close
      rescue
        Logger.log_internal {
          "DateFileOutputter '#{@name}' could not close #{@filename}"
        }
      end
      makeNewFilename
      @out = File.new(@filename, (@trunc ? "w" : "a"))
      Logger.log_internal {
        "DateFileOutputter '#{@name}' now writing to #{@filename}"
      }
    end
  end

end
