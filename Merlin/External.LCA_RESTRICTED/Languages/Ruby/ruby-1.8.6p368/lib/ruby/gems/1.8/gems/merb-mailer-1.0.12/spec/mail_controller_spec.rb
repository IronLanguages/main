require File.dirname(__FILE__) + '/spec_helper'

Spec::Runner.configure do |config|
  config.include Merb::Test::RequestHelper
  config.include Merb::Test::ControllerHelper
end

class Merb::Mailer
  self.delivery_method = :test_send
end

Merb.push_path(:mailer, File.join(File.dirname(__FILE__), "mailers"))

class TestMailController < Merb::MailController
  
  def first
    render_mail
  end
  
  def second
    render_mail
  end
  
  def third
    render_mail :text => :first, :html => :third
  end
  
  def fourth
    render_mail :text => "FOURTH_TEXT", :html => "FOURTH_HTML", :layout => :none
  end
  
  def fifth
    render_mail :action => {:text => :first, :html => :third}
  end
  
  def sixth
    render_mail :action => {:text => :first}, :html => "SIXTH_HTML"
  end
  
  def seventh
    render_mail :text => :first, :html => "SEVENTH_HTML"
  end
  
  def eighth
    @testing = "TEST"
    render_mail
  end
  
  def ninth
    render_mail
  end

  def tenth
    render_mail
  end
  
  def generates_relative_url
    render_mail
  end

  def generates_absolute_url
    render_mail
  end
end


Merb::Router.prepare do
  match("/subprojects/:subproject").
    to(:controller => "test_controller", :action => "whatever").
    name(:merb_subproject)
end


class TestController < Merb::Controller
  def one
    send_mail TestMailController, :ninth, {:from => "foo@bar.com", :to => "foo@bar.com"}, {:x => "ONE_CONTROLLER"}
  end
end

describe "A Merb Mail controller" do
  
  def deliver(action)
    TestMailController.dispatch_and_deliver action, :from => "foo@bar.com", :to => "foo@bar.com"
    @delivery = Merb::Mailer.deliveries.last
  end

  undef :call_action if defined?(call_action)
  def call_action(action)
    dispatch_to(TestController, action)
    @delivery = Merb::Mailer.deliveries.last  
  end
  
  it "should render files in its directory by default" do
    deliver :first
    @delivery.text.should == "TEXT\nFIRST\nENDTEXT"
  end
  
  it "should render files in its directory without a mimetype extension by default" do
    deliver :second
    @delivery.text.should == "TEXT\nSECOND\nENDTEXT"
  end
  
  it "should be able to accept a :text => :sym, :html => :sym render_mail" do
    deliver :third
    @delivery.text.should == "TEXT\nFIRST\nENDTEXT"
    @delivery.html.should == "BASIC\nTHIRDHTML\nLAYOUT"
  end
  
  it "should be able to accept a :text => STRING, :html => STRING render_mail" do
    deliver :fourth
    $DEBUGGER = true
    @delivery.text.should == "FOURTH_TEXT"
    @delivery.html.should == "FOURTH_HTML"    
    $DEBUGGER = false
  end
  
  it "should be able to accept an :action => {:text => :sym, :html => :sym}" do
    deliver :fifth
    @delivery.text.should == "TEXT\nFIRST\nENDTEXT"
    @delivery.html.should == "BASIC\nTHIRDHTML\nLAYOUT"    
  end
  
  it "should be able to accept a mix of action and :html => STRING" do
    deliver :sixth
    @delivery.text.should == "TEXT\nFIRST\nENDTEXT"
    @delivery.html.should == "SIXTH_HTML"    
  end
  
  it "should be able to accept a mix of :text => :sym and :html => STRING" do
    deliver :seventh
    @delivery.text.should == "TEXT\nFIRST\nENDTEXT"
    @delivery.html.should == "SEVENTH_HTML"    
  end
  
  it "should hold onto instance variables" do
    deliver :eighth
    @delivery.html.should == "BASIC\nTEST\nLAYOUT"    
  end
  
  it "should have access to the params sent by the calling controller" do
    call_action :one
    @delivery.html.should == "BASIC\nONE_CONTROLLER\nLAYOUT"    
  end
  
  it "should not raise an error if there are no templates" do
    lambda do
      deliver :tenth    
    end.should_not raise_error
  end
  
  it "should log an error if there are no templates available" do
    Merb.logger.should_receive(:error).once
    deliver :tenth
  end

  it "delegates relative url generation to base controller" do
    controller = TestController.new(fake_request)
    TestMailController.new({ :subproject => "core" }, controller).
      dispatch_and_deliver :generates_relative_url, :from => "foo@bar.com", :to => "foo@bar.com"
    
    Merb::Mailer.deliveries.last.text.should == "TEXT\n/subprojects/core\nENDTEXT"
  end

  it "delegates absolute url generation to base controller" do
    controller = TestController.new(fake_request)
    TestMailController.new({ :subproject => "extlib" }, controller).
      dispatch_and_deliver :generates_absolute_url, :from => "foo@bar.com", :to => "foo@bar.com"
    
    Merb::Mailer.deliveries.last.text.should == "TEXT\nhttp://merbivore.com/subprojects/extlib\nENDTEXT"
  end
end
