require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiation'

describe "Value array Void delegate instantiation" do
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

