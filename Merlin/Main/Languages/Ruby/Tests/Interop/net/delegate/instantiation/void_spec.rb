require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiation'

describe "Void Void delegate instantiation" do
  csc <<-EOL
    public partial class DelegateHolder {
      public delegate void VoidVoidDelegate();
      public delegate void VoidRefDelegate(string foo);
      public delegate void VoidValDelegate(int foo);
      public delegate void VoidARefDelegate(string[] foo);
      public delegate void VoidAValDelegate(int[] foo);
      public delegate void VoidGenericDelegate<T>(T foo);
    }
  EOL
  
  it_behaves_like :delegate_instantiation, DelegateHolder::VoidVoidDelegate  
end

describe "Void Reference delegate instantiation" do
  it_behaves_like :delegate_instantiation, DelegateHolder::VoidRefDelegate
end

describe "Void Value delegate instantiation" do
  it_behaves_like :delegate_instantiation, DelegateHolder::VoidValDelegate
end

describe "Void Reference array delegate instantiation" do
  it_behaves_like :delegate_instantiation, DelegateHolder::VoidARefDelegate
end

describe "Void Value array delegate instantiation" do
  it_behaves_like :delegate_instantiation, DelegateHolder::VoidAValDelegate
end

describe "Void Generic delegate instantiation" do
  it_behaves_like :delegate_instantiation, DelegateHolder::VoidGenericDelegate.of(Object)
end

