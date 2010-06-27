require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiation'

describe "Value Void delegate instantiation" do
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

