class Regexp
  public :initialize
end

puts '-- new --' + '-' * 25

p Regexp.new() rescue p $!
p Regexp.new(/foo/)
p Regexp.new(/foo/, false) rescue p $!
p Regexp.new(/foo/, false, "") rescue p $!
p Regexp.new("asd", "i", "nu")
p Regexp.new("asd", "mix", "nu")
p Regexp.new("asd", false, "mixnu")
p Regexp.new("asd", false, "umixnu")
p Regexp.new("asdsad", nil, nil, nil) rescue p $!

puts '-- init --' + '-' * 25

p (x = Regexp.new("foo")).initialize() rescue p $!
p x
p (x = Regexp.new("foo")).initialize(/xxx/)
p x
p (x = Regexp.new("foo")).initialize(/xxx/, false) rescue p $!
p x
p (x = Regexp.new("foo")).initialize(/xxx/, false, "") rescue p $!
p x
p (x = Regexp.new("foo")).initialize("xxx", "i", "nu") 
p x
p (x = Regexp.new("foo")).initialize("xxx", "mix", "nu") 
p x
p (x = Regexp.new("foo")).initialize("xxx", false, "mixnu")
p x
p (x = Regexp.new("foo")).initialize("xxx", false, "umixnu")
p x
p (x = Regexp.new("foo")).initialize("xxx", nil, nil, nil) rescue p $!
p x

puts '-- literal --' + '-' * 25

p /bar/.initialize("foo") rescue p $!

puts '-- compile --' + '-' * 25

p Regexp.compile("asdsad")
p Regexp.compile(/asdsad/, false) rescue p $!
p Regexp.compile("asdsad", true)
p Regexp.compile("asdsad", "m")
p Regexp.compile("asdsad", "m", "n")
p Regexp.compile("asdsad", "m", "e")
p Regexp.compile("asdsad", nil, "e")
p Regexp.compile("asdsad", nil, nil)
p Regexp.compile("asdsad", nil, nil, nil) rescue p $!

