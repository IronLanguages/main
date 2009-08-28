require File.dirname(__FILE__) + '/../spec_helper'

describe "Interface Groups" do
  it "maps to a Microsoft::Scripting::Actions::TypeGroup" do
    require 'microsoft.scripting'
    [IEmptyInterfaceGroup, IEmptyInterfaceGroup1,
      IInterfaceGroup, IInterfaceGroup1].each do |iface|
        iface.should be_kind_of Microsoft::Scripting::Actions::TypeGroup
      end
  end
  
end
