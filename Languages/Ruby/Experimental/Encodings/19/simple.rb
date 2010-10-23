puts "__ENCODING__: #{__ENCODING__}"
puts "literal.encoding: #{'hello'.encoding}"
puts "default_external: #{Encoding.default_external}"
puts "default_internal: #{Encoding.default_internal}"

puts '- cmd args -'
p ARGV.each { |a| p a.encoding }
puts '- $0 -'
p $0, $0.encoding

puts '---'
class C
  def respond_to? name
    p name
    super
  end
  
  def to_str
    'x'
  end
end

(Encoding.default_external = C.new) rescue p $!

p Encoding.locale_charmap

p Encoding.find("locale")

(Encoding.default_external = nil) rescue p $!
