class Exception
  remove_method "backtrace"
  #remove_method "backtrace="  # bug terminates process ???
end

module M
  def backtrace
    puts 'getter'
    ['foo', 'bar', @x]
  end

  def backtrace=(value)
    puts 'setter'
    p value
  end
end

class C < Exception
  include M
  
  def initialize(x)
    @x = x
  end
end

puts 'Hello'

$! = C.new "0"
p $@                   # [foo, bar, 0]
$! = C.new "X"

alias $! $foo
$foo = C.new "1"
$! = C.new "2"
p $@                   # [foo, bar, X] ignores alias

puts "-"*50

#$@ = 123               # ??? bug: segfault?
$@ = ['f','g']          # ??? bug: doesn't call setter 
p $@

puts 'Done'

