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


def test_call_itself
    def m
        m
    end
    assert_raise(SystemStackError) { m }
end 

def test_factorial
    def factorial(m)
        if m < 0
            raise ArgumentError, "m should be >=0"
        end 
        
        if m == 0 or m == 1
            return 1
        else
            return m * factorial(m-1)
        end
    end

    assert_raise(ArgumentError) { factorial -4 }
    assert_return(1) { factorial 0 }
    assert_return(120) { factorial 5 }
    #assert_return(2432902008176640000) { factorial 20 }  # bug 282346
end

#test_call_itself  # bug 280501
test_factorial