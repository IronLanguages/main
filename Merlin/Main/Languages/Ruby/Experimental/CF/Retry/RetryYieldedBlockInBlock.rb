def u *a, &p
	puts 'u.begin'
	$p = p
	v
ensure	
	puts 'u.finally'
end

def v
	puts 'v.begin'
	raise   
rescue
	w &$p                                # y does retry the try-block
ensure	
	puts 'v.finally'
end

def w &b
	puts 'w.begin'
	y &$p                                # return value == retry singleton && block == passed block ==> do retry
ensure	
	puts 'w.finally'
end

def y
	puts 'y.begin'
	yield                                # return reason == retry ==> do retry
ensure	
	puts 'y.finally'
end

def foo
	puts 'foo.begin'
	u puts('Y') do
		puts 'outer-block.begin'
		yield 
		puts 'outer-block.end'
	end
	puts 'foo.end'
end	

$i = true

foo do
	puts 'inner-block.begin'
	if $i then
		$i = false
		puts 'retrying'
		retry
	end
	puts 'inner-block.end'
end

