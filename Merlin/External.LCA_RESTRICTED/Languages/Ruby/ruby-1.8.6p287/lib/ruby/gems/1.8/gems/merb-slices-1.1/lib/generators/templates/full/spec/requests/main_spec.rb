require File.join(File.dirname(__FILE__), '..', 'spec_helper.rb')

describe  "/<%= base_name %>/" do
  
  before(:all) do
    mount_slice
  end
  
  describe "GET /" do
    
    before(:each) do
      @response = request("/<%= base_name %>/")
    end
    
    it "should be successful" do
      @response.status.should be_successful
    end
    
    # This is just an example of what you can do
    # You can also use the other webrat methods to click links,
    # fill up forms etc...
    it "should render the default slice layout" do
      @response.should have_tag(:h1, :content => "<%= module_name %> Slice")
      @response.should have_selector("div#container div#main")
      @response.should have_xpath("//div[@id='container']/div[@id='main']")
    end
    
  end
  
end