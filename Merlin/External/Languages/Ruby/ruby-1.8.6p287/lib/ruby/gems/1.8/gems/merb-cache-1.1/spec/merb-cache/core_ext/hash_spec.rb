require File.dirname(__FILE__) + '/../../spec_helper'

describe Hash do
  
  describe "to_sha1" do
    before(:each) do
      @params = {:id => 1, :string => "string", :symbol => :symbol}
    end
    
    it{@params.should respond_to(:to_sha2)}
    
    it "should encode the hash by alphabetic key" do
      string = ""
      @params.keys.sort_by{|k| k.to_s}.each{|k| string << @params[k].to_s}
      digest = Digest::SHA2.hexdigest(string)    
      @params.to_sha2.should == digest      
    end  
  end
  
end