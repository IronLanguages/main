require File.dirname(__FILE__) + '/../spec_helper'

describe "Interface Groups" do
  csc <<-EOL
    public interface IEmptyInterfaceGroup { }
    public interface IEmptyInterfaceGroup<T> { }

    public interface IEmptyInterfaceGroup1<T> {}
    public interface IEmptyInterfaceGroup1<T,V> {}

    public interface IInterfaceGroup {void m1();}
    public interface IInterfaceGroup<T> {void m1();}

    public interface IInterfaceGroup1<T> {void m1();}
    public interface IInterfaceGroup1<T,V> {void m1();}
  EOL
  it "maps to a Microsoft::Scripting::Actions::TypeGroup" do
    #MS::Scripting isn't autoloaded when it gets returned from IInterfaceGroup.class
    require 'microsoft.scripting'
    [IEmptyInterfaceGroup, IEmptyInterfaceGroup1,
      IInterfaceGroup, IInterfaceGroup1].each do |iface|
        iface.should be_kind_of Microsoft::Scripting::Actions::TypeGroup
      end
  end
  
end
