require "include"
require "runit/cui/testrunner"

# tests the customization of Log4r levels
class TestCustom < TestCase
  def test_validation
    assert_exception(TypeError) { Configurator.custom_levels "lowercase" }
    assert_exception(TypeError) { Configurator.custom_levels "With space" }
  end

  def test_create
    assert_no_exception { Configurator.custom_levels "Foo", "Bar", "Baz" }
    assert_no_exception { Configurator.custom_levels }
    assert_no_exception { Configurator.custom_levels "Bogus", "Levels" }
  end
  def test_methods
    l = Logger.new 'custom1'
    assert_respond_to(:foo, l)
    assert_respond_to(:foo?, l)
    assert_respond_to(:bar, l)
    assert_respond_to(:bar?, l)
    assert_respond_to(:baz, l)
    assert_respond_to(:baz?, l)
    assert_no_exception(NameError) { Bar }
    assert_no_exception(NameError) { Baz }
    assert_no_exception(NameError) { Foo }
  end
    
end

CUI::TestRunner.run(TestCustom.new("test_validation"))
CUI::TestRunner.run(TestCustom.new("test_create"))
CUI::TestRunner.run(TestCustom.new("test_methods"))
