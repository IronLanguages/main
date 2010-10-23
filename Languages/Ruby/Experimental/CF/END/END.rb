END {
  puts 'end-A'
}

require 'END1'

END {
  puts 'end-B-1'
  raise 'exception from end B'
  puts 'end-B-2'
}

def goo
  at_exit {
    puts 'at-exit-1'
    END {
      puts 'at-exit-nested-X'
    }
    puts 'at-exit-2'
    return rescue p $!
    puts 'at-exit-3'
  }
end

goo


puts 'xxx'

p begin
  END {
    puts 'end-C-1'
    return 123 rescue p $!
    puts 'end-C-2'  
  }
end




=begin
def syntax_valid?(code)
  eval("BEGIN {return true}\n#{code}")
rescue Exception
  false
end

puts syntax_valid?("puts 'hi'")
=end
