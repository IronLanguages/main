#!/usr/local/bin/ruby -w
require 'rubygems'
require 'sexp_processor'

class QuickPrinter < SexpProcessor
  def initialize
    super
    self.strict = false
    self.auto_shift_type = true
  end
  def process_defn(exp)
    name = exp.shift
    args = process exp.shift
    body = process exp.shift
    puts "  def #{name}"
    return s(:defn, name, args, body)
  end
end

QuickPrinter.new.process(*ParseTree.new.parse_tree(QuickPrinter))
