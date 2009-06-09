class Object
  # ==== Parameters
  # attr<~to_s>:: The name of the instance variable to get.
  #
  # ==== Returns
  # Object:: The instance variable @attr for this object.
  # 
  # ==== Examples
  #   # In a spec
  #   @my_obj.assigns(:my_value).should == @my_value
  def assigns(attr)
    self.instance_variable_get("@#{attr}")
  end
end
