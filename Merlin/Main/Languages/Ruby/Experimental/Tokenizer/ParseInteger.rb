puts '- MRI bugs -'
p "0b1010".oct
p "0xFF".oct
p "0d500".oct
p "__12___3".to_i
p "__12___3".to_i(0)
p "__12___3".to_i(16)

puts '---'

p "-1234".hex == -4660
p "+1234".hex == 4660

puts '---'
p " 0x2  ".to_i    # default param value is 10
p "".to_i
p " 2  ".to_i
puts '---'
p " 0x2  ".to_i(10)
p " 9  ".oct
p " -20  ".hex
p " -2  ".to_i
puts '---'
p " 0078  ".to_i(0)
p " 200\0100".to_i
puts '---'
p Integer(" 0x2_2_2122222222222222222222222222222222  ")
p Integer(" 2_2__2  ") rescue p $!
p Integer(" 2123123.12323  ") rescue p $!
p Integer(" 2123123 #  ") rescue p $!
p Integer("   ") rescue p $!
p Integer("  - ") rescue p $!
p Integer("  -     1") rescue p $!
p Integer("  -     0x1") rescue p $!
p Integer(" \" -     0x1") rescue p $!
p Integer("") rescue p $!
p Integer("200\0100") rescue p $!
puts '---'
p "0b1010".hex
p "0d500".hex
p "abcdefG".hex
puts '---'
