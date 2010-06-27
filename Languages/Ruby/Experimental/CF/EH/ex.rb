class Class
  alias old_new new
  def new *a,&b
    puts "{self.inspect}::new(#{a.inspect}, &#{b.inspect})"
    r = old_new *a,&b
    puts r.object_id
    r
  end
end

class E < Exception
  def initialize *a
    puts "{self.inspect}::initialize(#{a.inspect})"    
  end
  
  def respond_to? name
    puts "{self.inspect}::respond_to?(#{name})"
    super
  end
  
  def exception *a
    puts "{self.inspect}::exception(#{a.inspect})"   
    super
  end
  
  def backtrace
    puts "{self.inspect}::backtrace"
    ['goo']
  end

  def set_backtrace value
    puts "{self.inspect}::set_backtrace(#{value.inspect})"
    value + ['hoo']
  end
end

puts "--- raise(Exception, string, array) --------------"

begin
  puts '> raise:'
  raise E.new(1,2), "foo", ["x","y"]
rescue Exception => e
  puts '> rescue:'
  p e.object_id
  p $@
end

puts "--- raise(Exception, array) --------------"

begin
  puts '> raise:'
  raise E.new(1,2), ["x","y"]
rescue Exception => e
  puts '> rescue:'
  p e.object_id
  p $@
end

puts "--- raise(Exception, string) --------------"

begin
  puts '> raise:'
  raise E.new(1,2), "foo"
rescue Exception => e
  puts '> rescue:'
  p e.object_id
  p $@
end

puts "--- raise(Exception) --------------"

begin
  puts '> raise:'
  raise E.new(1,2)
rescue Exception => e
  puts '> rescue:'
  p e.object_id
  p $@
end

puts "--- raise(Class) --------------"

begin
  puts '> raise:'
  raise E
rescue Exception => e
  puts '> rescue:'
  p e.object_id
  p $@
end