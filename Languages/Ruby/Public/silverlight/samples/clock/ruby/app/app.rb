require 'Silverlight'

class Clock < SilverlightApplication
  use_xaml :type => Canvas

  def self.start
    clock = self.new(Time.now)
    clock.set_time
  end

  def initialize(time)
    @time = time
  end

  def set_time
    root.hour_animation.from,   root.hour_animation.to   = hour
    root.minute_animation.from, root.minute_animation.to = minute
    root.second_animation.from, root.second_animation.to = second
  end

private

  def hour
    [from_angle(@time.hour, 1, @time.min / 2), to_angle(@time.hour)]
  end

  def minute
    [from_angle(@time.min), to_angle(@time.min)]
  end

  def second
    [from_angle(@time.sec), to_angle(@time.sec)]
  end

  def from_angle(time, divisor = 5, offset = 0)
    ((time / (12.0 * divisor)) * 360) + offset + 180
  end

  def to_angle(time)
    from_angle(time) + 360
  end
end

Clock.start
