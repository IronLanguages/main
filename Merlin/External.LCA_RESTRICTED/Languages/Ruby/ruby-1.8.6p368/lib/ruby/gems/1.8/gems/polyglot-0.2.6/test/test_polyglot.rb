require File.dirname(__FILE__) + '/test_helper.rb'

class TestPolyglot < Test::Unit::TestCase
  TEST_FILE = 'test_file.stub'
  class StubLoader
    def self.load(*args); end
  end

  def setup
    Polyglot.register('stub', StubLoader)
    File.open(TEST_FILE, 'w') { |f| f.puts "Test data" }
  end

  def teardown
    File.delete(TEST_FILE)
  end
  
  def test_load_by_absolute_path
    full_path = File.expand_path(TEST_FILE.sub(/.stub$/, ''))
    assert_nothing_raised { require full_path }
  end
end
