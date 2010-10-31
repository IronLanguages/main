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

def test_redo_at_first
    $g = x = y = 0
    bools = [true, false, false, false]     # if bools[x] is re-evaluated during the redo, it is "false"
    while bools[x]
        $g += 7
        x += 1
        redo if $g < 15                   # redo at the first loop
        y += 1
    end 
    assert_equal($g, 21)
    assert_equal(x, 3)
    assert_equal(y, 1)
end 

def test_redo_at_second
    $g = x = y = 0
    bools = [true, true, false, true, false]
    while bools[x]
        $g += 8
        x += 1
        redo if $g > 9 and $g < 17       # redo at the second loop
        y += 1
    end 
    assert_equal($g, 32)
    assert_equal(x, 4)
    assert_equal(y, 3)
end 

def test_redo_at_interval
    $g = x = y = 0
    bools = [true, false, true, false, true, false]
    while bools[x]
        $g += 6
        x += 1
        redo if $g < 11 or ($g > 17 and $g < 23)   # redo at the first loop, not redo, and redo again 
        y += 1
    end 
    assert_equal($g, 30)
    assert_equal(x, 5)
end

test_redo_at_first
test_redo_at_second
test_redo_at_interval