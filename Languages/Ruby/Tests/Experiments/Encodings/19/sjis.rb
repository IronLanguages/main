#encoding: SJIS
p s = "abc", s.encoding
p s = "abc\x20", s.encoding
p s = "abc\x81", s.encoding
p s = "abc\xff", s.encoding
puts '---'
p s = "abc\u00ff", s.encoding                         # UTF8 encoding eventhough the escape is < 0x80
p s = "abc\u1234", s.encoding
puts '-escapes-'
p s = ?a, s.encoding
p s = ?\x20, s.encoding
p s = ?\x81, s.encoding
p s = ?\xff, s.encoding
p s = ?\u00ff, s.encoding
p s = ?\u{12345}, s.encoding
p s = ?あ, s.encoding
puts '-syms-'
p s = :a, s.encoding
p s = :@a, s.encoding
p s = :@@a, s.encoding
p s = :$a, s.encoding
p s = :あ, s.encoding
p s = :@あ, s.encoding
p s = :@@あ, s.encoding
p s = :$あ, s.encoding
p s = :"\x20", s.encoding

eval('
s = :"\xff"
p s.encoding
') rescue p $!

p s = :"\u1234", s.encoding


puts '-----'

# error: utf8_mixed_within_sjis_source.rb
begin
  eval('"a\x81\u00ff"')
rescue SyntaxError
  p $!.message
end

begin
  eval('"abc\u1234あ"')
rescue SyntaxError
  p $!.message
end

# concat:
begin
  eval('
    puts "hello"
    puts "abc\u1234" "あ"
 ')
rescue Exception
  p $!.message
end

