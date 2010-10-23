require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + "/../../shared/modification"

describe "Removing methods from a Class" do
  before(:each) do
    @klass = Klass
    @obj = Klass.new
    class << @obj
      def bar_s
        3
      end
    end
  end
  it_behaves_like :removing_a_method, Klass
  it_behaves_like :removing_class_methods, Klass
  it_behaves_like :removing_metaclass_methods, Klass
end
