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

module MyModule
end 

class MyClass
end 

def rescue_by thing
    begin 
        divide_by_zero
    rescue thing
        $g += 10
    end
end 

def test_rescue_by_not_class
    [1, MyClass.new, nil, [1,2], {1=>2}, "thing" ].each do |x|
        $g = 1
        begin 
            rescue_by x
        rescue TypeError
            $g += 100
        end
        assert_equal($g, 101)
    end 
end

def test_rescue_by_class
    [MyClass, MyModule, Array, Dir].each do |x|
        $g = 1
        begin 
            rescue_by x
        rescue ZeroDivisionError
            $g += 100
        end 
        assert_equal($g, 101)
    end 
    
    [Object, Exception, ].each do |x|
        $g = 1
        begin 
            rescue_by Object
        rescue 
            $g += 100
        end
        assert_equal($g, 11)
    end
end 

def test_exact_match_before_not_class
    $g = 1
    begin 
        divide_by_zero
    rescue ZeroDivisionError
        $g += 10
    rescue 1                        # will not be evaluated
        $g += 100
    end 
    assert_equal($g, 11)
end 

def test_exact_match_after_not_class
    $g = 1
    begin
        begin 
            divide_by_zero
        rescue 1
            $g += 10
        rescue ZeroDivisionError
            $g += 100
        end
    rescue TypeError
        $g += 1000
    end 
    assert_equal($g, 1001)
end 

test_rescue_by_not_class
test_rescue_by_class
test_exact_match_before_not_class
test_exact_match_after_not_class