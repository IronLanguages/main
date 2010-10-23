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

# exception does not raise, the "else"-block gets hit
def test_exception_not_raised
    $g = 1
    begin
        empty_func
    rescue RuntimeError
        $g += 10
    else
        $g += 100
    end
    assert_equal($g, 101)
end

# exception does raise, rescue properly, the "else"-block not be hit
def test_exception_raised_and_handled
    $g = 1
    begin
        divide_by_zero
    rescue ZeroDivisionError
        $g += 10
    else
        $g += 100
    end
    assert_equal($g, 11)
end 

# exception does raise, does not rescue properly, the "else"-block not be hit either
def test_exception_raised_but_not_handled
    $g = 1
    def f
        begin 
            divide_by_zero
        rescue RuntimeError
            $g += 10
        else
            $g += 100
        end
    end 

    begin 
        f
    rescue ZeroDivisionError
        $g += 1000
    end 
    assert_equal($g, 1001)
end 

test_exception_not_raised 
test_exception_raised_and_handled
test_exception_raised_but_not_handled

#runtest(self)