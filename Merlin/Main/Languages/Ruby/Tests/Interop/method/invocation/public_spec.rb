require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/calling'

describe "Invoking a public .NET method" do
  before :each do 
    @obj = ClassWithMethods.new
    @result = "public"
  end
  it_behaves_like :calling_a_method, "public_method"
end

describe "Invoking a public .NET method on an inherited Ruby class" do
  class RubyClassWithMethods < ClassWithMethods
  end
  before :each do 
    @obj = RubyClassWithMethods.new
    @result = "public"
  end

  it_behaves_like :calling_a_method, "public_method"
end
