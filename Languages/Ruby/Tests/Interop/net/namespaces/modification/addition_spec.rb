require File.dirname(__FILE__) + '/../../spec_helper'

describe "Adding to .NET namespaces" do
  it "is allowed for methods" do
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
    end
  end

  it "is allowed for methods with module_function" do
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
    end
  end

  it "is allowed for Ruby classes" do
    begin
      module NotEmptyNamespace
        class Test; end
      end

      NotEmptyNamespace::Test.new.should be_kind_of NotEmptyNamespace::Test
    ensure
      module NotEmptyNamespace
        self.send(:remove_const, :Test)
      end
    end
  end

  it "is allowed for ruby modules" do
    begin
      module NotEmptyNamespace
        module Bar
          def bar
            1
          end
          module_function :bar
        end
      end

      NotEmptyNamespace::Bar.bar.should == 1
    ensure
      module NotEmptyNamespace
        self.send(:remove_const, :Bar)
      end
    end
  end
end
