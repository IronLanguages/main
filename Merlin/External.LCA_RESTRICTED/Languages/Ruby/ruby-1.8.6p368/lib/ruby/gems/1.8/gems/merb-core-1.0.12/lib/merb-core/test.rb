require 'merb-core/test/test_ext/object'
require 'merb-core/test/test_ext/string'

module Merb; module Test; end; end

require 'merb-core/test/helpers'

begin
  require 'webrat'
  require 'webrat/merb'
rescue LoadError => e
  if Merb.testing?
    Merb.logger.warn! "Couldn't load Webrat, so some features, like `visit' will not " \
                      "be available. Please install webrat if you want these features."
  end
end

if Merb.test_framework.to_s == "rspec"
  require 'merb-core/test/test_ext/rspec'
  require 'merb-core/test/matchers'
end
