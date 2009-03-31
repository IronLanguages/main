require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/uninstantiable'

describe "Empty Static classes" do
  it_behaves_like :uninstantiable_class, EmptyStaticClass
end

describe "Static classes" do
  it_behaves_like :uninstantiable_class, StaticClass
end
