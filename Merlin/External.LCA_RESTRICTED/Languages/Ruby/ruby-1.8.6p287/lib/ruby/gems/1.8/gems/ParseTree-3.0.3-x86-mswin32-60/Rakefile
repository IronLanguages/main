# -*- ruby -*-

require 'rubygems'
require 'hoe'

Hoe.add_include_dirs("../../RubyInline/dev/lib",
                     "../../sexp_processor/dev/lib",
                     "../../ZenTest/dev/lib",
                     "lib")

require './lib/parse_tree.rb'

Hoe.new("ParseTree", ParseTree::VERSION) do |pt|
  pt.rubyforge_name = "parsetree"

  pt.developer('Ryan Davis', 'ryand-ruby@zenspider.com')

  pt.clean_globs << File.expand_path("~/.ruby_inline")
  pt.extra_deps << ['RubyInline',     '>= 3.7.0']
  pt.extra_deps << ['sexp_processor', '>= 3.0.0']
  pt.spec_extras[:require_paths] = proc { |paths| paths << 'test' }

  pt.multiruby_skip << "mri_rel_1_9" << "rubinius" << "mri_trunk"
end

desc 'Run in gdb'
task :debug do
  puts "RUN: r -d #{Hoe::RUBY_FLAGS} test/test_all.rb #{Hoe::FILTER}"
  sh "gdb ~/.multiruby/install/19/bin/ruby"
end

desc 'Run a very basic demo'
task :demo do
  sh "echo 1+1 | ruby #{Hoe::RUBY_FLAGS} ./bin/parse_tree_show -f"
end
