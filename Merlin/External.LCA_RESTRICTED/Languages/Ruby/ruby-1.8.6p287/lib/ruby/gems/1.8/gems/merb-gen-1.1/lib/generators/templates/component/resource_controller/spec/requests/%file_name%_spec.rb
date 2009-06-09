require File.join(File.dirname(__FILE__), <%= go_up(modules.size + 1) %>, 'spec_helper.rb')

given "a <%= singular_model %> exists" do
<%- if orm.to_sym == :datamapper -%>
  <%= model_class_name %>.all.destroy!
<%-elsif orm.to_sym == :activerecord -%>
  <%= model_class_name %>.delete_all
<% end -%>
  request(resource(:<%= plural_model %>), :method => "POST", 
    :params => { :<%= singular_model %> => { :id => nil }})
end

describe "resource(:<%= plural_model %>)" do
  describe "GET" do
    
    before(:each) do
      @response = request(resource(:<%= plural_model %>))
    end
    
    it "responds successfully" do
      @response.should be_successful
    end

    it "contains a list of <%= plural_model %>" do
      pending
      @response.should have_xpath("//ul")
    end
    
  end
  
  describe "GET", :given => "a <%= singular_model %> exists" do
    before(:each) do
      @response = request(resource(:<%= plural_model %>))
    end
    
    it "has a list of <%= plural_model %>" do
      pending
      @response.should have_xpath("//ul/li")
    end
  end
  
  describe "a successful POST" do
    before(:each) do
    <%- if orm.to_sym == :datamapper -%>
      <%= model_class_name %>.all.destroy!
    <%-elsif orm.to_sym == :activerecord -%>
      <%= model_class_name %>.delete_all
    <% end -%>
      @response = request(resource(:<%= plural_model %>), :method => "POST", 
        :params => { :<%= singular_model %> => { :id => nil }})
    end
    
    it "redirects to resource(:<%= plural_model %>)" do
      <%- if orm.to_sym == :datamapper -%>
      @response.should redirect_to(resource(<%= model_class_name %>.first), :message => {:notice => "<%= singular_model %> was successfully created"})
      <%- elsif orm.to_sym == :activerecord -%>
      @response.should redirect_to(resource(<%= model_class_name %>.first), :message => {:notice => "<%= singular_model %> was successfully created"})
      <% end -%>
    end
    
  end
end

describe "resource(@<%= singular_model %>)" do 
  describe "a successful DELETE", :given => "a <%= singular_model %> exists" do
     before(:each) do
       @response = request(resource(<%= model_class_name %>.first), :method => "DELETE")
     end

     it "should redirect to the index action" do
       @response.should redirect_to(resource(:<%= plural_model %>))
     end

   end
end

describe "resource(:<%= plural_model %>, :new)" do
  before(:each) do
    @response = request(resource(:<%= plural_model %>, :new))
  end
  
  it "responds successfully" do
    @response.should be_successful
  end
end

describe "resource(@<%= singular_model %>, :edit)", :given => "a <%= singular_model %> exists" do
  before(:each) do
    @response = request(resource(<%= model_class_name %>.first, :edit))
  end
  
  it "responds successfully" do
    @response.should be_successful
  end
end

describe "resource(@<%= singular_model %>)", :given => "a <%= singular_model %> exists" do
  
  describe "GET" do
    before(:each) do
      @response = request(resource(<%= model_class_name %>.first))
    end
  
    it "responds successfully" do
      @response.should be_successful
    end
  end
  
  describe "PUT" do
    before(:each) do
      @<%= singular_model %> = <%= model_class_name %>.first
      @response = request(resource(@<%= singular_model %>), :method => "PUT", 
        :params => { :<%= singular_model %> => {:id => @<%= singular_model %>.id} })
    end
  
    it "redirect to the <%= singular_model %> show action" do
      @response.should redirect_to(resource(@<%= singular_model %>))
    end
  end
  
end

