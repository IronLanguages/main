module M
  def foo
  end

  def M.mmm
    
  end
end

module N
  def bar
  end
  
  def N.nnn
    
  end
end

module X
end

puts '--- cannot initialize frozen module:'
X.freeze
p X.send(:initialize_copy, N) rescue p $!

puts '--- direct call:'

M.send(:initialize_copy, N)

p M.instance_methods(false)
p M.singleton_methods(false)

puts '--- name:'

$m = Module.new
$m.send :initialize_copy, M
p $m.name

##########################################

class Module
  def initialize_copy *a
    puts 'init_copy'
  end
end

puts '--- clone:'

M_clone = M.clone

p M_clone.instance_methods(false)
p M_clone.singleton_methods(false)

puts '--- dup:'

M_dup = M.dup

p M_dup.instance_methods(false)
p M_dup.singleton_methods(false)

puts '--- name:'

$m = Module.new
$m.send :initialize_copy, M
p $m.name


