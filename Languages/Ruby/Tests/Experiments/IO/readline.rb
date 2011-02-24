require 'stringio'

s = "aa\n\n\n\nbbbb\n\n\ncccc\n"

File.open("a.txt", "r+") do |f|
  f.write(s)
  
  puts '-- File.readlines("") --'
  f.seek(0)
  p f.readlines("")
  puts
  
  puts '-- File.each_line("") --'
  f.seek(0)
  f.each_line("") { |l| p l }
  puts
end

x = StringIO.new(s)

puts '-- StringIO.readlines("") --'
x.seek(0)
p x.readlines("")
puts

puts '-- StringIO.each_line("") --'
x.seek(0)
x.each_line("") { |l| p l }
puts

puts '-- String.each_line("") --'
s.each_line("") { |l| p l }
puts