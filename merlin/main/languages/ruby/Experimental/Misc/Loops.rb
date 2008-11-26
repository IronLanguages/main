# prints 1..10, var is not controlling the loop

for var in 1..10
  puts "var = #{var}"
  if var > 5
    var = var + 2
  end
end

puts '---'

i = 3
x = while i > 0 do 
  puts i
  i = i - 1
end
puts x

puts '---'

i = 3
x = while i > 0 do 
  puts i
  if i == 2 then
    eval("break")
  end
  i = i - 1
end
puts x

puts '---'

i = 3
x = while i > 0 do 
  puts i
  if i == 2 then
    eval("break 'foo'")
  end
  i = i - 1
end
puts x

puts '---'

i = 3
j = 2
x = while i > 0 do 
  puts i
  if i == 2 and j > 0 then
    j = j - 1
    eval('redo')
  end
  i = i - 1
end
puts x

puts '---'

def foo
  eval('break')
rescue LocalJumpError => e
  puts 'A'
end

begin
  foo
rescue LocalJumpError => e
  puts 'B'
end

puts '---'

def foo2
  i = 0
  while i < 5 do 
    eval("
      begin
        i += 1
        puts i
        eval('break')
      rescue LocalJumpError => e
        puts 'A'
      end");
  end
  
  eval("
      begin
        i += 1
        puts i
        eval('break')
      rescue LocalJumpError => e
        puts 'C'
      end");
end

begin
  foo2
rescue LocalJumpError => e
  puts 'B'
end

puts '---'

x = begin 1; 2; end 
puts x

puts '---'

while begin puts 'foo'; break; true; end do
  puts 'bar'
end

puts '---'

i = 0
while begin puts 'foo'; redo; puts 'baz'; true; end do
  puts i
  i += 1
  if i == 5 then break end
end

puts '---'

i = 0
while begin puts i; i += 1; next unless i > 5; puts 'baz'; i < 10; end do
  puts 'bar'
  i += 1
end

puts '---'

i = 0
until begin puts i; i += 1; next unless i > 5; puts 'baz'; i >= 10; end do
  puts 'bar'
  i += 1
end