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

# the retry statement can be used within a rescue clause to restart the enclosing begin/end block from the beginning

def test_retry_in_rescue
    def my_retry(gate)
        $g = 1
        begin 
            $g += 10
            if $g < gate
                divide_by_zero
            end 
        rescue ZeroDivisionError
            $g += 100
            retry
            $g += 1000
        end
    end 

    my_retry(5)
    assert_equal($g, 11)

    my_retry(12)
    assert_equal($g, 121)

    my_retry(122)
    assert_equal($g, 231)

    my_retry(232)
    assert_equal($g, 341)
end 

# ensure will hit once and only once.
def test_related_to_ensure
    $g = 1
    begin 
        $g += 10
        if $g < 342
            divide_by_zero
        end 
    rescue ZeroDivisionError
        $g += 100
        retry
        $g += 1000
    ensure
        $g += 10000
    end
    assert_equal($g, 10451)
end 

# LJE NOT rescuable in the current function 

# inside the "try" block
def test_retry_in_try
    $g = 1
    def f
        begin 
            $g += 10
            retry
            $g += 10
        rescue LocalJumpError
            $g += 100
        end
    end 

    begin 
        f
    rescue LocalJumpError
        $g += 1000
    end
    assert_equal($g, 1011)
end 

# inside the "else" block
def test_retry_in_else
    $g = 1
    def f 
        begin 
            $g += 10
        rescue 
            $g += 100
        else 
            $g += 1000
            retry
            $g += 1000
        end
    end 

    begin 
        f
    rescue LocalJumpError
        $g += 10000
    end
    assert_equal($g, 11011)
end 

# inside the "ensure" block
def test_retry_in_ensure
    $g = 1
    def f 
        begin
            $g += 10
        ensure 
            $g += 100
            retry
            $g += 100
        end 
    end

    begin
        f
    rescue LocalJumpError
        $g += 1000
    end
    assert_equal($g, 1111)
end 

test_retry_in_rescue
test_related_to_ensure
test_retry_in_try
test_retry_in_else
test_retry_in_ensure
