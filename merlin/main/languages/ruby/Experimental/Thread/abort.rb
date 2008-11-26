t = Thread.start { 
  puts 'start'
  while true 
    puts 'tick'
    sleep(0.6)
  end
}
sleep(0.3)
p t

p t.kill
p t.kill
p t.kill
p t.kill
p t.kill
p t.kill

