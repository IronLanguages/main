# encoding: UTF-8

["a", "b", "c", "d", "e", "é", "á"].each { |x| print x.dump, " " }
puts
["a", "b", "c", "d", "e", "é", "á"].sort.each { |x| print x.dump, " " }
puts

# å == U+00E5 == (U+0061, U+030A)
str = "combining mark: a\u{30a}";
p str.index("\u{e5}")

p "a\u{12345}".index("a\xF0\x92\x8D")
p "a\u{12345}".index("a\xF0\x92\x8D\x85")