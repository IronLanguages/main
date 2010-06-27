def foo
  puts 'Foo'
  retry
end

puts 'Begin'
i = 0
begin
  puts 'Class'
  begin
    puts 'InTry'
    if i == 0 then
      raise
    end 
    puts 'NoRaise'
  rescue
    puts 'Rescue'
    i = i + 1
    puts 'Retrying'
    #retry
    #eval('eval("retry")')
    #reties the block: 
    #1.times { |*| print 'B'; retry }
    foo
    puts 'Unreachable'
  else
    puts 'Else'
    #eval('retry');
  ensure
    puts 'Ensure'
  end
  puts 'ClassEnd'
rescue
  puts "OutterRescue:"
  puts $!.backtrace
end
puts 'End'