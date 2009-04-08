require File.dirname(__FILE__) + '/../../spec_helper'

describe "Adding methods to .NET namespaces" do
  it "is allowed" do
    begin
      module NotEmptyNamespace
        def bar
          1
        end
      end
      
      class Bar
        include NotEmptyNamespace
      end

      Bar.new.bar.should == 1
    ensure
      module NotEmptyNamespace
        undef :bar
      end

      lambda { Bar.new.bar }.should raise_error(NoMethodError)
    end
  end

  it "is allowed for module functions" do
    begin
      module NotEmptyNamespace
        def bar
          1
        end
        module_function :bar
      end

      NotEmptyNamespace.bar.should == 1
    ensure
      module NotEmptyNamespace
        class << self
          undef :bar
        end
      end
      lambda { NotEmptyNamespace.bar }.should raise_error
    end
  end
end
