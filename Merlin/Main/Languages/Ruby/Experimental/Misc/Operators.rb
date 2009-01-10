x = false
y = true


puts z = x || y            # true
puts z                     # true
puts z = x or y            # false
puts z                     # false
puts (z = x) or y          # false
puts z                     # false
puts ((z = x) or y)        # true
puts z                     # false

puts 

z = false
puts x or y and z          # false
puts x or y && z           # false 
puts x || y and z          # true
puts x || y && z           # false

puts

puts false || 1            # 1 
puts nil || 2              # 2
puts true || 3             # true
puts 3 || true             # 3
puts 5 || 4                # 5
puts x ||= 6               # 6
puts y ||= 6               # true

puts

puts false && 1            # false
puts nil && 2              # nil
puts true && 3             # 3
puts 3 && true             # true
puts 5 && 4                # 4
puts x &&= 6               # 6
puts y &&= 6               # 6

puts

class C
  def foo=(x) puts "write(#{x})" end
  def foo() puts "read()"; "bar" end
end

c = C.new

puts c.foo + "1"              # read() bar1
puts c.foo = c.foo + "1"      # read() write(bar1) bar1
puts c.foo += "1"             # read() write(bar1) bar1

