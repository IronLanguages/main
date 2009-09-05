require 'fileutils'

DLRJS = File.dirname(__FILE__) + "/dlr.js"
if File.exist? DLRJS
  FileUtils.rm DLRJS 
  puts "Deleted #{DLRJS}"
end

dlrjs = ""
%W(Silverlight mss).each do |file|
  file = File.dirname(__FILE__) + "/#{file}.js"
  next if File.directory?(file)
  dlrjs << "#{'//' * 40}\n// Start of #{file}\n#{'//' * 40}\n\n"
  data  = File.open(file, 'r'){|f| f.read}
  dlrjs << "#{data}\n\n#{'//' * 40}\n// End of #{file}\n#{'//' * 40}\n\n"
  puts "Wrote #{file} into #{DLRJS}"
end
dlrjs.gsub!(/﻿/, '')
File.open(DLRJS, 'w'){|f| f.write dlrjs}
puts "Saved #{DLRJS}"

