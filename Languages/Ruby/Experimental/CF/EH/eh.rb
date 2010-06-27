def dumpex e
    # this will modify the class name as well in MRI (bug)
    #if e.message.length > 0 then
    #    e.message[0] = 'X'                    
    #end    
    
    puts "inspect: #{e.inspect.inspect}"
    puts "class: #{e.class.inspect}"
    puts "message: #{e.message.inspect}"
    puts "to_s: #{e.to_s.inspect}"
    puts
end

def test0
    puts '> IOError.new("foo")'
    dumpex(IOError.new("foo"))
    
    puts '> IOError.new'
    dumpex(IOError.new)
    
    puts '> IOError.new(nil)'
    dumpex(IOError.new(nil))
    
    puts '> IOError.new("")'
    dumpex(IOError.new(""))
    
    puts '> Exception.new'
    dumpex(Exception.new)
    puts
end

def test1
    $! = IOError.new "1"                    
    test1_helper
    puts "E: #{$!.inspect}"                                                         # 7
end

def test1_helper
    begin
        $! = IOError.new "2"                
        begin                                   # t1 = 2 (have rescue)
            $! = IOError.new "3"            
            begin
                begin
                    raise IOError.new("4")      
                ensure                          # t2 = 4                    
                    puts "A: #{$!.inspect}"                                     # 4
                    $! = IOError.new "8"        
                end                             # $! = t2
            ensure                              # t3 = $!
                puts "G: #{$!.inspect}"                                         # 4
                $! = IOError.new "10"            
            end                                 # $! = t3
        rescue                                  # $! = 4
            puts "B: #{$!.inspect}"                                             # 4
            $! = IOError.new "5"            
        ensure                                  # $! = t4 = 2
            puts "C: #{$!.inspect}"                                             # 2
            $! = IOError.new "6"            
        end                                     # $! = t4
        puts "F: #{$!.inspect}"                                                 # 2
        $! = IOError.new "8"            
    ensure                                      # t5 = 8
        puts "D: #{$!.inspect}"                                                 # 8
        $! = IOError.new "7"                
        return
    end                                         # $! = t5 (skipped by return)    
end

def test2
    $! = IOError.new "1"                    
    begin
        $! = IOError.new "2"                
        begin
            $! = IOError.new "3"            
            puts $!.inspect
        end
        puts $!.inspect
   end
   puts $!.inspect
end

def test3
    puts $!
    $! = IOError.new "1"                    
    begin
        puts $!
        $! = IOError.new "2"                
        begin
            puts $!
            $! = IOError.new "3"            
            puts $!
        ensure
            puts $!
        end
        puts $!
   ensure
        puts $!
   end
   puts $!
end

def test4
    puts "A:#{$!.inspect}"
    $! = IOError.new "1"                    
    begin
        puts "B:#{$!.inspect}"
        $! = IOError.new "2"                
        begin
            puts "C:#{$!.inspect}"
            $! = IOError.new "3"            
            puts "D:#{$!.inspect}"
        rescue
        ensure
            puts "E:#{$!.inspect}"
        end
        puts "F:#{$!.inspect}"
   rescue
        puts "G:#{$!.inspect}"
   end
   puts "H:#{$!.inspect}"
end

def test5
    g
    puts "H:#{$!.inspect}"
end

def g
    f { return }
end

def f
    $! = IOError.new "1"
    puts "A:#{$!.inspect}"
    while (true) do
        begin
            puts "C:#{$!.inspect}"
            $! = IOError.new "3"
            puts "D:#{$!.inspect}"
        rescue
        ensure
            puts "G:#{$!.inspect}"
            yield
        end
    end    
    puts "H:#{$!.inspect}"
end

def test6
    r = true
    $! = IOError.new "1"
    puts "A:#{$!.inspect}"
    begin
        puts "C:#{$!.inspect}"
        $! = IOError.new "3"
        puts "D:#{$!.inspect}"
        raise
    rescue
        puts "E:#{$!.inspect}"
        if r then
            r = false
            puts 'retry'
            retry
        end    
    else
        puts "ELSE:#{$!.inspect}"
    ensure
        puts "G:#{$!.inspect}"
    end
    puts "H:#{$!.inspect}"
end

