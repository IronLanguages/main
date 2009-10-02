require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::Int32" do
  before(:each) do
    @size = NumericHelper.size_of_int32
  end
  
  it_behaves_like "A .NET numeric", System::Int32
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Int32
  it_behaves_like :numeric_size, System::Int32
  it_behaves_like :numeric_conversion, System::Int32
  
  it "is Fixnum" do
    System::Int32.should == Fixnum
  end

  it "gives preference to ruby methods before .NET methods" do
    module FixnumMixin
      def to_string
        "In FixnumMixin#to_string"
      end
    end

    class Fixnum
      include FixnumMixin
    end
    
    1.to_string.should == "In FixnumMixin#to_string"
  end

  it "doesn't map CLR operators" do
    begin
      class Fixnum
        def c
          @c
        end
        alias_method :compare, :<=>

        def <=>(other)
          @c = other
          compare(other)
        end
      end
      a=1
      a.between?(0,2)
      a.c.should == 2
    ensure
      class Fixnum
        undef :c
        alias_method :<=>, :compare
      end
    end

  end
end
