def foo(a)
  puts "begin #{a}"
  if a == 3 then
      1.times { |x|
          puts 'in block 1'
          1.times { |y| 
              puts 'in block 2'
              return
          }
      }
  end
  
  foo(a - 1)
  puts "end #{a}"
end

puts 'before'
foo 5
puts 'after'