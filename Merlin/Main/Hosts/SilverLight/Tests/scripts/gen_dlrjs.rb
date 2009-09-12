require 'fileutils'
dlrjspath = File.dirname(__FILE__) + '/../../Scripts/dlr.js'
gendlrjspath = File.dirname(__FILE__) + '/../../Scripts/generate_dlrjs.rb'
FileUtils.rm dlrjspath if File.exist?(dlrjspath)
load gendlrjspath
mydlrjspath = File.dirname(__FILE__) + '/../dlr.js'
FileUtils.rm mydlrjspath if File.exist?(mydlrjspath)
FileUtils.cp dlrjspath, mydlrjspath
puts "Copied #{dlrjspath} to #{mydlrjspath}"
