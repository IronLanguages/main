# -*- ruby -*-

require 'rubygems'
require 'hoe'

Hoe.add_include_dirs("lib",
                     "../../ZenTest/dev/lib",
                     "../../ParseTree/dev/test",
                     "../../ParseTree/dev/lib",
                     "../../RubyInline/dev/lib",
                     "../../ruby_parser/dev/lib",
                     "../../sexp_processor/dev/lib")

require './lib/ruby2ruby.rb'

Hoe.new('ruby2ruby', Ruby2Ruby::VERSION) do |r2r|
  r2r.rubyforge_name = 'seattlerb'
  r2r.developer('Ryan Davis', 'ryand-ruby@zenspider.com')

  r2r.clean_globs << File.expand_path("~/.ruby_inline")
  r2r.extra_deps << ["ParseTree", "~> 3.0"]
end

task :test => :clean

task :rcov_info do
  pat = ENV['PATTERN'] || "test/test_*.rb"
  ruby "#{Hoe::RUBY_FLAGS} -S rcov --text-report --save coverage.info #{pat}"
end

task :rcov_overlay do
  rcov, eol = Marshal.load(File.read("coverage.info")).last[ENV["FILE"]], 1
  puts rcov[:lines].zip(rcov[:coverage]).map { |line, coverage|
    bol, eol = eol, eol + line.length
    [bol, eol, "#ffcccc"] unless coverage
  }.compact.inspect
end

# vim: syntax=Ruby
