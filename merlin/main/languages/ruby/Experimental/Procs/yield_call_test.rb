def def_lambda
  $lambda = lambda { |*| return 'ok' }
end
def_lambda


def test_yield
  yield
end

def test_call &p
  p[]
end

puts test_yield(&$lambda) rescue puts "yield: #$!"
puts test_call(&$lambda)



