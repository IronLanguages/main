class C
  def to_str
    puts 'to_str'
    'xxx'
  end
end

class Regexp
  def =~(*args)
    puts "=~#{args}"
  end

  def !~(*args)
    puts "=~#{args}"
  end
end

c = C.new

puts '-- match'
/foo/ =~ c

puts '-- not match'
/foo/ !~ c