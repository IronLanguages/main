require File.join(File.dirname(__FILE__), "..", 'spec_helper.rb')

describe "Merb::Authentication Session" do
    
  before(:each) do
    @session_class = Merb::CookieSession
    @session = @session_class.generate
  end

  describe "module methods" do
    before(:each) do
      @m = mock("mock")
      clear_strategies!
    end
    
    after(:all) do
      clear_strategies!
    end
    
    describe "store_user" do
      it{@session.authentication.should respond_to(:store_user)}
      
      it "should raise a NotImplemented error by default" do
        pending "How to spec this when we need to overwrite it for the specs to work?"
        lambda do
          @session.authentication.store_user("THE USER")
        end.should raise_error(Merb::Authentication::NotImplemented)
      end
    end
    
    describe "fetch_user" do
      it{@session.authentication.should respond_to(:fetch_user)}
      
      it "should raise a NotImplemented error by defualt" do
        pending "How to spec this when we need to overwrite it for the specs to work?"
        lambda do 
          @session.authentication.fetch_user
        end.should raise_error(Merb::Authentication::NotImplemented)
      end
    end
  end
  
  describe "error_message" do
    
    before(:each) do
      @request = fake_request
      @auth = Merb::Authentication.new(@request.session)
    end
    
    it "should be 'Could not log in' by default" do
      @auth.error_message.should == "Could not log in"
    end
    
    it "should allow a user to set the error message" do
      @auth.error_message = "No You Don't"
      @auth.error_message.should == "No You Don't"
    end
  end
  
  describe "user" do
    it "should call fetch_user with the session contents to load the user" do
      @session[:user] = 42
      @session.authentication.should_receive(:fetch_user).with(42)
      @session.user
    end
    
    it "should set the @user instance variable" do
      @session[:user] = 42
      @session.authentication.should_receive(:fetch_user).and_return("THE USER")
      @session.user
      @session.authentication.assigns(:user).should == "THE USER"
    end
    
    it "should cache the user in an instance variable" do
      @session[:user] = 42
      @session.authentication.should_receive(:fetch_user).once.and_return("THE USER")
      @session.user
      @session.authentication.assigns(:user).should == "THE USER"
      @session.user
    end
    
    it "should set the ivar to nil if the session is nil" do
      @session[:user] = nil
      @session.user.should be_nil
    end
    
  end
  
  describe "user=" do
    before(:each) do
      @user = mock("user")
      @session.authentication.stub!(:fetch_user).and_return(@user)
    end
    
    it "should call store_user on the session to get the value to store in the session" do
      @session.authentication.should_receive(:store_user).with(@user)
      @session.user = @user
    end
    
    it "should set the instance variable to nil if the return of store_user is nil" do
      @session.authentication.should_receive(:store_user).and_return(nil)
      @session.user = @user
      @session.user.should be_nil
    end
    
    it "should set the instance varaible to nil if the return of store_user is false" do
      @session.authentication.should_receive(:store_user).and_return(false)
      @session.user = @user
      @session.user.should be_nil
    end
    
    it "should set the instance variable to the value of user if store_user is not nil or false" do
      @session.authentication.should_receive(:store_user).and_return(42)
      @session.user = @user
      @session.user.should == @user
      @session[:user].should == 42
    end
  end
  
  describe "abandon!" do
    
    before(:each) do
      @user = mock("user")
      @session.authentication.stub!(:fetch_user).and_return(@user)
      @session.authentication.stub!(:store_user).and_return(42)
      @session[:user] = 42
      @session.user
    end
    
    it "should delete the session" do
      @session.should_receive(:clear)
      @session.abandon!
    end
    
    it "should not have a user after it is abandoned" do
      @session.user.should == @user
      @session.abandon!
      @session.user.should be_nil
    end
  end
  
  describe "Merb::Authentication" do
    it "Should be hookable" do
      Merb::Authentication.should include(Extlib::Hook)
    end
    
  end
  
  describe "#authenticate" do
    
    before(:all) do
      clear_strategies!
    end
    
    after(:all) do
      clear_strategies!
    end
    
    before(:each) do
      class Sone < Merb::Authentication::Strategy
        def run!
          Viking.capture(Sone)
          params[:pass_1]
        end
      end
      class Stwo < Merb::Authentication::Strategy
        def run!
          Viking.capture(Stwo) 
          params[:pass_2]
        end
      end
      class Sthree < Merb::Authentication::Strategy
        def run!
          Viking.capture(Sthree)
          params[:pass_3] 
        end
      end
      class Sfour < Merb::Authentication::Strategy
        abstract!
        
        def run!
           "BAD MAN"
        end
      end
      
      Sfour.should_not_receive(:run!)
      @request = Users.new(fake_request)
      @auth = Merb::Authentication.new(@request.session)
      Viking.captures.clear
    end
    
    it "should execute the strategies in the default order" do
      @request.params[:pass_3] = true
      @auth.authenticate!(@request, @request.params)
      Viking.captures.should == %w( Sone Stwo Sthree )
    end
    
    it "should run the strategeis until if finds a non nil non false" do
      @request.params[:pass_2] = true
      @auth.authenticate!(@request, @request.params)
      Viking.captures.should == %w( Sone Stwo )
    end
    
    it "should raise an Unauthenticated exception if no 'user' is found" do
      lambda do
        @auth.authenticate!(@request, @request.params)
      end.should raise_error(Merb::Controller::Unauthenticated)
    end
    
    it "should store the user into the session if one is found" do
      @auth.should_receive(:user=).with("WINNA")
      @request.params[:pass_1] = "WINNA"
      @auth.authenticate!(@request, @request.params)
    end
    
    it "should use the Authentiation#error_message as the error message" do
      @auth.should_receive(:error_message).and_return("BAD BAD BAD")
      lambda do
        @auth.authenticate!(@request, @request.params)
      end.should raise_error(Merb::Controller::Unauthenticated, "BAD BAD BAD")
    end
    
    it "should execute the strategies as passed into the authenticate! method" do
      @request.params[:pass_1] = true
      @auth.authenticate!(@request, @request.params, Stwo, Sone)
      Viking.captures.should == ["Stwo", "Sone"]
    end
    
    
    describe "Strategy loading as strings" do
      
      before :each do
        Merb::Authentication.reset_strategy_lookup!
        
        class Merb::Authentication::Strategies::Zone < Merb::Authentication::Strategy
          def run!
            Viking.capture(Merb::Authentication::Strategies::Zone)
            params[:z_one] 
          end
        end
      end
      
      it "should allow for loading the strategies as strings" do
        @request.params[:z_one] = "z_one"
        @request.session.authenticate!(@request, @request.params, "Zone")
        @request.session.user.should == "z_one"
      end
      
      it "should raise a const misisng error when the error is not namespaced" do
        @request.params[:pass_1] = "s_one"
        lambda do
          @request.session.authenticate!(@request, @request.params, "Sone")
        end.should raise_error(NameError)
      end
        
    
      it "should allow a mix of strategies as strings or classes" do
        @request.params[:pass_2] = "s_two"
        @request.session.authenticate!(@request, @request.params, "Zone", Sone, Stwo)
        Viking.captures.should == %w(Merb::Authentication::Strategies::Zone Sone Stwo)
      end
    end
    
  end
  
  describe "user_class" do
    it "should have User as the default user class if requested" do
      Merb::Authentication.user_class.should == User
    end  
  end
  
  describe "redirection" do
    
    before(:all) do
      class FooController < Merb::Controller
        before :ensure_authenticated
        def index; "FooController#index" end
      end
    end
    
    before(:each) do
      class MyStrategy < Merb::Authentication::Strategy
        def run!
          if params[:url]
            self.body = "this is the body"
            params[:status] ? redirect!(params[:url], :status => params[:status]) : redirect!(params[:url])
          else
            "WINNA"
          end
        end
      end # MyStrategy
      
      class FailStrategy < Merb::Authentication::Strategy
        def run!
          request.params[:should_not_be_here] = true
        end
      end
      
      Merb::Router.reset!
      Merb::Router.prepare{ match("/").to(:controller => "foo_controller")}
      @request = mock_request("/")
      @s = MyStrategy.new(@request, @request.params)
      @a = Merb::Authentication.new(@request.session)
    end
    
    it "should answer redirected false if the strategy did not redirect" do
      @a.authenticate! @request, @request.params
      @a.should_not be_redirected
    end
    
    it "should answer redirected true if the strategy did redirect" do
      @request.params[:url] = "/some/url"
      @a.authenticate! @request, @request.params
      @a.halted?
    end
    
    it "should provide access to the Headers" do
      @request.params[:url] = "/some/url"
      @a.authenticate! @request, @request.params
      @a.headers.should == {"Location" => "/some/url"}
    end
    
    it "should provide access to the status" do
      @request.params[:url] = "/some/url"
      @request.params[:status] = 401
      @a.authenticate! @request, @request.params
      @a.should be_halted
      @a.status.should == 401
    end
    
    it "should stop processing the strategies if one redirects" do
      @request.params[:url] = "/some/url"
      lambda do
        @a.authenticate! @request, @request.params, MyStrategy, FailStrategy
      end.should_not raise_error(Merb::Controller::NotFound)
      @a.should be_halted
      @request.params[:should_not_be_here].should be_nil
    end
    
    it "should allow you to set the body" do
      @a.body = "body"
      @a.body.should == "body"
    end
    
    it "should put the body of the strategy as the response body of the controller" do
      controller = request "/", :params => {:url => "/some/url"}
      controller.should redirect_to("/some/url")
    end
  end
  



end