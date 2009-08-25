# Suppose we don't like having 5 levels named DEBUG, INFO, etc.
# Suppose we'd rather use 3 levels named Foo, Bar, and Baz.
# Log4r allows you to rename the levels and their corresponding methods
# in a painless way. This file provides and example

$: << '../src'

require 'log4r'
require 'log4r/configurator'
include Log4r

# This is how we specify our levels
Configurator.custom_levels "Foo", "Bar", "Baz"

l = Logger.new('custom levels')
l.add StdoutOutputter.new('console')

l.level = Foo
puts l.foo?
l.foo "This is foo"
puts l.bar?
l.bar "this is bar"
puts l.baz?
l.baz "this is baz"

puts "Now change to Baz"

l.level = Baz
puts l.foo?
l.foo {"This is foo"}
puts l.bar?
l.bar {"this is bar"}
puts l.baz?
l.baz {"this is baz"}
