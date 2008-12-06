# brackets
puts %r(a*)
puts %r<a*>
puts %r{a*}
puts %r[a*]

puts "# arbitrary separators:"

puts
(0..255).each do |x|
  print "0x#{sprintf('%X', x)}('%r#{x.chr}regex#{x.chr}'): "
  begin
    print eval("%r#{x.chr}regex#{x.chr}")
  rescue SyntaxError:
    print "error"
  end
  puts
end

puts "# options (note that comments and delimiters are included):"

puts
(0..255).each do |x|
  print "0x#{sprintf('%X', x)}('/regex/#{x.chr}'): "
  begin
    print eval("/regex/#{x.chr}")
  rescue SyntaxError:
    print "error"
  end
  puts
end