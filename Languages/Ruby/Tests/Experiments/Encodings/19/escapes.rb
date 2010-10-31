p __ENCODING__
p "abc".encoding
p "\u0123".encoding
#p "\u0123".bytes.to_a.map { |x| x.to_s(16) }
p "\xc4\xa4".encoding
p "\x81".encoding
puts '-- ? --'
p ?a.encoding
p ?\M-a.encoding
p ?\b.encoding
p ?\x81.encoding
p ?\u0020.encoding
p ?\u1234.encoding
p ?\u{12345}.encoding
puts '-- // --'
p /a/.encoding
p /\x81/.encoding
p /\u1234/.encoding
p /\u{12345}/.encoding
puts '-- //x --'
p /a/s.encoding
p /a/u.encoding
p /a/e.encoding
p ?-
r = /\x20/u
p r, r.encoding, r.source
p :"x\0x"                        # zero in symbol (1.8 used to report an error)
p "\uFFFF"                       # invalid Unicode char