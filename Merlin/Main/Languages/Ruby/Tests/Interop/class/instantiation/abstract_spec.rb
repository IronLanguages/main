require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/uninstantiable'
require File.dirname(__FILE__) + '/../shared/instantiable'

describe "Empty Abstract classes" do
  it_behaves_like :uninstantiable_class, EmptyAbstractClass
end

describe "Abstract classes" do
  it_behaves_like :uninstantiable_class, AbstractClass
end

describe "Derived from abstract classes" do
  csc <<-EOL
  public partial class DerivedFromAbstract : AbstractClass {
    public override int m() {return 1;}
  }
  EOL
  it_behaves_like :instantiable_class, DerivedFromAbstract
end

describe "Ruby derived from abstract classes" do
  class RubyDerivedFromAbstract
    def m
      1
    end
  end
  it_behaves_like :instantiable_class, RubyDerivedFromAbstract
end

describe "Abstract derived classes" do
  csc <<-EOL
  public abstract partial class AbstractDerived : Klass {}
  EOL
  it_behaves_like :uninstantiable_class, AbstractDerived
end
