require File.dirname(__FILE__) + '/../spec_helper'

describe "Type Groups" do
  csc <<-EOL
    public class EmptyTypeGroup { }
    public class EmptyTypeGroup<T> { }

    public class EmptyTypeGroup1<T> {}
    public class EmptyTypeGroup1<T,V> {}

    public class TypeGroup {int m1() {return 1;}}
    public class TypeGroup<T> {int m1() {return 1;}}

    public class TypeGroup1<T> {int m1() {return 1;}}
    public class TypeGroup1<T,V> {int m1() {return 1;}}
  EOL
  it "maps to a Microsoft::Scripting::Actions::TypeGroup" do
    #MS::Scripting isn't autoloaded when it gets returned from TypeGroup.class
    load_assembly 'microsoft.scripting'
    [EmptyTypeGroup, EmptyTypeGroup1,
      TypeGroup, TypeGroup1].each do |klass|
        klass.should be_kind_of Microsoft::Scripting::Actions::TypeGroup
      end
  end
end
