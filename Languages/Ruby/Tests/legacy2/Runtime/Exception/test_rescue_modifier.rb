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

# rescue StandardError and its children

# no need to rescue
def test_never_rescued
    $g = 1
    empty_func rescue $g+=10 rescue $g+=100
    assert_equal($g, 1)
end 

# need rescue, succeed
def test_rescue_succeed
    $g = 1
    divide_by_zero rescue $g+=10
    assert_equal($g, 11)
end 

# need rescue, failed: LoadError is not inherited from StandardError
def test_rescue_not_succed
    $g = 1
    def f
        require "not_existing" rescue $g+=10
    end 
    begin
        f 
    rescue LoadError
        $g+= 100    
    end    
    assert_equal($g, 101)
end 

# multiple rescue modifer
def test_cancat_rescue
    $g = 1
    divide_by_zero rescue $g+=10 rescue $g+=100
    assert_equal($g, 11)

    $g = 1
    divide_by_zero rescue $g+=10; x rescue $g+=100  # x is defined nowhere, NameError is expected.
    assert_equal($g, 111)
end 

test_never_rescued
test_rescue_succeed
test_rescue_not_succed
test_cancat_rescue