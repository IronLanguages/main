$i = 0

def foo
	if $i == 0
		puts 'retrying'
		$i += 1
		retry
	end
end

def goo
	puts 'goo.begin'
	raise
rescue
	foo	
end

goo



  