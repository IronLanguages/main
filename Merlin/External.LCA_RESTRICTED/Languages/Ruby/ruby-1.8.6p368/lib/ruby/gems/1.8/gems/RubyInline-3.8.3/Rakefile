# -*- ruby -*-

require 'rubygems'
require 'hoe'

Hoe.add_include_dirs "../../ZenTest/dev/lib"
Hoe.add_include_dirs "lib"

Hoe.plugin :seattlerb

Hoe.spec "RubyInline" do
  developer 'Ryan Davis', 'ryand-ruby@zenspider.com'

  clean_globs << File.expand_path("~/.ruby_inline")
  spec_extras[:requirements] =
    "A POSIX environment and a compiler for your language."
  extra_deps << 'ZenTest' # ZenTest mapping
end

task :test => :clean

desc "run all examples"
task :examples do
  %w(example.rb example2.rb
     tutorial/example1.rb
     tutorial/example2.rb).each do |e|
    rm_rf '~/.ruby_inline'
    ruby "-Ilib -w #{e}"
  end
end

desc "run simple benchmarks"
task :bench do
  verbose(false) do
    ruby "-Ilib ./example.rb"
    ruby "-Ilib ./example.rb 1000000 12" # 12 is the bignum cutoff for factorial
  end
end
