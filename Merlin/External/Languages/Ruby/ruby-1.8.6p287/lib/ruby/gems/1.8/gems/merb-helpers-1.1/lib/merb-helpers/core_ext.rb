require "date"
require File.dirname(__FILE__) + '/date_time_formatting'
class Date
  include DateAndTimeFormatting
  
  # Converts a Date instance to a Time, where the time is set to the beginning of the day.
  # The timezone can be either :local or :utc (default :utc).
  #
  # ==== Examples:
  #   date = Date.new(2007, 11, 10)
  #   date.to_s                      # => 2007-11-10
  #
  #   date.to_time                   # => Sat Nov 10 00:00:00 UTC 2007
  #   date.to_time(:utc)             # => Sat Nov 10 00:00:00 UTC 2007
  #   date.to_time(:local)           # => Sat Nov 10 00:00:00 -0800 2007
  #
  def to_time(form = :utc)
    ::Time.send("#{form}", year, month, day)
  end
  
  def to_date; self; end
  
  def formatted(format=:default)
    format = Date.formats[format] 
    if format.nil?
      self.to_s 
    else
      self.strftime(format)
    end
  end
  
end

class Time
  include DateAndTimeFormatting
  
  # Ruby 1.8-cvs and 1.9 define private Time#to_date
  %w(to_date to_datetime).each do |method|
    public method if private_instance_methods.include?(method)
  end
  
  def to_time; self; end
end

# Truncates a string to the given length and appends the given suffix if the string is, in fact, truncated.
#
# ==== Examples:
#  "This is a long string right here".truncate(10, "...")  #=> "This is..."

class String
  def truncate(length = 30, truncate_string = "...")
    return self unless self.length > length
    length = length - truncate_string.split(//).length
    self[0...length] + truncate_string
  end
end
