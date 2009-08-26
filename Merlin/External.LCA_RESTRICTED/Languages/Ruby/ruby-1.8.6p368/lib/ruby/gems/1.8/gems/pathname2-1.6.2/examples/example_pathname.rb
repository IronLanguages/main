########################################################################
# example_pathname.rb
#
# Some examples to demonstrate the behavior of the pathname2 library.
########################################################################
require 'pathname2'

puts "VERSION: " + Pathname::VERSION

path1 = Pathname.new("foo/bar")
path2 = Pathname.new("baz/blah")

path3 = Pathname.new("foo/../bar")
path4 = Pathname.new("../baz")

p path1 + path2 # foo/bar/baz/blah
p path3 + path4 # baz

# Shortcut syntax
path = pn{ "C:\\Documents and Settings\\snoopy\\My Documents" }

p path[0]    # C:
p path[1]    # Documents and Settings
p path[0,2]  # C:\\Documents and Settings
p path[0..2] # C:\\Documents and Settings\\snoopy