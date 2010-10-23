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

# page: 124
# local variables in a loaded or required file are not propagated to the scope that 
# loads or requires them.

module Pacific
    SIZE = -100
    
    def Pacific.listen
        -101
    end
    
    class Ocean
        FLAG = -102
    end   
    
    module Hawaii
        KIND = -103
    end  
end 

TOP_SIZE = -200

def top_method
    -201
end 

class Top_Class
    FLAG = -202
end

var = -203

require "module_a"

# things changed after require
assert_equal(Pacific::SIZE, 100)
assert_equal(Pacific::listen, 101)
assert_equal(Pacific::Ocean::FLAG, 102)
assert_equal(Pacific::Hawaii::KIND, 103)

assert_equal(TOP_SIZE, 200)
assert_equal(top_method, 201)
assert_equal(Top_Class::FLAG, 202)

# local var stays
assert_equal(var, -203)  # !! 

## 

require "module_var_as_method"

assert_equal(var, -203)
assert_equal(var(), 800)

var = 403
assert_equal(var, 403)
assert_equal(var(), 800)

def var; 900; end
assert_equal(var, 403)
assert_equal(var(), 900)

