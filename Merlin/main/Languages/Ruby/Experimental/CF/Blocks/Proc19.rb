def foo &q
  p = lambda {
    puts 'P'
  }
  
  q[1, &p]
  q.(1){ puts 'Q' }
  
  # syntax error: yield(1,&p)
  
  q.yield(1,&p)
  q.yield(1) { puts 'R' }  
end

foo { |a,&q;x,y|
  p a
  yield rescue puts $! # no block given
  q[]
  q.()
  q.yield
}

puts '---'

z = ->(a, b = 5) { puts a + b }
5.times &z
