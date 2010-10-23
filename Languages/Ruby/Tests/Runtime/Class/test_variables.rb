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

require '../../util/assert.rb'

# basic uages around variables inside the class

class My_variables
    @@sv = 10
    def initialize
        @iv = 20
    end 

    def check_iv; @iv; end 
    def check_sv; @@sv; end
    def My_variables.check_sv; @@sv; end 
end 

assert_equal(My_variables::check_sv, 10)    ## @@sv has been assigned

x = My_variables.new
assert_raise(NoMethodError) { x.iv }
assert_raise(NoMethodError) { My_variables.sv }
assert_raise(NoMethodError) { My_variables::sv }

assert_equal(x.check_sv, 10)
assert_equal(x.check_iv, 20)   


# what if the static variable name is same as instance variable

class My_variables_with_same_name
    @@v = 10
    def initialize
        @v = 20
    end
    def check_iv; @v; end 
    def check_sv; @@v; end     
end 
x = My_variables_with_same_name.new
assert_equal(x.check_sv, 10)
assert_equal(x.check_iv, 20)   

# instance variable is initialized as nil

class My_instance_variable
    @iv = 10
    def check_iv; @iv; end
    def set_iv; @iv = 20; end
end 

x = My_instance_variable.new 
assert_equal(x.check_iv, nil)           ## @iv is not assigned during the class creation
x.set_iv
assert_equal(x.check_iv, 20)
