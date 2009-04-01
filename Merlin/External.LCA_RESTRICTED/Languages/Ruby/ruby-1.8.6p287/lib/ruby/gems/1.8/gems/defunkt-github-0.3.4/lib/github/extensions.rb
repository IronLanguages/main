# define #try
class Object
  def try
    self
  end
end

class NilClass
  klass = Class.new
  klass.class_eval do
    instance_methods.each { |meth| undef_method meth.to_sym unless meth =~ /^__(id|send)__$/ }
    def method_missing(*args)
      self
    end
  end
  NilProxy = klass.new
  def try
    NilProxy
  end
end

# define #tap
class Object
  def tap(&block)
    block.call(self)
    self
  end
end

# cute
module Color
  COLORS = { :clear => 0, :red => 31, :green => 32, :yellow => 33 }
  def self.method_missing(color_name, *args)
    color(color_name) + args.first + color(:clear)
  end
  def self.color(color)
    "\e[#{COLORS[color.to_sym]}m"
  end
end
