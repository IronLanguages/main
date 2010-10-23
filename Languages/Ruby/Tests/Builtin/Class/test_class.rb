# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require "../../Util/simple_test.rb"

describe "inspecting class attributes" do
  it "reads the name of a class" do
    "bob".class.name.should == "String"
    "bob".class.to_s.should == "String"
    "bob".class.class.name.should == "Class"
    String.name.should == "String"
    String.class.name.should == "Class"
  end
end

finished
