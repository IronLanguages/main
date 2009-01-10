=begin

Exception not rescuable in the primary frame.

=end

def test_next
	puts "test_next.begin"
	next
	puts "test_next.end"
rescue
	puts "Unreachable"
end

def test_redo
	puts "test_redo.begin"
	redo
	puts "test_redo.end"
rescue
	puts "Unreachable"
end

begin
	test_next
rescue
	puts "E: #{$!}"
end

begin
	test_next {}
rescue
	puts "E: #{$!}"
end

begin
	test_redo
rescue
	puts "E: #{$!}"
end

begin
	test_redo {}
rescue
	puts "E: #{$!}"
end