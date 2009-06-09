require File.dirname(__FILE__) + '/../../spec_helper'

describe "Removing from .NET namespaces" do
  it "is allowed for methods" do
    begin
      module NotEmptyNamespace
        def bar
          1
        end
      end

    ensure
      module NotEmptyNamespace
        undef :bar
      end
      
      class Bar
        include NotEmptyNamespace
      end

      lambda { Bar.new.bar }.should raise_error(NoMethodError)
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
    ensure
      module NotEmptyNamespace
        class << self
          undef :bar
        end
      end

      lambda { NotEmptyNamespace.bar }.should raise_error(NoMethodError)
    end
  end

  it "is allowed for classes" do
    module NotEmptyNamespace
       remove_const :Foo
    end
    lambda { NotEmptyNamespace::Foo }.should raise_error(NameError)
  end

  it "is allowed for Ruby classes" do
    module NotEmptyNamespace
      class Test; end
    end

    module NotEmptyNamespace
      self.send(:remove_const, :Test)
    end

    lambda { NotEmptyNamespace::Test }.should raise_error NameError
  end

  it "is allowed for Ruby modules" do
    module NotEmptyNamespace
      module Bar
        def bar
          1
        end
        module_function :bar
      end
    end

    module NotEmptyNamespace
      self.send(:remove_const, :Bar)
    end

    lambda { NotEmptyNamespace::Bar }.should raise_error NameError
  end
end
