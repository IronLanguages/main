def foo
	throw :foo
ensure
	puts "A:", $!.inspect
end

catch :foo do
    begin
        foo
    rescue Exception
        puts "B:", $!.inspect
    end
end