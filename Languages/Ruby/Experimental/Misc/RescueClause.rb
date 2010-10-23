class Module
  def ===(other)
    puts '1:==='
    puts $!
    true
  end
end

class MyExc < Exception
end

begin
  raise MyExc, "my-message"
rescue IOError
  puts $!
  puts 'rescued'
end
