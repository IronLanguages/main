require File.dirname(__FILE__) + '/../spec_helper'

describe "Interfaces" do
  csc <<-EOL
    public interface IEmptyInterface {}
    public interface IInterface { void m();}
  EOL
  it "map to modules" do
    [IEmptyInterface, IInterface].each do |iface|
      iface.should be_kind_of Module
    end
  end
end
