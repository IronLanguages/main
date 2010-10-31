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

def test_empty_if
    def f(v)
        if v;
        end 
    end 
    
    assert_equal(f(false), nil); 
    assert_equal(f(true), nil); 
end 

def test_only_one_if
    def f(v)
        if v; 10; end
    end 
    
    assert_equal(f(false), nil); 
    assert_equal(f(true), 10); 
end 

def test_parallel_ifs    
    def f(x, y)
        if x; $g+=10; 7; end 
        if y; $g+=100; 8; end
    end 
    
    $g = 1;     assert_equal(f(false, false), nil); assert_equal($g, 1)
    $g = 1;     assert_equal(f(true, false), nil);  assert_equal($g, 11)
    $g = 1;     assert_equal(f(false, true), 8);    assert_equal($g, 101)
    $g = 1;     assert_equal(f(true, true), 8);     assert_equal($g, 111)
end 

def test_nested_ifs    
    def f(x, y)
        if x;
            $g+= 10; 14
            if y:
                $g += 100; 15
            end 
        end
    end
    
    $g = 1;     assert_equal(f(false, false), nil); assert_equal($g, 1)
    $g = 1;     assert_equal(f(true, false), nil);  assert_equal($g, 11)
    $g = 1;     assert_equal(f(false, true), nil);  assert_equal($g, 1)
    $g = 1;     assert_equal(f(true, true), 15);    assert_equal($g, 111)
    
    def f(x, y, z, w)
        if x;
            $g += 10 
            if y;  $g += 100; end
            if z;
                $g += 1000
                if w;   $g += 10000; end 
                $g += 1000
            end
            $g += 10
        end
    end
    
    $g = 1; f(false, false, true, true);    assert_equal($g, 1)
    $g = 1; f(true, false, false, false);   assert_equal($g, 21)
    $g = 1; f(true, false, false, true);   assert_equal($g, 21)
    $g = 1; f(true, false, true, false);   assert_equal($g, 2021)
    $g = 1; f(true, false, true, true);   assert_equal($g, 12021)
    $g = 1; f(true, true, false, false);   assert_equal($g, 121)
    $g = 1; f(true, true, false, true);   assert_equal($g, 121)
    $g = 1; f(true, true, true, false);   assert_equal($g, 2121)
    $g = 1; f(true, true, true, true);   assert_equal($g, 12121)
end 

def test_empty_else
    def f(x)
        if x; 23 
        else
        end 
    end 

    assert_equal(f(true), 23)
    assert_equal(f(false), nil)
end 

def test_if_else_only
    def f(x)
        if x;   
            27
        else;   
            28
        end 
    end 
    
    assert_equal(f(true), 27)
    assert_equal(f(false), 28)
end 

def test_one_elsif
    def f(x, y)
        if x
            33
        elsif y
            34
        end 
    end 
    
    assert_equal(f(false, false), nil)
    assert_equal(f(false, true), 34)
    assert_equal(f(true, false), 33)
    assert_equal(f(true, true), 33)
end 

def test_two_elsif
    def f(x, y, z)
        if x; 41
        elsif y; 42
        elsif z; 43
        end 
    end 
    
    assert_equal(f(false, false, false), nil)
    assert_equal(f(false, false, true), 43)
    assert_equal(f(false, true, false), 42)
    assert_equal(f(false, true, true), 42)
    assert_equal(f(true, false, false), 41)
    assert_equal(f(true, false, true), 41)
    assert_equal(f(true, true, false), 41)
    assert_equal(f(true, true, true), 41)
end 

def test_elsif_with_else
    def f(x, y)
        if x; 55
        elsif y; 56
        else; 57
        end 
    end 
    assert_equal(f(false, false), 57)
    assert_equal(f(false, true), 56)
    assert_equal(f(true, false), 55)
    assert_equal(f(true, true), 55)
end 

def test_exception
    def f
        if ($g+=10; divide_by_zero; $g+=100)
            $g += 1000
        else 
            $g += 10000
        end 
    end 
    $g = 1; begin; f; rescue; $g += 100000; end; assert_equal($g, 100011)
    
    def f(x)
        if x
            $g += 10; divide_by_zero; $g+=100
        else 
            $g += 1000; divide_by_zero; $g+=10000
        end 
    end 
    $g = 1; begin; f true; rescue; $g += 100000; end; assert_equal($g, 100011)
    $g = 1; begin; f false; rescue; $g += 100000; end; assert_equal($g, 101001)
end 

test_empty_if
test_only_one_if
test_parallel_ifs
test_nested_ifs
test_empty_else
test_if_else_only
test_one_elsif
test_two_elsif
test_elsif_with_else
test_exception