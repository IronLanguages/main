class Proc
  def initialize
    puts 'initialized'
  end 
end

def foo &p
  puts p  
end

Proc.new {}   # prints "initialized"
foo { }       # prints nothing
lambda {}     # prints nothing

