# Provides a a simple way of calling time units and to see the elapsed time between 2 moments
# ==== Examples
#   142.minutes => returns a value in seconds
#   7.days => returns a value in seconds
#   1.week => returns a value in seconds
#   2.weeks.ago => returns a date
#   1.year.since(time) => returns a date
#   5.months.since(2.weeks.from_now) => returns a date
module TimeDSL
  
  def second
    self * 1
  end
  alias_method :seconds, :second
  
  def minute
    self * 60
  end
  alias_method :minutes, :minute
  
  def hour
    self * 3600
  end
  alias_method :hours, :hour
  
  def day
    self * 86400
  end
  alias_method :days, :day
  
  def week
    self * 604800
  end
  alias_method :weeks, :week
  
  def month
    self * 2592000
  end
  alias_method :months, :month
  
  def year
    self * 31471200
  end
  alias_method :years, :year
  
  # Reads best without arguments:  10.minutes.ago
  def ago(time = ::Time.now)
    time - self
  end
  alias :until :ago
  
  # Reads best with argument:  10.minutes.since(time)
  def since(time = ::Time.now)
    time + self
  end
  alias :from_now :since
end

Numeric.send :include, TimeDSL