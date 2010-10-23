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

def test_1_defaultvalue
    def m(arg=7)
        arg
    end

    assert_return(7) { m }
    assert_return(9) { m 9 }
    assert_return(7) { m 7 }
    assert_return("abc") { m "abc" }
    assert_raise(ArgumentError) { m 7, 8 }

    assert_return(7) { m *[]}
    assert_return(17) { m *[17]}
end 

def test_1_normal_1_default
    def m(arg1, arg2=10) 
        [arg1, arg2]
    end

    assert_raise(ArgumentError) { m }
    assert_return([1, 10]) { m 1 }
    assert_return([2, 3]) { m 2, 3 }
    assert_raise(ArgumentError) { m 4, 5, 6 }

    assert_raise(ArgumentError) { m *[]}
    assert_return([17, 10]) { m *[17]}
end 

def test_2_defaultvalues
    def m(arg1=20, arg2=30) 
        [arg1, arg2]
    end

    assert_return([20, 30]) { m }
    assert_return(["a", 30]) { m "a" }
    assert_return([3, 5]) { m 3, 5 }
    assert_return([nil, nil]) { m nil, nil }
    assert_raise(ArgumentError) { m 4, 5, 6 }

    assert_return([20, 30]) { m *[]}
    assert_return(["a", 30]) { m *["a"]}
    assert_return(["b", "c"]) { m "b", *["c"]}
end 

# what can be the default value
# when the default value is evaluated
# order of how default value is evaluated

def test_local_var_as_defaultvalue
    local_var = 100
    def m(arg = local_var)
        arg
    end 
    assert_raise(NameError) { m } # undefined local variable or method `local_var' for main:Object (NameError)
    assert_return(120) { m 120 }
    
    local_var = 150
    assert_raise(NameError) { m }
end 

def test_global_var_as_defaultvalue
    $global_var = 200
    def m(arg = $global_var)
        arg
    end 
    assert_return(200) { m }
    
    $global_var = 220
    assert_return(220) { m }

    $global_var = 300
    def m(arg1 = ($global_var+=1), arg2 = ($global_var+=11))
        return arg1, arg2
    end 
    assert_return([301, 312]) { m }
end 

def test_expression_as_defaultvalue
    def m(arg1 = 400, arg2 = (arg1+=1; arg1 + 10))  # first arg changed after assign...
        [arg1, arg2]
    end 
    assert_return([401, 411]) { m }  ## !!!
    assert_return([501, 511]) { m 500 }
    assert_return([600, 700]) { m 600, 700 }
end 

def test_use_second_arg_to_get_first_arg_default_value
    def m(arg1 = arg2 + 1, arg2 = 100) 
        [arg1, arg2]
    end 
    assert_raise(NameError) { m }
end 

def test_new_defined_local_var_by_defaultvalue
    def m(a, b=(c = 10; 20), d=(e=b+30; e+40))
        return a, b, c, d, e
    end 

    assert_return([0, 20, 10, 90, 50]) { m 0 }
    assert_return([1, 2, nil, 72, 32]) { m 1, 2 }
    assert_return([3, 4, nil, 5, nil]) { m 3, 4, 5 }
end 

def test_array_as_defaultvalue
    def m(a, b=(c=20, 30))  # comma
        return a, b, c
    end 

    assert_return([10, [20, 30], [20, 30]]) { m 10 }
    assert_return([40, 50, nil]) { m 40, 50 }
end 

def test_exception
    $g = 1
    def m(a, b= begin; if a == 1; raise "msg"; end; end)
        $g += 10
    rescue 
        $g += 100
    end 
    
    m 2
    assert_equal($g, 11)
    
    $g = 1
    begin 
        m 1
    rescue 
        $g += 1000
    end 
    assert_equal($g, 1001)
end 

def test_statement
    $g = 1
    def m(a, b=(def m; $g += 10; end; m))
        $g += 100
    end 
    m 7
    assert_equal($g, 111)
end 

def test_evaluate_during_invocation
    $g = 1
    def m(a, b=($g += 10))
        $g += 100
    end 
    
    m 1
    assert_equal($g, 111)
    m 2
    assert_equal($g, 221)
    m 1, 2
    assert_equal($g, 321)
end 

test_1_defaultvalue
test_1_normal_1_default
test_2_defaultvalues
#test_local_var_as_defaultvalue
test_global_var_as_defaultvalue
test_expression_as_defaultvalue
#test_use_second_arg_to_get_first_arg_default_value
#test_new_defined_local_var_by_defaultvalue
#test_array_as_defaultvalue
test_exception
test_statement
test_evaluate_during_invocation