class String
  alias old_dup dup
  
  def dup  
    puts 'duped'
    old_dup
  end  
end

class S < String
end

["", "a", "ab", "abc", "abcd"].each { |s| 
  puts "-- '#{s}' -- "
  
  p s.reverse    
  p s
  p s.reverse!
  p s
}

puts '---'

s = S.new("xyz")
s.taint
s.freeze

r = s.reverse

p r.class
p r.tainted?
p r.frozen?

s.reverse! rescue puts $!

