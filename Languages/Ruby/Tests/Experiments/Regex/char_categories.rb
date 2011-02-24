
rstr = ARGV[0] || "\\p{Linear_B}"
encoding = Encoding::UTF_8

puts rstr
r = /#{rstr.encode('utf-8')}/u

if defined? IRONRUBY
  N = 0x10000
else
  N = 0x110000
end


first = last = -1
ranges = []

N.times do |codepoint|
  if codepoint < 0xd800 or codepoint > 0xdfff
    s = codepoint.chr(encoding)
    if (r =~ s) == 0
      if first == -1
        first = last = codepoint 
      elsif codepoint == last + 1
        last = codepoint
      else
        if first == last
          ranges << first
        else
          ranges << (first..last)
        end
        
        first = last = codepoint        
      end
    end
  end
end

if first == last
  ranges << first
else
  ranges << (first..last)
end

ranges.each do |range|
  if range.kind_of? Range
    puts "#{range.begin.to_s(16)}..#{range.end.to_s(16)}"
  else
    puts range.to_s(16)
  end
end