require File.dirname(__FILE__) + '/../../spec_helper'

describe 'Mixing in' do
  it "namespaces to Ruby classes" do
    class RubyClass
      def self.foo
        Foo.bar
      end
    end

    lambda { RubyClass.foo }.should raise_error NameError

    class RubyClass
      include NotEmptyNamespace
    end

    RubyClass.foo.should == 1
  end
end
