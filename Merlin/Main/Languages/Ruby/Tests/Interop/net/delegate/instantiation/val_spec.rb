require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiation'

describe "Value Void delegate instantiation" do
  csc <<-EOL
    public partial class DelegateHolder {
      public delegate int ValVoidDelegate();
      public delegate int ValRefDelegate(string foo);
      public delegate int ValValDelegate(int foo);
      public delegate int ValARefDelegate(string[] foo);
      public delegate int ValAValDelegate(int[] foo);
      public delegate int ValGenericDelegate<T>(T foo);
    }
  EOL
  
  it_behaves_like :delegate_instantiation, DelegateHolder::ValVoidDelegate  
end

describe "Value Reference delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::ValRefDelegate
end

describe "Value Value delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::ValValDelegate
end

describe "Value Reference array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::ValARefDelegate
end

describe "Value Value array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::ValAValDelegate
end

describe "Value Generic delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::ValGenericDelegate.of(Object)
end

