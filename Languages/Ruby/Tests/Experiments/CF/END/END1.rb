END {
  puts 'end1'
}

puts 'xxx'

class C
  def foo
    puts 'C::foo'
  end
end

class D < C
  def foo
    x = 1
    $end = lambda {
      return 123
    }
    
    END {
      puts "end-in-method-1: #{x}"
      super rescue p $!
      puts "end-in-method-2"
      END {
        puts 'at-exit-nested-Y'
      }
      puts "end-in-method-3"
      return 123 rescue p $!
      puts "end-in-method-4"
    }
  end
end

D.new.foo

p $end[]