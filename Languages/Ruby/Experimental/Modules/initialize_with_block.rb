class Module
  public :initialize  
end

X = Module.new { puts 'xxx' }

puts '-1-'
p(X.initialize)
puts '-2-'
p(X.initialize { puts 'foo' })
puts '-3-'
p(X.initialize { break 'foo' })
puts '-4-'
p(Module.new { break 'foo' })

class MM < Module
  def initialize *a, &b
    puts 'init'
  end
end

# doesn't call the block (=> block yield is in initialize, not in factory)
puts '-5-'
p(MM.new { puts 'yyy' })
p(MM.new { break 'yyy' })
