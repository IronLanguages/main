require 'mock'

class C
  def test_iv name
    instance_variable_set(name, 0) rescue p $!
    instance_variable_get(name) rescue p $!
    instance_variable_defined?(name) rescue p $!
    remove_instance_variable(name) rescue p $!    
  end
  
  def foo
  end
end

class SSym < String
  def respond_to? name
    puts "?Sym #{name}"
    super
  end
end

[
  Sym.new(:@foo),
  SSym.new("@foo"),
  :@foo,
  :@foo.to_i,
  Sym.new(:@foo, :@foo),
  1111111111111111111111111111,
  1.23
].each { |value| 
  C.new.test_iv value 
  puts '---'  
}
