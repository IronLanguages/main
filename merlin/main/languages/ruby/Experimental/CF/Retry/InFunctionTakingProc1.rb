=begin
output:

A
block called 0
B
retrying
A
block called 1
B

comment:
  Retry on Proc returns to the call-site that made the block, if the frame of the call-site is still alive.
  
=end

$i = 0;

def retry_proc *,&p
	if ($i == 0)
	  puts "retrying '#{p}'"
	  $i = $i + 1
	  retry
	end  
end

def block_def
	defp(puts('P')) { 
		puts "P block called"
	}
	defq(puts('Q')) { 
		puts "Q block called"
	}
end

def defp *,&p
	$p = p
end

def defq *,&q
	$q = q
end

def g1 *,&x
  puts '1.begin'
  retry_proc puts('C'), &$p
  puts '1.end'
ensure   
  puts '1.finally'
end

def g2 *,&x
  puts '2.begin'
  g1 puts('2.r'), &$p
  puts '2.end'
ensure   
  puts '2.finally'
end

def g3 *,&x
  puts '3.begin'
  g2 puts('3.r'), &$p         # jumps here, because this function's block differs from the one that is being retried
  puts '3.end'
ensure   
  puts '3.finally'
end

def g4 *,&x
  $p = x
  puts '4.begin'
  g3 puts('4.r'), &$q
  puts '4.end'
ensure   
  puts '4.finally'
end

def g5 *,&x
  puts '5.begin'
  g4(puts('5.r')) { puts 'z' }
  puts '5.end'
ensure   
  puts '5.finally'
end

block_def
g5




  