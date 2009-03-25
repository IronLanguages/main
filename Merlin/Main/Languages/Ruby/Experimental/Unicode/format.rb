# encoding: UTF-8

a = "f%x%c"
u = "f%x%c"

p af = (a % [1,0x12345])
p uf = (u % [1,0xe300000])


p af.encoding rescue 0
p uf.encoding rescue 0


p "%c%c%c%c" % [0,0xff,0x100,0x200, 0x10000000000000000000000]
p "%c%c%c" % [-0x100,-0x101, -0xff] rescue p $!
p (-0xff) % 256