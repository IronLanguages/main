# setup ##############################

$i = 0

def defp &p
  $p = p
end

defp {}

def f *,&p
	if $i == 0
		puts 'retrying'
		$i = 1
		retry
	end
end

#######################################

def g1 *a
	puts 'G'
    f puts('F'),&$p             
end

def g2 *a
	puts 'G'
	raise
rescue
    f puts('F'),&$p             # retries the try-block, unless this method doesn't get block $p
end

def g3 *a
	puts 'G'
    1.times do
		puts 'T'
		f puts('F'),&$p         # retries the block, unless this method doesn't get block $p
	end
end

$outer_lambda = lambda do
	puts 'T'
	f puts('F'),&$p             
end

def g4 *a
	puts 'G'
    1.times &$outer_lambda      # retries the f call in $outer_lambda; we are not in the function g4 here, because the block has been created outside it
end

def g5 *a
	$local_lambda = lambda do
		puts 'T'
		f puts('F'),&$p         
	end
		
	puts 'G'
    1.times &$local_lambda      # retries the block; we are in the scope of g5
end

def h
   puts '- 1 -'
   $i = 0
   g1 puts('g')                 # the function f call in g1 is retried
   
   puts '- 2 -'
   $i = 0
   g1 puts('g'), &$p            # this call is retried

   puts '- 3:rescue -'
   $i = 0
   g2 puts('g')                 # the function f call in g2 is retried

   puts '- 4:rescue -'
   $i = 0
   g2 puts('g'), &$p            # the try-block in g1 is retried (!)
   
   puts '- 5:block -'
   $i = 0
   g3 puts('g')                 # the function f call in g3 is retried

   puts '- 6:block -'
   $i = 0
   g3 puts('g'), &$p            # the 1.times call in g3 is retried (!)
   
   puts '- 7:out-lambda -'
   $i = 0
   g4 puts('g')                 # the function f call in $l is retried

   puts '- 8:out-lambda -'
   $i = 0
   g4 puts('g'), &$p            # the function f call in $l is retried
   
   puts '- 9:in-lambda -'
   $i = 0
   g5 puts('g')                 # the function f call in $l is retried

   puts '- A:in-lambda -'
   $i = 0
   g5 puts('g'), &$p            # the function f call in $l is retried
end

h



  