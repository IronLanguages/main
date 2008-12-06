def bar  
  begin
    eval('
      begin
        eval("eval(\'retry\')")
      rescue Exception => e
        puts "E"
      end
    ')
  rescue
    puts 'A'
  end  
rescue LocalJumpError => e
  puts 'B'
end

def foo
  bar
rescue LocalJumpError => e
  puts 'C'
end

foo
  
