$M = {}

class Class
  def def_method(name, &p) 
    $M[name] = lambda &p
  end
end


class C
  define_method(:foo) {
	puts 'breaking foo'
	break 'foo'
  }

  define_method(:bar, &lambda{
	puts 'breaking bar'
	break 'bar'  
  })
    
  define_method(:f1) {
    puts 'redoing f1'
    if $r > 0
      $r -= 1
      redo
    end  
  }
  
  define_method(:g1) {
    puts 'retrying g1'
    if $r > 0
      $r -= 1
      retry
    end  
  }
  
  def_method(:xfoo) {
	puts 'breaking xfoo'
	break 'xfoo'
  }

  def_method(:xbar, &lambda{
	puts 'breaking xbar'
	break 'xbar'  
  })
  
  def_method(:xf1) {
    puts 'redoing xf1'
    if $r > 0
      $r -= 1
      redo
    end  
  }
  
  def_method(:xg1) {
    puts 'retrying xg1'
    if $r > 0
      $r -= 1
      retry
    end  
  }
  
  def call_xfoo
    $M[:xfoo][]
  end
  
  def call_xbar
    $M[:xbar][]
  end
  
  def call_xf1
    $M[:xf1][]
  end
  
  def call_xg1
    $M[:xg1][]
  end  
end

puts '-- real --'

puts C.new.foo
puts C.new.bar

$r = 1;
puts C.new.f1

$r = 1;
puts C.new.g1 rescue puts $! # retry from proc-closure

puts '-- emulated --'

puts C.new.call_xfoo
puts C.new.call_xbar

$r = 1;
puts C.new.call_xf1

$r = 1;
puts C.new.call_xg1 rescue puts $! # retry from proc-closure

puts '-- hash --'

h = Hash.new {
	puts 'breaking 2'
	break '2'
  }
  
begin
  puts h[1]
rescue
  puts $!
end  

h = Hash.new(&lambda {
	puts 'breaking 3'
	break '3'
  })
  
puts h[1]