def test7
    begin
        begin
            raise IOError.new("1")
        ensure
            puts $!.inspect
        end
    rescue
        puts $!.inspect
    end
end

def test8
    $! = IOError.new "1"
    begin
        puts $!.inspect
        $! = IOError.new "2"
        puts $!.inspect
    else
        puts "X", $!.inspect
    end
    puts $!.inspect
end

def test9
    $! = IOError.new "1"
    begin
        puts $!.inspect                #1
        $! = IOError.new "2"       
        puts $!.inspect                #2
    rescue
    else
        puts $!.inspect                #1
    end 
    puts $!.inspect                    #1
end

def test10
    $! = IOError.new "1"
    begin
    rescue
    else
        puts $!.inspect
    end
    puts $!.inspect
end

def test11
    $! = IOError.new "1"
    begin
    rescue
    ensure
        puts $!.inspect
    end
    puts $!.inspect
end

def test12
    $! = IOError.new "1"
    begin
    rescue
    else
        puts $!.inspect
    ensure
        puts $!.inspect
    end
    puts $!.inspect
end

def test13
    test13_helper
rescue
end

def test13_helper
    begin
        $! = IOError.new "1"
        begin
            puts "A: #{$!.inspect}"     
            raise IOError.new("4")      
        ensure                          
            puts "B: #{$!.inspect}"     
            $! = IOError.new "8"        
        end
    ensure
        puts "C: #{$!.inspect}"     
    end    
end

def test14
    $! = IOError.new "1"
    test14_helper
end

def test14_helper
    begin
        puts "A: #{$!.inspect}"     
        raise IOError.new("4")      
    ensure                          
        puts "B: #{$!.inspect}"     
        return                      # exception swallowed
    end
end

def test15
    $! = IOError.new "1"
    begin
        begin
            puts "A: #{$!.inspect}"     
            raise IOError.new("4")      
        ensure                          
            puts "B: #{$!.inspect}"     
            $! = IOError.new("5")
        end
    rescue
        puts "C: #{$!.inspect}"     
    end    
end

def test16
    $! = IOError.new "1"
    test16_helper
    puts "D: #{$!.inspect}"     
end

def test16_helper
    begin
        puts "A: #{$!.inspect}"     
    rescue
    else
        puts "B: #{$!.inspect}"     
        $! = IOError.new "2"
        raise IOError.new("3")
    ensure                          
        puts "C: #{$!.inspect}"     
        $! = IOError.new("5")
        return
    end
end

def test17
    $! = IOError.new "1"
    test17_helper
    puts "D: #{$!.inspect}"     
end

def test17_helper
    begin
        puts "A: #{$!.inspect}"     
    else
        puts "B: #{$!.inspect}"     
        $! = IOError.new "2"
        raise IOError.new("3")
    ensure                          
        puts "C: #{$!.inspect}"     
        $! = IOError.new("5")
        return
    end
end

def test18
	begin
		test18_helper
	rescue
		puts "C: #{$!.inspect}"     
	end
end

def test18_helper
	begin
        raise IOError.new("1")
    rescue (raise IOError.new("2"))
		puts "A: #{$!.inspect}"     
        raise IOError.new("3")
    end
    puts "B: #{$!.inspect}"     
end

def test19
	$! = IOError.new "1"
    begin
    else
        $! = IOError.new "2"
    ensure
		$! = IOError.new "3"
    end
    puts $!.inspect   #2
end


$! = nil
puts 'test0'
test0

$! = nil
puts 'test1'
test1

$! = nil
puts 'test2'
test2

$! = nil
puts 'test3'
test3

$! = nil
puts 'test4'
test4

$! = nil
puts 'test5'
test5

$! = nil
puts 'test6'
test6

$! = nil
puts 'test7'
test7

$! = nil
puts 'test8'
test8

$! = nil
puts 'test9'
test9

$! = nil
puts 'test10'
test10

$! = nil
puts 'test11'
test11

$! = nil
puts 'test12'
test12

$! = nil
puts 'test13'
test13

$! = nil
puts 'test14'
test14

$! = nil
puts 'test15'
test15

$! = nil
puts 'test16'
test16

$! = nil
puts 'test17'
test17

$! = nil
puts 'test18'
test18

$! = nil
puts 'test19'
test19
