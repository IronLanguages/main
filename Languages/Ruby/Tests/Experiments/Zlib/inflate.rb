require 'zlib'

c = "x\x01\x01\x18\x00\xE7\xFFabcdefghijkabcdefghijklmv\xD7\t\x9E"

d = Zlib::Inflate.new

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << (p c[0...5])

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << (p c[5...10])

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << (p c[10..-1])

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << (p nil)

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << (p nil)

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << (p "-stuff")

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << ("1" * 1000)

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << ("2" * 100)

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << ("3" * 100)

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

p d.finish



