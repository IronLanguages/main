class Class
  p private_instance_methods(false)
end

class C
  def foo
  end
end

class B
  def bar
  end
end

p C.send(:initialize_copy, B) rescue p $!
p Class.new.send(:initialize_copy, B) rescue p $!

class C
  def foo
  end
end

class Class
  alias xi initialize_copy

  def initialize_copy a
    puts "init_copy"
    p name
    r = xi(a)
    p name    
    r
  end
end

x = C.clone
p x
p x.instance_methods(false)
p x.ancestors

puts '-----'

class Class
  def initialize_copy a
    puts 'init_copy'    
  end
end

x = C.clone
p x
p x.instance_methods(false)
p x.ancestors

x.send :xi, B
p x
p x.instance_methods(false)
p x.ancestors

x.send :xi, C rescue p $!
