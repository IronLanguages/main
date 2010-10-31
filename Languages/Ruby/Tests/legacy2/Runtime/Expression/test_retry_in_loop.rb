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

# restarts the loop, and reevaluating the condition ??

def test_jump_error_happen
    def retry_inside_while
        $c = 0
        while $c < 7
            $c += 1
            retry if $c < 2
        end 
    end 
    
    assert_raise(LocalJumpError) { retry_inside_while }
    assert_equal($c, 1)   
    
    def retry_inside_until
        $c = 0
        until $c > 7
            $c += 2
            retry if $c < 4
        end 
    end 
    
    assert_raise(LocalJumpError) { retry_inside_until }
    assert_equal($c, 2)   
end 

def test_jump_error_not_happen
    def retry_inside_while
        $c = 0
        while $c < 7
            $c += 1
            retry if false
        end 
    end 
    retry_inside_while
    
    def retry_inside_until
        $c = 0
        until $c > 7
            $c += 2
            retry if false
        end 
    end 
    retry_inside_until
end 

def test_retry_inside_for_loop
    expr = [ [1, 10], [100, 1000, 10000], [100000] ] 
    
    $sum = 0
    $c = 0
    for x in expr[$c] # will not be evaluated again
        $sum += x
        $c += 1
        $sum += 1000000
    end 
    
    assert_equal($sum, 2000011)
    
    $sum = 0
    $c = 0
    for x in expr[$c] # will be evaluated again, the 10 in the first array will be skipped
        $sum += x
        $c += 1
        retry if $c == 1
        $sum += 1000000
    end 
    
    assert_equal($sum, 3011101)    
end 

test_jump_error_happen
test_jump_error_not_happen
test_retry_inside_for_loop