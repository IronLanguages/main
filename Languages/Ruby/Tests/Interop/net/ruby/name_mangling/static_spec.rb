require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/mangling'

describe "Name mangling on static methods" do
  before(:each) do
    @objs = [StaticNameHolder, SubStaticNameHolder, Class.new(StaticNameHolder), Class.new(SubStaticNameHolder)]
  end
  it_behaves_like :name_mangling, nil
end
