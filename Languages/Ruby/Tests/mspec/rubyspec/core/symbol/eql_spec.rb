require File.dirname(__FILE__) + '/../../spec_helper'

describe "Symbol#==" do
  it "only returns true when the other is exactly the same symbol" do
    (:ruby.eql? :ruby).should == true
    (:ruby.eql? :"ruby").should == true
    (:ruby.eql? :'ruby').should == true
    (:@ruby.eql? :@ruby).should == true
    
    (:ruby.eql? :@ruby).should == false
    (:foo.eql? :bar).should == false
    (:ruby.eql? 'ruby').should == false
  end
end
