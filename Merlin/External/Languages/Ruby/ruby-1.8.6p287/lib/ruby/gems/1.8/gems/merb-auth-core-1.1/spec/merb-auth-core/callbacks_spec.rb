require File.join(File.dirname(__FILE__), "..", 'spec_helper.rb')

describe "Authentication callbacks" do
  
  before(:each) do
    Merb::Authentication.after_callbacks.clear
    clear_strategies!
    Viking.captures.clear
    
    # A basic user model that has some simple methods 
    # to set and aknowlege that it's been called  
    class AUser
      attr_accessor :active, :name
      
      def initialize(params)
        params.each do |k,v|
          instance_variable_set("@#{k}", v)
        end
      end
      
      def acknowledge(value)
        Viking.capture(value)
      end
      
      def acknowledge!(value = "default acknowledge")
        throw(:acknowledged, value)
      end

      def method_missing(name, *args)
        if /(.*?)\?$/ =~ name.to_s
          !!instance_variable_get("@#{$1}")
        end
      end
    end
    
    # Create a strategy to test the after stuff
    class MyStrategy < Merb::Authentication::Strategy
      def run!
        AUser.new(request.params[:user] || {}) unless request.params[:no_user]
      end
    end
  
    @request  = fake_request
    @params   = @request.params
    @auth     = Merb::Authentication.new(@request.session)
  end
  
  after(:all) do
    clear_strategies!
    Merb::Authentication.after_callbacks.clear
  end
  
  it "should allow you to setup a callback as a block" do
    Merb::Authentication.after_authentication{ |user, request, params| user.acknowledge!("w00t threw it") }
    result = catch(:acknowledged) do
      @request.session.authenticate!(@request, @params)
    end
    result.should == "w00t threw it"
  end
  
  it "should allow you to setup a callback as a method" do
    Merb::Authentication.after_authentication(:acknowledge!)
    result = catch(:acknowledged) do
      result = @request.session.authenticate!(@request,@params)
    end
    result.should == "default acknowledge"
  end
  
  it "should allow many callbacks to be setup and executed" do
    Merb::Authentication.after_authentication{|u,r,p| u.acknowledge("first");  u}
    Merb::Authentication.after_authentication{|u,r,p| u.acknowledge("second"); u}
    @request.session.authenticate!(@request, @params)
    Viking.captures.should == %w(first second)
  end

  it "should stop processing if the user is not returned from the callback" do
    Merb::Authentication.after_authentication{|u,r,p| u.acknowledge("first");  nil}
    Merb::Authentication.after_authentication{|u,r,p| u.acknowledge("second"); u}
    lambda do
      @request.session.authenticate!(@request,@params)
    end.should raise_error(Merb::Controller::Unauthenticated)
    Viking.captures.should == ["first"]
  end
  
  it "should raise an Unauthenticated if a callback returns nil" do
    Merb::Authentication.after_authentication{|u,r,p| nil }
    lambda do
      @request.session.authenticate!(@request,@params)
    end.should raise_error(Merb::Controller::Unauthenticated)
  end
  
  it "should not try to process the callbacks when no user is found" do
    Merb::Authentication.after_authentication{|u,r,p| u.acknowledge("first");  u}
    Merb::Authentication.after_authentication{|u,r,p| u.acknowledge("second"); u}
    @request.params[:no_user] = true
    lambda do
      @request.session.authenticate!(@request,@params)
    end.should raise_error(Merb::Controller::Unauthenticated)
    Viking.captures.should be_empty
  end
    
end