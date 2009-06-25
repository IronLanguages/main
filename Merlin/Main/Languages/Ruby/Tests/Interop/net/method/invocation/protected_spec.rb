require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/calling'

# TODO: test for errors
=begin
describe "Invoking a protected .NET method" do
  before :each do 
    @obj = ClassWithMethods.new
    @result = "protected"
  end
  it_behaves_like :calling_a_method, "protected_method"
end
=end

describe "Invoking a protected .NET method on an inherited Ruby class" do
  class RubyClassWithMethods < ClassWithMethods
  end
  before :each do 
    @obj = RubyClassWithMethods.new
    @result = "protected"
  end

  it_behaves_like :calling_a_method, "protected_method"
end
