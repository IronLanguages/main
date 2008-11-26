class Form
  def initialize
    @click = Event.new
  end  
  
  def click
    puts "TRACE: Click"
    @click
  end

  def click=(x)
    puts "TRACE: Click=#{x.inspect}"
    if x.instance_of? MethodHolder then
		@click.add(x.method)
	else
		@click.set(x)
	end
  end
end

class MethodHolder
  attr :method
  
  def initialize(m)
    @method = m
  end
end

class Event
  def initialize
    @methods = []
  end

  def +(m)
    puts "TRACE: +(#{m.inspect}"
    MethodHolder.new(m)
  end 
  
  def call
    @methods.each { |m| m.call }    
  end
  
  def add(m)
    @methods << m
  end
  
  def <<(m)
    @methods << m
  end
  
  def set(m)
    @methods = [m]
  end
  
  def delete(m)
    @methods.delete m
  end
  
  alias :add,:push
  alias :remove,:delete
end

def bar
  puts 'hello'
end

def baz
  puts 'world'
end

form = Form.new
form.click += method(:bar)
form.click += method(:baz)
form.click.call
