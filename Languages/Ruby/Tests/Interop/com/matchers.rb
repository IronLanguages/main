class ComPropertyMatcher
  def initialize(prop, *values)
    @property = prop
    @values = values
  end
  
  def matches?(com_obj)
    @com_obj = com_obj
    getter = @property.to_sym
    setter = "#{@property}=".to_sym
    @failed = []
    @values.each do |value|
      begin
        @com_obj.send(getter)
        @com_obj.send(setter, value)
        got = @com_obj.send(getter)
        unless got == value
          @failed << ["Expected '#{@property}' to have the value #{value}, but it was #{got}"]
        end
      rescue Exception => e
        @failed << ["Exception calling property #{@property} with value #{value}:\n#{e.message}\n"]
      end
    end

    @failed.empty?
  end

  def failure_message
    ["Failures testing property #{@property}", @failed.join("\n")]
  end
end

class Object
  def have_com_property(property, *values)
    ComPropertyMatcher.new(property, *values)
  end
end
