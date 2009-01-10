=begin
while-loop cannot be retried

output:
A
times
retrying
A
times

=end


$i = 0;

begin
	puts 'A'
	raise
rescue		
	while true
		puts 'times'
		if $i == 0
			$i = 1
			puts 'retrying'
			retry
		end
		
		break;
	end
end