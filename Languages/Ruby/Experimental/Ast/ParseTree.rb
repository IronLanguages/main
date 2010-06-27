if defined? IRONRUBY_VERSION
  require 'ir_parse_tree'
else
  require 'rubygems'
  require 'parse_tree' 
end

class Example
  def blah
    return 1 + 1
  end
end

class ParseTree
  p ancestors
  p instance_methods(false)
  class << self
    p instance_methods(false)
  end  
  p constants
end

pt = ParseTree.new false

# syntax error -> seg fault
p pt.parse_tree_for_string("1+1")

puts '-----'

module M
end

class C
  include M

  def C.foo
    
  end
  
  define_method(:goo) {
  }
  
  puts 'boo'
end

C.class_eval {
  def bar
  end
}

p pt.parse_tree(C)

puts '-----'

module X
  def i_x
  end
end

class B
  def i_b
  end
end

class A < B
  include X

  def i_a
  end
end

# only declared methods are considered
p pt.parse_tree_for_method(A, :i_a) # [...]
p pt.parse_tree_for_method(A, :i_b) # [nil]
p pt.parse_tree_for_method(A, :i_x) # [nil]

puts '----'

ns = ParseTree::NODE_NAMES
p ns[ns.index(:scope)] = :foooooooo
p ns[ns.index(:defn)] = :baaaar   # bug, hardcoded "defn"
p pt.parse_tree_for_method(A, :i_a) # [...]

ParseTree::NODE_NAMES = nil
p pt.parse_tree_for_method(A, :i_a) # [...]




