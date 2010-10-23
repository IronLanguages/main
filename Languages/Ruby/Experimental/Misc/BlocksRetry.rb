$retry = true

def do_until(cond)
  puts 'begin'
  
  if cond                 
    $retry = false         # nop
    return                 # return
  end
  
  puts 'yield:'
  yield
  puts 'retry:'
  
  return                   # retry
  
  puts 'end'
end

i = 0

while $retry               # nop  
  do_until(i > 10) do
    puts i
    i += 1
  end
end                        # nop
