require 'test/unit'
require 'fox16'
require 'testcase'

include Fox

class TC_FXSettings < TestCase

  def setup
    super(self.class.name)
  end
  
  def test_each_section_empty_settings
    empty = FXSettings.new
    num_sections = 0
    empty.each_section do |sect|
      num_sections = num_sections + 1
    end
    assert_equal(0, num_sections)
  end
  
  def test_each_section
    settings = FXSettings.new
    settings.writeStringEntry('sect1', 'key1', 'value1')
    settings.writeStringEntry('sect2', 'key2', 'value2')
    keys = []
    settings.each_section do |sect|
      sect.each_key do |key|
        keys << key
      end
    end
    assert_equal(['key1', 'key2'], keys.sort)
  end
end
