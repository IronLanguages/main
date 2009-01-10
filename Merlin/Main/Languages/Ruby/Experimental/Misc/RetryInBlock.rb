i = 0
3.times { |x| 
  puts x
  i = i + 1
  if i == 2 then retry end
}