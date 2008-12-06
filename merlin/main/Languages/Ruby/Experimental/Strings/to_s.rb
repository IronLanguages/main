
(0..255).each { |o|
  puts o.chr.to_s  
}

puts '---'

(0..255).each { |o|
  puts o.chr.inspect  
}

puts '---'

(0..255).each { |o|
  s = "##{o.chr}"
  if s.inspect[0..2] == '"\\#' then
    puts s.inspect
  end
  
  if s.to_s[0..1] == '\\#' then
    puts s.to_s
  end
}