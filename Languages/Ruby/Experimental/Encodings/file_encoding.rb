f = File.open("a.txt")

f.set_encoding(nil,nil)
p f.external_encoding
p f.internal_encoding

f.set_encoding(nil)
p f.external_encoding
p f.internal_encoding

f.set_encoding(nil, Encoding::UTF_8) rescue p $!
p f.external_encoding
p f.internal_encoding

f.set_encoding(Encoding::UTF_8, Encoding::UTF_8)
p f.external_encoding
p f.internal_encoding

f.set_encoding("UTF-8", "SJIS")
p f.external_encoding
p f.internal_encoding

f.set_encoding(Encoding::UTF_8, "SJIS")
p f.external_encoding
p f.internal_encoding

f.set_encoding("UTF-8", Encoding::US_ASCII)
p f.external_encoding
p f.internal_encoding

class C
  def respond_to? name
    p name
    true
  end
  
  def to_hash
    {external: "UTF-8"}
  end
end

puts '--'
f.set_encoding(nil, C.new)
