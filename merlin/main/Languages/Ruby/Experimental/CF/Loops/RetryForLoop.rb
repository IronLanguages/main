def foo x
  puts "foo(#{x})"
  x * ($i + 1)
end

$i = 0

for i in [foo(1), foo(2), foo(3)] do
  puts "i = #{i}"
  
  if $i == 0 then
    $i = 1
    retry
  end  
end