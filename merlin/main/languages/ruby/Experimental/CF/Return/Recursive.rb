$r = lambda {
  begin
    puts 'returning R'; 
    return 'r'
  rescue 
    puts 'ERROR 1'
  end  
}

def f0
  puts 'f0.begin'
  
  $q = lambda {
      1.times {
        puts 'returning Q'; 
        return 'q' 
	  }
  }
  
  f1
ensure
  puts 'f0.finally'
end

def f1
  puts 'f1.begin'
  $p = lambda { 
      puts 'returning P'; 
      return 'p'
  }
  
  puts "result: #{f2}"
  puts 'f1.end'
ensure
  puts 'f1.finally'
end

def f2
  puts 'f2.begin'
  f3
  puts 'f2.end'
ensure
  puts 'f2.finally'
  1.times {  
    $q[]                           # returns to $q's definition (! note difference from $p[])
    puts 'Unreachable'
  }
end

def f3
  puts 'f3.begin'
  1.times {
    2.times {
      3.times {
        puts "result: #{$p[]}"     # returns right here, different from y &$p (!)
        puts "result: #{$r[]}"     # returns right here
        y &$r                      # ERROR 1
        y &$p                      # unwinds stack
      }
    }
  }
  puts 'f3.end'
ensure
  puts 'f3.finally'
end

def y 
  puts 'yielding P'
  yield                            # returns to $p's definition
end

f0