def f1
  puts 'foo-begin'
  f2 { puts 'break'; break }  # break jumps after this statement
  puts 'foo-end'
end

def f2 &p
  puts 'bar-begin'
  f3 &p
  puts 'bar-end'
end

def f3
  puts 'baz-begin'
  yield                       # unwinds to f1
  puts 'baz-end'
end

f1

puts '------------'
# not all call-sites need to pass block thru
# proc.call also works, not only yield

def h1
  puts 'foo-begin'
  h2 { puts 'break'; break }  # break jumps after this statement
  puts 'foo-end'
end

def h2 &p
  puts 'bar-begin'
  $p = p
  h3
  puts 'bar-end'
end

def h3
  puts 'baz-begin'
  $p[]                        # unwinds to h1
  puts 'baz-end'
end

h1

puts '------------'
# owner of proc != owner of block
# 

def defp &p
  $p = p
  g2 &p                               # this is ok
end  

def g1
  puts 'foo-begin'
  #$p = lambda do                     # the same error result if the following line is replaced by lambda call
  defp do
	begin
		puts 'break'                  # break from proc-closure error
		break
	rescue LocalJumpError
		puts 'ERROR 1'
	end	
  end
  puts 'foo-mid'
  
  g2 &$p                              # this raises an error #3            
  puts 'foo-end'
rescue LocalJumpError
	puts 'ERROR 4'
end

def g2 &p
	puts 'bar-begin'
	g3 &p
	puts 'bar-end'
rescue LocalJumpError
	puts 'ERROR 2'
end

def g3 &p
	puts 'baz-begin'
	1.times &p                      # break returns here, we need to decide now whether we can unwind or not
	puts 'baz-end'
rescue LocalJumpError => e
	puts 'ERROR 3'                  # rescued here
	puts e
	puts e.backtrace
end

g1

