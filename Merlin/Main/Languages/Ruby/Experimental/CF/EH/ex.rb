class Class
  alias old_new new
  def new *a,&b
    puts "#{self}::new(#{a.inspect}, &#{b.inspect})"
    old_new *a,&b
  end
end

class X
  def respond_to? name
    puts "XXXX?#{name}"
  end
  
  def to_s
    '---'
  end
end

puts "#{X.new}::xxx"

class E < Exception
  def initialize *a
    puts "#{self}::initialize(#{a.inspect})"    
  end
  
  def respond_to? name
    puts "?#{name}"
    super
  end
  
  def exception *a
    puts "#{self}::exception(#{a.inspect})"   
    super 
  end
  
  def backtrace
    puts "#{self}::backtrace"
    ['goo']
  end

  def backtrace= value
    puts "#{self}::backtrace(#{value.inspect})"
    value + ['hoo']
  end
end

class C
  def respond_to? name
    puts "?#{self}::#{name}"
    super
  end
  
  def exception
    puts "#{self}::exception"
    E.new
  end
end


#set_trace_func proc { |*args|
#  if args[0] == 'call' then
#    p args
#  end
#}


#E.exception("foo", ["x","y"])	

begin
  raise E.new, "foo", ["x","y"]
rescue Exception
  p $@
end