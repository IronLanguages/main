require File.join(File.dirname(__FILE__), "..", 'spec_helper.rb')

describe "Failed Login" do
  
  before(:all) do
    Merb::Config[:exception_details] = true
    reset_exceptions!
    class Exceptions < Merb::Controller
      def unauthenticated
        "Unauthenticated"
      end
    end
  end
  
  after(:all) do
    reset_exceptions!
    class Exceptions < Merb::Controller
      def unauthenticated
        "Unauthenticated"
      end
    end
    
    Viking.captures.clear
  end
  
  def reset_exceptions!
    Object.class_eval do
      remove_const(:Exceptions) if defined?(Exceptions)
    end
  end
  
  before(:each) do
    clear_strategies!
    Viking.captures.clear
    Merb::Router.reset!
    Merb::Router.prepare do
      match("/").to(:controller => "a_controller")
      match("/login", :method => :put).to(:controller => "sessions", :action => :update)
    end
    
    class LOne < Merb::Authentication::Strategy
      def run!
        Viking.capture self.class
        params[self.class.name.snake_case.gsub("::", "_")]
      end
    end
    
    class LTwo < LOne; end
    
    class LThree < LOne; end
    
    class AController < Merb::Controller
      before :ensure_authenticated, :with => [LThree]
      def index
        "INDEX OF AController"
      end
    end
    
    class Sessions < Merb::Controller
      before :ensure_authenticated
      def update
        "In the login action"
      end
    end
  end  
  
  it "should fail login and then not try the default login on the second attempt but should try the original" do
    r1 = request("/")
    r1.status.should == 401
    Viking.captures.should == ["LThree"]
    Viking.captures.clear
    r2 = request("/login", :method => "put", :params => {"l_three" => true})
    r2.status.should == 200
    Viking.captures.should == ["LThree"]
  end
  
  it "should not be able to fail many times and still work" do
    3.times do 
      r1 = request("/")
      r1.status.should == 401
      Viking.captures.should == ["LThree"]
      Viking.captures.clear
    end
    r2 = request("/login", :method => "put", :params => {"l_three" => true})
    r2.status.should == 200
    Viking.captures.should == ["LThree"]
  end
  
  
end