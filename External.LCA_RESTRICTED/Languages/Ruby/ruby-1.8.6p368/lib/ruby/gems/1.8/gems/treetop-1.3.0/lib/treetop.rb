require 'rubygems'

dir = File.dirname(__FILE__)

TREETOP_ROOT = File.join(dir, 'treetop')
require File.join(TREETOP_ROOT, "ruby_extensions")
require File.join(TREETOP_ROOT, "runtime")
require File.join(TREETOP_ROOT, "compiler")

require 'polyglot'
Polyglot.register(["treetop", "tt"], Treetop)
