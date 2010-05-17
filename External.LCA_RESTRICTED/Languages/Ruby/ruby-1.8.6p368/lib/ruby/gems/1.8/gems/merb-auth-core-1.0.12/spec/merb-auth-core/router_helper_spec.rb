require File.join(File.dirname(__FILE__), "..", 'spec_helper.rb')
require 'ruby-debug'

describe "router protection" do

  before(:each) do
    class Foo < Merb::Controller
      def index; "INDEX"; end
    end
    
    clear_strategies!
    
    Object.class_eval do
      remove_const("Mone") if defined?(Mone)
      remove_const("Mtwo") if defined?(Mtwo)
      remove_const("Mthree") if defined?(Mthree)
    end
    
    Viking.captures.clear
    
    class Mone < Merb::Authentication::Strategy
      def run!
        Viking.capture self.class
        if request.params[self.class.name]
          request.params[self.class.name]
        elsif request.params[:url]
          redirect!(request.params[:url])
        end
      end
    end
    
    class Mthree < Mone; end
    class Mtwo < Mone; end

    Merb::Router.prepare do
      to(:controller => "foo") do
        authenticate do
          match("/single_level_default").register
            
          authenticate(Mtwo) do
            match("/nested_specific").register
          end
        end
              
        authenticate(Mtwo, Mone) do
          match("/single_level_specific").register
        end
      end
    end
  end
  
  describe "single level default" do
    
    it "should allow access to the controller if the strategy passes" do
      result = request("/single_level_default", :params => {"Mtwo" => true})
      result.body.should == "INDEX" 
      Viking.captures.should == %w(Mone Mthree Mtwo)
    end
    
    it "should fail if no strategies match" do
      result = request("/single_level_default")
      result.status.should == Merb::Controller::Unauthenticated.status
    end
    
    it "should set return a rack array if the strategy redirects" do
      result = request("/single_level_default", :params => {"url" => "/some/url"})
      result.status.should == 302
      result.body.should_not =="INDEX"
    end
  end
  
  describe "nested_specific" do
    
    it "should allow access to the controller if the strategy passes" do
      result = request("/nested_specific", :params => {"Mtwo" => true})
      result.body.should == "INDEX" 
      Viking.captures.should == %w(Mone Mthree Mtwo)
    end
    
    it "should fail if no strategies match" do
      result = request("/nested_specific")
      result.status.should == Merb::Controller::Unauthenticated.status
    end
    
    it "should set return a rack array if the strategy redirects" do
      result = request("/nested_specific", :params => {"url" => "/some/url"})
      result.status.should == 302
      result.body.should_not =="INDEX"
    end
  end
  
  describe "single_level_specific" do
    
    it "should allow access to the controller if the strategy passes" do
      result = request("/single_level_specific", :params => {"Mone" => true})
      result.body.should == "INDEX" 
      Viking.captures.should == %w(Mtwo Mone)
    end
    
    it "should fail if no strategies match" do
      result = request("/single_level_specific")
      result.status.should == Merb::Controller::Unauthenticated.status
    end
    
    it "should set return a rack array if the strategy redirects" do
      result = request("/single_level_specific", :params => {"url" => "/some/url"})
      result.status.should == 302
      result.body.should_not =="INDEX"
    end
  end
  
end