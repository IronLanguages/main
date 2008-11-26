=begin
    Comprehensive test of break, next, redo and return statements semantics.
    Doesn't test all interactions with exception handling though.
=end

def make_block name
	%{
		puts '#{name}.begin'
		begin
			if $statement == :next then
				puts $statement
				$state = true
				next "#{name}"
			elsif $statement == :break then
				puts $statement
				$state = true
				break "#{name}"
			elsif $statement == :return then
				puts $statement
				$state = true
				return "#{name}"
			else
				if not $state then
					puts $statement
					$state = true
					redo
				end	
			end		
		rescue
		    # why the $! is nil here?
		
	        puts "#{name}.ERROR: #{$!.inspect}" + 
	            ($statement == :return ? " (unexpected return)" : " unreachable")
		end		
	}
end

def run
	puts 'run.begin'
	
	test :break
	test :next
	test :redo
	test :return
	
	puts 'run.end'
end

def test statement
	$statement = statement
	
	puts 'test.begin'
	
	f :call,  :lambda, :owner => :active
	f :yield, :lambda, :owner => :active
	f :call,  :lambda, :owner => :inactive
	f :yield, :lambda, :owner => :inactive

	f :call,  :proc, :owner => :active, :converter => :active
	f :yield, :proc, :owner => :active, :converter => :active
	f :call,  :proc, :owner => :inactive, :converter => :active
	f :yield, :proc, :owner => :inactive, :converter => :active
	f :call,  :proc, :owner => :active, :converter => :inactive
	f :yield, :proc, :owner => :active, :converter => :inactive
	f :call,  :proc, :owner => :inactive, :converter => :inactive
	f :yield, :proc, :owner => :inactive, :converter => :inactive

	puts '-'*55
	puts 'test.end'
ensure
	puts 'test.finally'
end

def f action, kind, frames
    $action = action
    $kind = kind
    $owner = frames[:owner]
    $converter = ($kind == :proc) ? frames[:converter] : :inactive

    puts '-'*55, "#{$statement}: #{$action} #{$kind}, owner:#{$owner}, converter:#{$converter}", '-'*55
    
    puts 'f.begin'
    
    r = f0
    puts "result = '#{r}'"
    
    puts 'f.end' + state_msg
ensure
	puts 'f.finally'
end

def define_procs
	if $kind == :lambda then
        $proc = lambda do |*a|                    # owner inactive
		    puts "args = #{a.inspect}"  
		    a = a + [1]
		    eval make_block('lambda')
	    end
    else	
        $proc = Proc.new do |*a|                  # owner inactive
		    puts "args = #{a.inspect}"
		    a = a + [1]
		    eval make_block('proc')
	    end
	end    
end

def f0
	print 'f0.begin'
	
	if $owner == :active then
	    if $converter == :inactive then
	        puts "    <-- owner"
	        if $kind == :lambda then
	            $proc = lambda do |*a|                         # converter (Kernel#lambda) inactive
		            puts "args = #{a.inspect}"
		            a = a + [1]
		            eval make_block('lambda1')
	            end
            else
	            $proc = Proc.new do |*a|                       # converter (Proc#new) inactive
		            puts "args = #{a.inspect}"
		            a = a + [1]
		            eval make_block('proc1')
	            end
	        end    
	    else
	        $proc = nil                          # to be created later
	    end
    else
        puts
        define_procs
	end
	
	f1
		
	puts 'f0.end' + state_msg
ensure
	puts 'f0.finally'
end

def f1
	puts 'f1.begin'
	f2
  	puts 'f1.end' + state_msg
ensure
	puts 'f1.finally'
end

def f2
	puts 'f2.begin'
	r = f3 &$proc
	puts "result = '#{r}'"
  	puts 'f2.end' + state_msg
ensure
	puts 'f2.finally'
end

def f3 &p
	$state = false
	print 'f3.begin'
	
	if p == nil then      
		puts '    <-- owner'
	    if $kind == :proc then
		    r = f4 true do |*a|                           # f4 is the converter
			    puts "args = #{a.inspect}"
			    a = a + [1]
			    eval make_block('proc2')                  # redo goes here (at the beginning of the block)			
		    end                                           # break unwinds to here           
		else
		    r = f4(false, &lambda do |*a|                 # lambda is the converter => it is inactive!
			    puts "args = #{a.inspect}"
			    a = a + [1]
			    eval make_block('lambda2')                # redo goes here (at the beginning of the block)
		    end)                                          # break does NOT uwind here  
		end    
	else
		puts
	    r = f4 false, &p
	end	
	puts "result = '#{r}'"
	puts 'f3.end' + state_msg
ensure
	puts 'f3.finally'	
end

def f4 is_converter, &p
	puts "f4.begin#{is_converter ? '    <-- converter' : ''}"
	
	b = true
	while b do                                        # loop does NOT interfere with stack unwinding   
		f5 &p
		b = false
	end
	
	puts 'f4.end' + state_msg
ensure
	puts 'f4.finally'	
end

def f5 &p
	puts 'f5.begin'
	invoke &p
	puts 'f5.end' + state_msg
ensure
	puts 'f5.finally'	
end

def invoke &p
	puts 'invoke.begin'
	if $action == :yield then
		puts 'yielding'
		puts "yield-result = '#{yield}'"              # next returns here
	else
		puts 'calling'
		puts "call-result = '#{p[]}'"                 # next returns here
	end  
	puts 'invoke.end' + state_msg
rescue
	$state = false
	puts "invoke.ERROR: #{$!.inspect}"
ensure 
	puts 'invoke.finally'  
end 

def state_msg
	if $state then
		$state = false
		" <---- #{$statement} returned here"	  
	else
		""
	end
end

run

  