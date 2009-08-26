# -*- ruby -*-

require 'rubygems'
require 'hoe'

Hoe.add_include_dirs("lib",
                     "../../ParseTree/dev/test",
                     "../../ruby_parser/dev/lib",
                     "../../sexp_processor/dev/lib")

Hoe.plugin :seattlerb

Hoe.spec 'ruby2ruby' do
  developer 'Ryan Davis', 'ryand-ruby@zenspider.com'

  self.rubyforge_name = 'seattlerb'

  extra_deps     << ["sexp_processor", "~> 3.0"]
  extra_deps     << ["ruby_parser",    "~> 2.0"]
  extra_dev_deps << ["ParseTree",      "~> 3.0"]
end

# vim: syntax=ruby
