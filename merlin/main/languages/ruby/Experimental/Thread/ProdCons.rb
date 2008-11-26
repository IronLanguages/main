require 'thread' unless defined? IRONRUBY_VERSION

queue = Queue.new

producer = Thread.new do
  5.times do |i|
    sleep i/2 # simulate expense
    queue << i
    puts "#{i} produced"
  end
end

consumer = Thread.new do
  5.times do |i|
    value = queue.pop
    sleep i/2 # simulate expense
    puts "consumed #{value}"
  end
end

consumer.join