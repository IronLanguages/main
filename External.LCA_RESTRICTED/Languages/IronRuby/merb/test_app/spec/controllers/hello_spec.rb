require File.join(File.dirname(__FILE__), '..', 'spec_helper.rb')

describe Hello, "index action" do
  before(:each) do
    dispatch_to(Hello, :index)
  end
end