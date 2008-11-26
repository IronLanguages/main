def m
  puts 'm'
  yield
end

puts 'A'

begin

t = Thread.new {
  begin
    puts 'B'
    m {
      begin
        puts 'C'
        return
      rescue ThreadError
        puts 'R1'                      # rescues here
      end  
    }
    puts 'D'
  rescue ThreadError
    puts 'R2'
  end  
}

rescue ThreadError
  puts 'R3'
end


puts 'E'

begin
  t.join
rescue ThreadError
  putst 'R4'
end  