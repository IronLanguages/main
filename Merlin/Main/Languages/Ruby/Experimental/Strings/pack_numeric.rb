d = 'l_'

puts '64'
p [-2**64].pack(d) rescue p $!
p [-2**64+1].pack(d) rescue p $!
p [2**64].pack(d) rescue p $!
p [2**64-1].pack(d) rescue p $!

puts '32'
p [-2**32].pack(d) rescue p $!
p [-2**32+1].pack(d) rescue p $!
p [2**32].pack(d) rescue p $!
p [2**32-1].pack(d) rescue p $!

puts '16'
p [-2**16].pack(d) rescue p $!
p [-2**16+1].pack(d) rescue p $!
p [2**16].pack(d) rescue p $!
p [2**16-1].pack(d) rescue p $!

puts '8'
p [-2**8].pack(d) rescue p $!
p [-2**8+1].pack(d) rescue p $!
p [2**8].pack(d) rescue p $!
p [2**8-1].pack(d) rescue p $!

puts '-' * 10

D = ['F', 'f', 'D', 'd', 'G', 'g', 'E', 'e']
I = ['Q', 'q', 'V', 'v', 'N', 'n', 'S', 's']

(I + D).each {|d|
  puts "#{d}: #{(x = [-2.0].pack(d)).inspect} #{x.unpack(d).inspect}"
}