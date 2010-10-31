puts
(0..255).each do |x|
  begin
    eval("%q#{x.chr}a#{x.chr}")
    puts "0x#{sprintf('%X', x)}('%q#{x.chr}a#{x.chr}'): OK"
  rescue SyntaxError:
  end
end

puts
(0..255).each do |x|
  begin
    eval("%#{x.chr}(a)") unless x.chr == 'x' 
    puts "0x#{sprintf('%X', x)}('%#{x.chr}(a)'): OK"
  rescue SyntaxError:
  end
end

puts %q(a)
puts %q<a>
puts %q{a}
puts %q[a]

puts "----"

#nesting:
puts %q(a ((a) b))

puts "----"

#multiline:
puts %q(a (
(a) b
)
)

puts "----"

#multiline:
puts %q-a (
(as
  -

puts "----"

puts "dasdas
a
          das
das dasd asd as
"

puts "----"

#escaping:
puts %q-foo\-bar-
puts %q(foo\(bar)
puts %q(foo\)bar)
