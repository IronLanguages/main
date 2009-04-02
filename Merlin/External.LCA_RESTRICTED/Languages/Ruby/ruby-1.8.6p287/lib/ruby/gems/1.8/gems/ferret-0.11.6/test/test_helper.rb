$:.unshift File.dirname(__FILE__)
$:.unshift File.join(File.dirname(__FILE__), '../lib')
$:.unshift File.join(File.dirname(__FILE__), '../ext')

ENV['LANG'] = "en_US.UTF-8"
ENV['LC_CTYPE'] = "en_US.UTF-8"

class Float
  def approx_eql?(o)
    return (1 - self/o).abs < 0.0001
  end
  alias :=~ :approx_eql?
end

require 'test/unit'
require 'ferret'
require 'unit/index/th_doc' if (defined?(IndexTestHelper).nil?)


def load_test_dir(dir)
  Dir[File.join(File.dirname(__FILE__), dir, "t[scm]*.rb")].each do |file|
    require file
  end
end
