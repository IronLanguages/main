require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiation'

describe "Reference Void delegate instantiation" do
  it_behaves_like :delegate_instantiation, DelegateHolder::RefVoidDelegate  
end

describe "Reference Reference delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::RefRefDelegate
end

describe "Reference Value delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::RefValDelegate
end

describe "Reference Reference array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::RefARefDelegate
end

describe "Reference Value array delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::RefAValDelegate
end

describe "Reference Generic delegate" do
  it_behaves_like :delegate_instantiation, DelegateHolder::RefGenericDelegate.of(Object)
end

