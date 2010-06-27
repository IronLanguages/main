require 'src_binary'
require 'src_utf8'
require 'src_sjis'

s = {
  ' b' => get_binary_str,
  ' s' => get_sjis_str,
  ' u' => get_utf8_str,
  'ab' => get_binary_ascii_str,
  'au' => get_utf8_ascii_str,
  'as' => get_sjis_ascii_str,
}

s.each do |k1,v1|
  s.each do |k2,v2| 
    e = (v1 + v2 rescue nil)
    puts "#{v1.encoding.to_s.upcase}#{k1[0]} + #{v2.encoding.to_s.upcase}#{k2[0]} -> #{e.nil? ? 'error' : e.encoding.to_s.upcase} #{e.nil? ? '' : e.size}"
  end
end


