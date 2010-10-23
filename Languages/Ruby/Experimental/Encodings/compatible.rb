class C
  def respond_to? name
    puts name
    false
  end
  
  def to_str
    puts 'tostr'
    'xx'
  end
  
  def to_s
    puts 'tostr'
    'xx'
  end
end

p Encoding.compatible?("UTF-8", "US_ASCII")
p Encoding.compatible?(Encoding::US_ASCII, C.new)
p Encoding.compatible?(Encoding::ASCII_8BIT, "UTF-8")

puts '---'
e = ["SJIS", "UTF-8", "ASCII", "BINARY", "iso-8859-1"].map { |n| Encoding.find(n) }
p e
puts '---'
#e.each { |a| e.each { |b| puts "#{a}\t#{b}:\t#{Encoding.compatible?(a, b)}" } } 

def comp a,b
  puts "#{a.encoding} #{b.kind_of?(String) ? b.encoding : b} -> #{Encoding.compatible?(a,b)}"
end

comp "a", "b"
comp "a".force_encoding("UTF-8"), "a\x81"
comp "a\x81", "b"
comp "a\x81", "b\x80"
comp "a\u0123", "b"
comp "a\u0123", "b\x80"
comp "a\u0123", "b\u0123"

puts '---'
U = Encoding.find("UTF-8")

comp "a", U
comp "a\x81", U
comp "a\u0123", U

puts '---'
p ("x".force_encoding("SJIS") + "a\u0123").encoding
comp "x".force_encoding("SJIS"), "a\u0123"