require File.dirname(__FILE__) + '/../spec_helper'

describe Templater::Generator, ".custom_action" do
  before do
    @generator_class = Templater::Generator
    @generator_class.stub!(:source_root).and_return('/tmp/source')
  end

  it 'evaluates given block in context of generator' do
    $custom_action_evaluation_result = "not called yet"
    @generator_class.custom_action :update_routes_rb do
      $custom_action_evaluation_result = source_root
    end

    @generator_class.new("/tmp/destination").render!
    $custom_action_evaluation_result.should == "/tmp/source"
  end
end
