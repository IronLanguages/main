=begin
output:

A
ppt.begin
block called 0
B
retrying
ppt.finally
A
ppt.begin
block called 1
B
ppt.end
ppt.finally

comment:
  Let B is the block/proc of the current frame (passed to the current function).
  Retry unwinds all frames whose call-sites passed B thru and jumps right before the last call-site that passed B thru 
  (the next call-site on the stack passed different block).
  Ensure clauses are called during stack unwinding.
  
=end

def retry_proc *,&p
	if ($i == 0)
	  puts 'retrying' 
	  $i = $i + 1
	  retry
	end  
end

def pass_proc_thru *,&p
	puts 'ppt.begin'
	yield
	retry_proc puts('B'), &p
	puts 'ppt.end'
ensure
    puts 'ppt.finally'	
end

$i = 0;

def retry_target
	pass_proc_thru(puts('A')) { 
		puts "block called #{$i}"
	}
end

retry_target


  