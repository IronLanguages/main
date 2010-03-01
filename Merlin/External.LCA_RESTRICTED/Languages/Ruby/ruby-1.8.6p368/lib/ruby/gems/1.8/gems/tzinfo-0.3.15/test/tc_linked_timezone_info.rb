$:.unshift File.join(File.dirname(__FILE__), "..", "lib")
require 'test/unit'
require 'tzinfo/linked_timezone_info'

include TZInfo

class TCLinkedTimezoneInfo < Test::Unit::TestCase
  
  def test_identifier
    lti = LinkedTimezoneInfo.new('Test/Zone', 'Test/Linked')
    assert_equal('Test/Zone', lti.identifier)
  end
  
  def test_link_to_identifier
    lti = LinkedTimezoneInfo.new('Test/Zone', 'Test/Linked')
    assert_equal('Test/Linked', lti.link_to_identifier)
  end
end
