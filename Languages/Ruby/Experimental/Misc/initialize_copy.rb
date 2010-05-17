class C
  def initialize
    @x = 1
    @y = 2
    puts "#{self.inspect}.initialize"
  end
  
  def initialize_copy *c
    puts "#{self.inspect}.initialize_copy(#{c.inspect})"
  end

  def allocate
    puts "#{self.inspect}.allocate"
  end
end

c = C.new
c.taint
c.freeze

puts '-- dup:'
dup_c = c.dup
puts "frozen: #{dup_c.frozen?} tainted: #{dup_c.tainted?}"

puts '-- clone:'
clone_c = c.clone
puts "frozen: #{clone_c.frozen?} tainted: #{clone_c.tainted?}"
