x = 1
y = 2

puts 'foo' if 
x == 
1

puts x = if true then false else true end         # false

puts x = 
if true then false else true end                  # false

puts x = 
if 
true then false else true end                     # false

puts x = 
if 
true 
then false else true end                          # false

puts x = 
if 
true 
then 
false else true end                               # false

puts x = 
if 
true 
then 
false 
else true end                                     # false

puts x = 
if 
true 
then 
false 
else 
true end                                          # false

puts x = 
if 
true 
then 
false 
else 
true 
end                                               # false

puts (if true then false else true end)           # false
puts true if true                                 # true

class C; end      
class C; def foo; end; end
class C; def foo() end; end

# class C; def foo end; end
#                     ^
# syntax error, unexpected kEND, expecting '\n' or ';'

# class C end      
#            ^  
# syntax error, unexpected kEND, expecting '<' or '\n' or ';'


