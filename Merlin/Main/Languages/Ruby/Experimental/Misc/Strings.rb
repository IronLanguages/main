b = true
a = 1
$a = 10

def h(a)
  "[" + a + "]"
end

# precendence: static concat has higher precedence than a method call (RubyWay is wrong on pg 43)

puts "[" "a" "b".center(6) + "]"                  # ' [ab  ]'
puts "[" + "a" + "b".center(6) + "]"              # '[a  b   ]'
puts "[" + "a" + ("b".center(6)) + "]"            # '[a  b   ]'

puts "}"                                          # '}'
puts "#}"                                         # '#}'  
puts "#"                                          # '#'  
puts "##"                                         # '##'  
puts "a#{h" "}b"                                  # 'a[ ]b'
puts "a#{h" 15 + 10 "}b"                          # 'a[ 15 + 10 ]b'
puts "\#{a}"                                      # '#{a}'
puts "\#a"                                        # '#a'
puts "#{a}"                                       # '1'
puts "#a"                                         # '#a'
puts "\#%\##"                                     # '#%##'

puts "#{}"                                        # ''
puts "#{if b then 1 else 2 end}"                  # '1'
puts "#{"#{1 + 1}"}"                              # '2'
puts "#{x = 1}#{y = x + 2}"                       # '13'
puts "#{z = "#{x = 3}"}#{y = x + 2}"              # '35'

puts "x#$a x"                                     # 'x10 x'
puts "x\#$a"                                      # 'x#$a'
puts "x#$"a blah 346 ^&(*!@~)_+ "[0]              # 'x'
puts "x#$"a blah 346 ^&(*!@~)_+ "[-11..-1]        # '^&(*!@~)_+'

#error puts "#{"

b = "foo"

puts %q{abc}                                      # 'abc'
puts %q{a#{b}c}                                   # 'a#{b}c'
puts %Q{abc}                                      # 'abc'
puts %Q{a#{b}c}                                   # 'afooc'
puts %{a#{b}c}                                    # 'afooc'



