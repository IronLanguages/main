require File.join(File.dirname(__FILE__), <%= go_up(modules.size + 1) %>, 'spec_helper.rb')

describe "/<%= full_class_name.to_s.to_const_path %>" do
  before(:each) do
    @response = request("/<%= full_class_name.to_s.to_const_path %>")
  end
end