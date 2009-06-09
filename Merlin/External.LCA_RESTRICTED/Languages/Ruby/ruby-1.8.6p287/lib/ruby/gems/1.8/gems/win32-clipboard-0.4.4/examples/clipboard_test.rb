##########################################################################
# clipboard_test.rb (win32-clipboard)
#
# Generic test script for those without TestUnit installed, or for
# general futzing.
##########################################################################
Dir.chdir('..') if File.basename(Dir.pwd) == 'examples'
$LOAD_PATH.unshift(Dir.pwd)
$LOAD_PATH.unshift(Dir.pwd + '/lib')

require "win32/clipboard"
require "pp"
include Win32

puts "VERSION: " + Clipboard::VERSION

pp Clipboard.formats
pp Clipboard.data(Clipboard::UNICODETEXT)
pp Clipboard.format_available?(49161)
pp Clipboard.format_name(999999999)
pp Clipboard.format_available?(9999999)

puts "Data was: [" + Clipboard.data + "]"

Clipboard.set_data("foobar")

puts "Data is now: [" + Clipboard.data + "]"

puts "Number of available formats: " + Clipboard.num_formats.to_s

Clipboard.empty

puts "Clipboard emptied"