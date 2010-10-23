require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiation'

describe "Generic Void delegate instantiation" do
  it_behaves_like :delegate_instantiation, DelegateHolder::GenericVoidDelegate.of(Object)  
end

describe "Generic Reference delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::GenericRefDelegate.of(Object)
end

describe "Generic Value delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::GenericValDelegate.of(Object)
end

describe "Generic Reference array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::GenericARefDelegate.of(Object)
end

describe "Generic Value array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::GenericAValDelegate.of(Object)
end

describe "Generic Generic delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::GenericGenericDelegate.of(Object, Object)
end

