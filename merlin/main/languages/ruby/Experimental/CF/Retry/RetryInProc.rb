=begin

primary-frame(main):
    block: -
    
	call-site: owner
	primary-frame(owner):
		block: -
		
		call-site: Proc.new B1
		primary-frame(Proc.new):
			block: B1

    call-site: foo B1 {}
	primary-frame(foo):
		block: B1
		
		call-site: y B1 {InRescue}
		primary-frame(y):
			block: B1
			
			call-site: yield B1 {}
			block-frame(B1):
				owner: primary-frame(owner), Inactive
				retry

			call-site: B1.call {}
			ERROR
				
=end

$i = 0

def y *a
#  puts 'y.begin'
#  raise
#rescue            # uncomment -> yield retries this try block

  if $yield then
    yield                          
  else
    begin
      $p[]
    rescue
      puts "Error: #{$!}"
    end  
  end 
end

def owner
	$lambda = lambda do         # B1
	  puts 'block.begin'
	  if ($i == 0)
		$i = 1
		puts 'retrying'
		begin
		  retry    
		rescue
		  puts "Unreachable"   
		end
	  end
	  puts 'block.end'
	end
	
	$proc = Proc.new do         # B2
	  puts 'block.begin'
	  if ($i == 0)
		$i = 1
		puts 'retrying'
		retry    
	  end
	  puts 'block.end'
	end
	
    $p = $lambda
    y puts('Y')
    
    $p = $proc
    y puts('Y')
end

def foo *a
    puts 'foo.begin'
    raise
rescue
    y puts('Y'),&$p              # retries try block ($yield) / Error: retry from proc-closure (!$yield), doesn't depend on whether the block is active or not
ensure
    puts 'foo.finally'
end

puts '-' * 50,'Tests []-call to a lambda and proc from with their owner active','-' * 50

$i = 0
$yield = false
owner

puts '-' * 50,'Tests yield to a lambda (with inactive owner)','-' * 50

$i = 0
$p = $lambda
$yield = true
foo puts('F'),&$p

puts '-' * 50,'Tests yield to a proc (with inactive owner)','-' * 50

$i = 0
$p = $proc
$yield = true
foo puts('F'),&$p

puts '-' * 50,'Tests []-call to a lambda with incactive owner','-' * 50

$i = 0
$p = $lambda
$yield = false
foo puts('F'),&$p

puts '-' * 50,'Tests []-call to a proc with incactive owner','-' * 50

$i = 0
$p = $proc
$yield = false
foo puts('F'),&$p

puts '-' * 50,'Tests retry in method w/o block and outside rescue','-' * 50

def bar
  retry
rescue 
  puts "E1: #{$!}"
end

begin
  bar
rescue 
  puts "E2: #{$!}"     # exception caught here
end
