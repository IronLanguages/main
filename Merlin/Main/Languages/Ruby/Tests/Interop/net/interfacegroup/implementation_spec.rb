require File.dirname(__FILE__) + "/../spec_helper"
require File.dirname(__FILE__) + "/fixtures/classes"
module InterfaceSpecs end

describe "Using interface groups" do
  it "works as the non-generic member when included" do
    module InterfaceSpecs
      class WithInterfaceGroup
        include IInterfaceGroup
        def a
          @a
        end
        
        def m
          @a = 1
        end
      end
    end
    a = InterfaceSpecs::WithInterfaceGroup.new.m
    a.a.should == 1
    a.should be_kind_of IInterfaceGroup
  end

  it "works as a module" do
    module IInterfaceGroup1
      def self.foo
        :foo
      end
    end

    IInterfaceGroup1.new.foo.should == :foo
    IInterfaceGroup[String].new.foo.should == :foo
    IInterfaceGroup[String,String].new.foo.should == :foo
  end
end
