str = " "*255
255.times { |x| str[x] = x }

255.times { |x| 
  s = str.lstrip
  if s != str
    puts x
  end
  str = str[1..-1]
}

puts '---'

str = " "*255
255.times { |x| str[x] = x }

255.times { |x| 
  s = str.rstrip
  if s != str
    puts 254-x
  end
  str = str[0..-2]
}