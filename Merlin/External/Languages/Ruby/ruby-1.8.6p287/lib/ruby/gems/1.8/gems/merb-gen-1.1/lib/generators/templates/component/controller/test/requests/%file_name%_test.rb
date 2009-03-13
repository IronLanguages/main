require File.join(File.dirname(__FILE__), <%= go_up(modules.size + 1) %>, "test_helper")

# Re-raise errors caught by the controller.
class <%= full_class_name %>; def rescue_action(e) raise e end; end

class <%= full_class_name %>Test < Test::Unit::TestCase

  def setup
    @resonse = request("/<%= full_class_name.to_s.to_const_path %>")
  end

  # Replace this with your real tests.
  def test_should_be_setup
    assert false
  end
end