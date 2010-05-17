describe :calling_a_method, :shared => true do
  it "works directly" do 
    eval("@obj.#{@method}").should equal_clr_string(@result)
  end

  it "works via .send" do
    @obj.send(@method.to_sym).should equal_clr_string(@result)
  end

  it "works via .__send__" do
    @obj.__send__(@method.to_sym).should equal_clr_string(@result)
  end

  it "works via .instance_eval" do
    @obj.instance_eval(@method).should equal_clr_string(@result)
  end
end
