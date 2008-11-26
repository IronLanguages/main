#require 'y.rb'

puts '1'
puts '2'
puts '3'

class C
  def initialize
    puts caller(0)
  
    puts '===='

    raise
  end
end

def foo
  send :bar
end

def bar
  goo
end

def goo
  1.times {
    C.new
  }
end

def catcher
  foo
rescue => e
  puts $@
  puts '---'
  raise e
end

def catcher_caller
  catcher
end

begin
  catcher_caller
rescue
  puts $@
  puts '---'
end




	