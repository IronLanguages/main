require File.dirname(__FILE__) + '/../spec_helper'

describe "Interface Groups" do
  it "maps to a Microsoft::Scripting::Actions::TypeGroup" do
    #MS::Scripting isn't autoloaded when it gets returned from IInterfaceGroup.class
    require 'microsoft.dynamic'
    [IEmptyInterfaceGroup, IEmptyInterfaceGroup1,
      IInterfaceGroup, IInterfaceGroup1].each do |iface|
        iface.should be_kind_of Microsoft::Scripting::Actions::TypeGroup
      end
  end
  
end
