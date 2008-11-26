puts 'Begin'
begin
  puts 'Class'
  class C
    puts 'NoRaise'
  rescue
    puts 'Rescue'
  else
    puts 'Else'
  ensure
    puts 'Ensure'
  end
  puts 'ClassEnd'
rescue
  puts 'OutterRescue'
end
puts 'End'