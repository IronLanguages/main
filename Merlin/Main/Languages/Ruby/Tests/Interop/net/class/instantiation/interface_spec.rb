require File.dirname(__FILE__) + "/../../spec_helper"
module InterfaceSpec end
describe "Classes with .NET parents or includes" do
  it "System::Type superclass" do
    class InterfaceSpec::N1 < System::Type; end
    InterfaceSpec::N1.should be_classlike
  end

  it "including System::Collections::Generic::IList[Fixnum]" do
    class InterfaceSpec::N2
      include System::Collections::Generic::IList[Fixnum]
    end
    InterfaceSpec::N2.should be_classlike
  end

  it "including System::IComparable" do
    class InterfaceSpec::N3
      include System::IComparable
    end
    InterfaceSpec::N3.should be_classlike
  end
end
