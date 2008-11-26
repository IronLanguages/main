def visibility
  x = 1
  1.times { |i|
    z = x + i
    1.times { |j|
      w = z + j
    }
    puts w rescue puts $!
  }
  puts z rescue puts $!
end

def init
  5.times { |i|
    if i == 0
      x = 1
    else  
      x = (x.nil? ? 0 : x) + 2
    end
    puts x
  }
end

def closure 
  p = [] 
  2.times { |i|
    p << lambda {
      puts i
    }
  }
  
  p[0][]
  p[1][]  
end

def closure_binding
  p = [] 
  2.times { |i|
    p << binding
  }
  
  eval('puts i', p[0])
  eval('puts i', p[1])
end

def module_scope
  eval('
    $p = [] 
    $i = 0
    while $i < 2    
      module M
        x = $i
        $p << binding
      end 
    $i += 1
    end  
  ')
    
  eval('puts x', $p[0])
  eval('puts x', $p[1])
end

visibility
puts '---'
init
puts '---'
closure
puts '---'
closure_binding
puts '---'
module_scope
puts '---'

puts 'done'