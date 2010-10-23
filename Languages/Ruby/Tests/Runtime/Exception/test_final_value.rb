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

def test_begin_ensure_and_nothing_in_the_body
    x = begin 
        # nothing here        
    ensure; 10; end 
    assert_equal(x, nil)
    -1
end 

def test_begin_ensure_and_number_in_the_body
    x = begin 
        20
    ensure; 30; end 
    
    assert_equal(x, 20)
    -1
end 

def test_begin_ensure_and_return_in_the_body
    begin 
        return 40
        50
    ensure; 60; end 
    -1
end 

def test_begin_ensure_and_return_in_the_ensure
    begin; 70; ensure; return 80; 90; end 
    -1
end 

def test_begin_ensure_and_return_in_both
    begin; return 100; ensure; return 110; 120; end 
    -1
end 

def test_begin_rescue_not_throw
    x = begin; 10; rescue; 20; end 
    assert_equal(x, 10)
    -1
end 

def test_begin_rescue_throw_but_return_nothing_in_rescue
    x = begin; divide_by_zero; 30; rescue; end 
    assert_equal(x, nil)
    -1
end 

def test_begin_rescue_throw
    x = begin; divide_by_zero; 30; rescue; 40; end 
    assert_equal(x, 40)
    -1
end 

# !!!
def test_begin_rescue_else
    x = begin; 10; rescue; 20; else; 30; end 
    assert_equal(x, 30)
    -1
end 

def test_begin_rescue_else_and_return_in_both
    begin; return 40; rescue; 50; else; return 60; end 
    -1
end 

def test_throw
    begin
        x = begin; divide_by_zero; ensure; 20; end 
    rescue 
    end 
    assert_equal(x, nil)
    -1
end 

def test_throw_with_return_inside_ensure
    begin
        x = begin; divide_by_zero; ensure; return 20; end 
    rescue 
    end 
    -1
end 

assert_equal(test_begin_ensure_and_nothing_in_the_body, -1)
assert_equal(test_begin_ensure_and_number_in_the_body, -1)
assert_equal(test_begin_ensure_and_return_in_the_body, 40)
assert_equal(test_begin_ensure_and_return_in_the_ensure, 80)
assert_equal(test_begin_ensure_and_return_in_both, 110)
 
assert_equal(test_begin_rescue_not_throw, -1)
assert_equal(test_begin_rescue_throw_but_return_nothing_in_rescue, -1)
assert_equal(test_begin_rescue_throw, -1)
assert_equal(test_begin_rescue_else, -1)
assert_equal(test_begin_rescue_else_and_return_in_both, 40)
# bug: 269526
#assert_equal(test_throw, -1)
assert_equal(test_throw_with_return_inside_ensure, 20)
