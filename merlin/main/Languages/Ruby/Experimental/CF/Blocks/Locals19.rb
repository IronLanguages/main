# Observations:
# - block defines a lexical scope
# - variables defined in the block are not visible outside the block
# - variables defined before the block are visible in the block unless they are stated after ; in block params
# - semicolon is similar to comma 
#   - comma hides outside variables because it represents a local variable assignment
#   - the variables following the semicolon are not formal parameters though

def foo2
  puts '|a;x|'
  x = 1
  1.times { |a;x|
    puts "in-x = #{x}"
    x = 2
  }
  
  puts "x = #{x}"
  puts
end

def foo3
  puts '|a,x|'
  x = 1
  1.times { |a,x|
    puts "in-x = #{x}"
    x = 2
  }
  
  puts "x = #{x}"
  puts
end

def foo4
  puts '|a|'
  x = 1
  1.times { |a|
    puts "in-x = #{x}"
    x = 2
  }
  
  puts "x = #{x}"
  puts
end

foo2
foo3
foo4

puts '---'

def goo
  yield 1,2
end

goo { |x,y| p x,y}
goo { |x;y| p x,y}
