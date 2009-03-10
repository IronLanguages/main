require File.dirname(__FILE__) + '/../../spec_helper'

describe "Exception.initialize" do
  it "sets a message and clears the backtrace" do
    e = Exception.new('msg')
    e.set_backtrace(['a', 'b'])
    
    e.send(:initialize, 'newmsg')
    e.message.should == 'newmsg'
    e.backtrace.should == nil
  end
  
   it "sets a default message if no parameter given" do
    e = Exception.new('msg')
    e.set_backtrace(['a', 'b'])
    
    e.send(:initialize)
    e.message.should == 'Exception'
    e.backtrace.should == nil
  end
end
