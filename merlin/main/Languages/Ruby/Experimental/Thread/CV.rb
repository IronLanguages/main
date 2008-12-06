require 'thread' unless defined? IRONRUBY_VERSION

mutex = Mutex.new
resource = ConditionVariable.new

i = 0

a = Thread.new {
  50.times do
    mutex.synchronize {
      resource.wait(mutex) while i == 0
      puts "C: #{i}"
      i -= 1
    }
    
    sleep(0.15)      
  end
}

b = Thread.new {
  50.times do
    mutex.synchronize {
      i += 1
      puts "P: #{i}"
      resource.broadcast  
    }
    
    sleep(0.1)
  end
}

a.join
b.join