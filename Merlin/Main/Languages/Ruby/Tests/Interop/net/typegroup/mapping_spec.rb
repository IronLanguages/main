require File.dirname(__FILE__) + '/../spec_helper'

describe "Type Groups" do
  it "maps to a Microsoft::Scripting::Actions::TypeGroup" do
    #MS::Scripting isn't autoloaded when it gets returned from TypeGroup.class
    load_assembly 'microsoft.dynamic'
    [EmptyTypeGroup, EmptyTypeGroup1,
      TypeGroup, TypeGroup1].each do |klass|
        klass.should be_kind_of Microsoft::Scripting::Actions::TypeGroup
      end
  end
end
