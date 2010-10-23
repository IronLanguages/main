#encoding: SJIS
x = "\x80"
p x, x.encoding, x.valid_encoding?

y = "Ω"
p y, y.encoding, y.valid_encoding?

y = "a"
p y, y.encoding, y.valid_encoding?

y = "\u1234"
p y, y.encoding, y.valid_encoding?

y = /12/
p y, y.encoding

y = /12/s
p y, y.encoding

y = /12/e
p y, y.encoding

y = /\u1234/
p y, y.encoding
