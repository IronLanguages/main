require 'zlib'

@deflator = Zlib::Deflate.new
data = "\000" * 10

p @deflator.method(:<<)

p @deflator.deflate data, Zlib::SYNC_FLUSH
puts 1
p @deflator.deflate("hello", Zlib::SYNC_FLUSH)
puts 2
p @deflator.deflate("", Zlib::SYNC_FLUSH)
puts 3
#p @deflator.deflate(nil, Zlib::SYNC_FLUSH)
puts 4
p @deflator.deflate("hello", Zlib::SYNC_FLUSH)
puts 5
p @deflator.flush
puts 6
p @deflator.finish
puts 7
p @deflator.finish

p @deflator

puts '%%%%%%%%%%%%%%%%%%%%%%%%'


d = Zlib::Deflate.new Zlib::NO_COMPRESSION

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

p d.deflate("abcdefghijk")

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

p d.deflate('abcdefghijklm')

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

p x = d.finish

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

puts '---'

d = Zlib::Deflate.new Zlib::NO_COMPRESSION

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << "abcdefghijk"

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << 'abcdefghijklm'

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

p x = d.finish

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

puts '==='

d = Zlib::Deflate.new Zlib::NO_COMPRESSION
p d.deflate("abcdefghijkabcdefghijklm", Zlib::FINISH)

puts '======'  

d = Zlib::Deflate.new Zlib::NO_COMPRESSION

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

d << "abcdefghijk"

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

p d.deflate('abcdefghijklm', Zlib::SYNC_FLUSH)

puts "ain: #{d.avail_in}, aout: #{d.avail_out}, tin: #{d.total_in}, tout: #{d.total_out}"

puts '=========='  

