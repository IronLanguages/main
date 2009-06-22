require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiation'

describe "Value array Void delegate instantiation" do
  csc <<-EOL
    public partial class DelegateHolder {
      public delegate int[] AValVoidDelegate();
      public delegate int[] AValRefDelegate(string foo);
      public delegate int[] AValValDelegate(int foo);
      public delegate int[] AValARefDelegate(string[] foo);
      public delegate int[] AValAValDelegate(int[] foo);
      public delegate int[] AValGenericDelegate<T>(T foo);
    }
  EOL
  
  it_behaves_like :delegate_instantiation, DelegateHolder::AValVoidDelegate  
end

describe "Value array Reference delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::AValRefDelegate
end

describe "Value array Value delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::AValValDelegate
end

describe "Value array Reference array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::AValARefDelegate
end

describe "Value array Value array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::AValAValDelegate
end

describe "Value array Generic delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::AValGenericDelegate.of(Object)
end

