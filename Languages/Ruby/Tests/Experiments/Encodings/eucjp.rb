#
# Windows returns 2 EUC-JP encodings (CP 20932 and CP 51932).
# It seems CP20932 is what Ruby uses for Encoding::EUC_JP 
# (by comparing the codepoint mapping to Unicode).
#

=begin
E1 = Encoding.list.find { |x| x.name == "EUC-JP" }
E2 = Encoding.list.find { |x| x.name == "euc-jp" }

[E1, E2].each do |enc|
  p enc, enc.CodePage
  e = "\xB0\xA1".force_encoding(enc)
  u = e.encode(Encoding::UTF_8)
  p u.encoding
  p u.codepoints.to_a.map { |x| x.to_s(16) }
  puts
end
=end

#enc = Encoding::EUC_JP

enc = Encoding.list.find { |x| x.name == "EUC-JP" }

p enc  #, enc.CodePage
(0x00..0xff).each { |a|
  (0x00..0xff).each { |b|
    s = "  "
    s.force_encoding('binary')    
    s.setbyte(0, a)
    s.setbyte(1, b)
    
    begin
      s.force_encoding(enc)
      u = s.encode(Encoding::UTF_8)
      cps = u.codepoints.to_a
      if cps.size == 1
        print "#{a.to_s(16)} #{b.to_s(16)}: "
        puts "#{cps.map { |x| x.to_s(16) } }"
      end
    rescue
      #print "#{a.to_s(16)} #{b.to_s(16)}: "
      #puts 'error'
    end
  }
}