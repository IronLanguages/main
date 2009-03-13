require File.join(File.dirname(__FILE__), <%= go_up(modules.size + 1) %>, 'test_helper' )

class <%= test_class_name %> < Test::Unit::TestCase

  def test_should_be_tested
    assert false
  end

end