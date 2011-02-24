p e = Encoding.find("SJIS")
p f = Encoding.find("UTF-8")

puts '- ascii only -'

se = "a"
sf = "a"
sb = "a"

p se.force_encoding(e)
p sf.force_encoding(f)

p se == sb, sf == sb, se == sf
p se.hash == sb.hash, sf.hash == sb.hash, se.hash == sf.hash

puts '- non-ascii -'

se = "\xce"
sf = "\xce"
sb = "\xce"

p se.force_encoding(e)
p sf.force_encoding(f)

p se == sb, sf == sb, se == sf
p se.hash == sb.hash, sf.hash == sb.hash, se.hash == sf.hash
