# retry cannot be "rescued"
# output: ABCA

i = 0
begin
  puts 'A'
  raise if i == 0 
rescue
  i += 1
  puts 'B'
  begin
    puts 'C'
    retry
  rescue Exception => e
    puts 'D'
    puts e
  end
end




  