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

# reraise the exception in $! or a new RuntimeError if $! is nil

require '../../util/assert.rb'
#
assert_equal($!, nil)

# raise with really nothing
def test_raise_nothing
    $g = 1
    begin 
        raise
    rescue Exception
        $g += 10
        assert_isinstanceof($!, RuntimeError)
    else
        $g += 100
    end 
    assert_equal($g, 11)
end 

# reraise 

def test_reraise
    def bad_thing
        1 / 0
    rescue 
        raise 
    end 

    $g = 1
    begin 
        bad_thing
    rescue
        $g += 10 
        assert_isinstanceof($!, ZeroDivisionError)
    else
        $g += 100
    end
    assert_equal($g, 11)
end

test_raise_nothing
test_reraise