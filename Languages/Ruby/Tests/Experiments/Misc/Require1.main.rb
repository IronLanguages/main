a = 'a'

puts require('Require1.a') # true
puts foo()                 # foo
# error: puts self.foo()   # private method `foo' called
puts instance_variables()  # @f
puts a                     # a
puts $g                    # 1
puts @f                    # goo
puts C                     # C

# wrapped

puts load('Require1.b.rb', true) # true
# error: puts fooB()          # undefined
# error: puts self.fooB()     # private method `foo' called
puts instance_variables()     # @f
# error: puts aB              # undefined
puts $g                       # 1
puts @fB                      # nill
# error: puts MB              # uninitialized
# error: include MB           # uninitialized