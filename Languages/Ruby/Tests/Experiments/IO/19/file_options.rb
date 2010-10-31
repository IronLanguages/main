f = File.open("a.txt", "r")
s = f.gets
p [f.external_encoding, f.internal_encoding, s, s.encoding]

f = File.open("a.txt", "r:utf-8:us-ascii")
s = f.gets
p [f.external_encoding, f.internal_encoding, s, s.encoding]

File.open("a.txt", "r:utf-8:us-ascii", encoding: "sjis:us-ascii") rescue p $!
File.open("a.txt", "r:utf-8:us-ascii", mode: "r") rescue p $!
File.open("a.txt", mode: 1) rescue p $!
File.open("a.txt", zzz: 1)
File.open("a.txt", "r:garbage") rescue p $!
File.open("a.txt", encoding: "garbage") rescue p $!
p File.open("a.txt", mode: "r:utf-8")
p File.open("a.txt", mode: "r:utf-8", encoding: "sjis") rescue p $!
p File.open("a.txt", mode: "r", encoding: "utf-8")


=begin
puts '-- reopen --'

f.reopen("a.txt", "r:utf-8:utf-8")
s = f.gets
p [f.external_encoding, f.internal_encoding, s, s.encoding]

puts '-- IO.new --'

class H < Hash
  def [] value
    puts "#{value}"
    "sjis:us-ascii"
  end
end

class C
  def respond_to? name
    p name
    false
  end
end

h = H.new
h["encoding"] = "sjis:us-ascii"
h[:encoding] = Encoding::UTF_8

i = IO.new(f.to_i, h)
s = i.gets
p [i.external_encoding, i.internal_encoding]

f.close

puts '---'


=end