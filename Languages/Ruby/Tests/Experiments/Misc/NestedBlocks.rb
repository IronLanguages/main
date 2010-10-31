def outer
  puts 'outer'
  yield
end

def inner
  puts 'inner'
  yield
end

outer { inner { puts 'here' } }
