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

# variables defined in each block, whether that block gets executed

def test_begin_ensure
    begin 
        local_begin = 1
    ensure
        local_ensure = 2
    end 
    
    assert_equal(local_begin, 1)
    assert_equal(local_ensure, 2)
end 

def test_begin_rescue_else
    begin 
        local_begin = 1
    rescue 
        local_rescue = 2
    else 
        local_else = 3
    end 

    assert_equal(local_begin, 1)
    assert_equal(local_else, 3)
    #assert_nil(local_rescue)  # bug: 269526
end 

def test_rescue_not_raised 
    begin 
        local_begin = 1
    rescue (local_rescue = 2; RuntimeError)
        local_rescue2 = 3
    end 

    assert_equal(local_begin, 1)
#    assert_nil(local_rescue)    # bug: 269526
#    assert_nil(local_rescue2)   # bug: 269526
end 

def test_rescue_raised 
    begin 
        1/0
    rescue IOError => local_ioerror
    rescue StandardError => local_stderror
    rescue (local_rescue = 2; RuntimeError) 
        local_rescue2 = 3
    end 
    
#    assert_nil(local_ioerror)      # bug: 269526
    assert_not_nil(local_stderror)
#    assert_nil(local_rescue)       # bug: 269526
#    assert_nil(local_rescue2)      # bug: 269526
end 

test_begin_ensure
test_begin_rescue_else
test_rescue_not_raised
test_rescue_raised