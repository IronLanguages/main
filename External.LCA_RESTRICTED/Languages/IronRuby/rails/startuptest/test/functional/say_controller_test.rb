require File.dirname(__FILE__) + '/../test_helper'

class SayControllerTest < ActionController::TestCase
  def test_hello
    get :hello
    assert_template "say/hello"
    assert_response :success
    assert_tag :tag => "h1", :child => /john/
  end
end
