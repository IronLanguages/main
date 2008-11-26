x = /\A\/(?i-:(say|rails_info|rails\/\/info))(?:\/?\Z|\/([^\/.?]+)(?:\/?\Z|\/([^\/.?]+)\/?))\Z/
 
 
puts "x.inspect"
puts x.inspect

puts "x.to_s"
puts x.to_s

puts "escape(x.to_s)"
puts Regexp.escape(x.to_s)

puts "escape(x.inspect)"
puts Regexp.escape(x.inspect)

puts "new(x)"
puts Regexp.new(x)

puts "new(x.to_s)"
puts Regexp.new(x.to_s)

puts "new(x.inspect)"
puts Regexp.new(x.inspect)
 
class Regexp
  alias :ts :to_s
  alias :i :initialize
  
  def to_s
    x = ts
    puts "to_s #{ts}"
    x
  end
  
  def initialize *s
    puts "init: #{s.inspect}"
    i *s
  end
end

s = "~!@$# %^&*()_+1-2=[x]{1};':\|\\ \\a\\b\\c\",./<>?"

puts "- 1 ------"

z = Regexp.new(s)

puts "- 2 ------"

puts z

puts "- 3 ------"

puts Regexp.escape(s)

puts "- 4 ------"

class R < Regexp
end

u = R.union("/", "*", "/", /./, /\//)

puts "- 5 ------"
puts u
puts u.class

puts "- 6 ------"
u = Regexp.new('/\/[/]')
puts u.to_s

puts "- 7 ------"
u = Regexp.new('/\/[/]')
puts u.inspect

