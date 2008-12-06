$a = 4
b = 2

puts %w{a b c}            # ["a","b","c"]
puts "---"      
puts %W{a b c}            # ["a","b","c"]
puts "---" 
puts %w{a #{b} c}         # ['a','#{b}','c']
puts "---"
puts %W{a #{b + 1} c}     # ['a','3','c']
puts "---"
puts %W{$a #{b + 1} c}    # ['$a','3','c']
puts "---"
puts %W{#$a #{b + 1} c}   # ['4','3','c']
puts "---"
puts %w{#$a #{b + 1} c}   # ['#$a','#{b','+','1','c']

