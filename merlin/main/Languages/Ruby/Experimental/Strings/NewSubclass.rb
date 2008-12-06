class Class
  alias old_new new
  alias old_allocate allocate
  
  def new *args, &x
    puts "new #{args[0]}"
    old_new(*args, &x)
  end
  
  def allocate *args, &x
    puts "allocate #{args[0]}"
    old_allocate(*args, &x)
  end
end

class S < String
end

x = S.new("foo")

p x.dump.class
p x.downcase.class
p x.upcase.class
p x.center(10).class