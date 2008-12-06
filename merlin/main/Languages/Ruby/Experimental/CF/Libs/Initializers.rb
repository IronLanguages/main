class A
  def initialize *a
    puts "initialize: #{self.object_id}, #{a.inspect}"
    yield
    100
  end

  def init *a, &p
    initialize *a, &p
  end
end

x = A.new {
  break 'foo'
}

puts x

x = A.new {
  break
}

puts x

x = A.new {
}

puts x

y = x.init {
  break 'bar'
}
puts y

y = x.init {
  break
}
puts y

y = x.init {
}
puts y


$i = 0
y = A.new($i += 1) {
  if $i < 5 then
    retry
  end
}