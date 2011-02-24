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

# raise thing [, string [stack trace]]
#
# creates an exception object by invoking the method exception on its first argument
# it then sets this exception's message/backtrace to its second/third arguments
#

require '../../util/assert.rb'

# not anything, but exception class/object(?)
def test_raise_nonsense_object
    assert_raise(TypeError) { raise nil }
    assert_raise(TypeError) { raise 0 }
    assert_raise(TypeError) { raise Object }
end 

# raise exception CLASS without string
def test_raise_exception_class
    $g = 1
    begin 
        raise ArgumentError
    rescue 
        $g += 10
        assert_isinstanceof($!, ArgumentError)
        #assert_equal($!.message, "ArgumentError")  # bug: 293623
    else
        $g += 100
    end
    assert_equal($g, 11)
end

# raise exception CLASS with string
def test_raise_exception_class_with_string
    $g = 1
    begin 
        raise ArgumentError, 'the xth argument is invalid'
    rescue 
        $g += 10
        assert_isinstanceof($!, ArgumentError)
        assert_equal($!.message, 'the xth argument is invalid')
    else
        $g += 100
    end 
    assert_equal($g, 11)
end 

# raise exception OBJECT with string
def test_raise_exception_object_with_string
    begin 
        raise ArgumentError, "try to get the object"
    rescue ArgumentError => argError
    else
        assert_should_have_thrown
    end 

    assert_not_nil(argError)
    
    $g = 1
    begin
        raise argError, "the thing is an exception object"
    rescue
        $g += 10
        assert_isinstanceof($!, ArgumentError)
        assert_equal($!.message, 'the thing is an exception object')  
    else
        $g += 100
    end 
    assert_equal($g, 11)
        
    begin
        raise argError
    rescue 
        assert_equal($!.message, 'try to get the object')  
    end 
end

# raise self-defined exception
class MyError < StandardError
end 
    
def test_raise_my_exception
    $g = 1
    begin 
        raise MyError
    rescue 
        $g += 10
        assert_isinstanceof($!, MyError)
        #assert_equal($!.message, 'MyError')    # bug: 293623
    else
        $g += 100
    end 
    assert_equal($g, 11)

    $g = 1
    begin 
        raise MyError, "special line"
    rescue 
        $g += 10
        assert_isinstanceof($!, MyError)
        assert_equal($!.message, 'special line')
    else
        $g += 100
    end 
    assert_equal($g, 11)
end 

# raise object that has an exception method, 
# 1. which returns not exception
class Foo1
    def exception; "10"; end 
end 

# 2. which returns IOError exception
class Foo2
    def exception; $g += 10; IOError.new; end
end 

# 3. which causes exception itself
class Foo3
    def exception; divide_by_zero; end 
end 

# 4, which asks for message
class Foo4
    def exception m; IOError.new m; end 
end 

def test_raise_object_having_exception_method
    $g = 1
    begin; raise Foo1.new; rescue TypeError; $g += 10; end
    assert_equal($g, 11)
    
    $g = 1
    begin; raise Foo2.new; rescue IOError; $g += 100; end
    assert_equal($g, 111)
    
    $g = 1
    begin; raise Foo3.new; rescue ZeroDivisionError; $g += 10; end 
    assert_equal($g, 11)
    
    $g = 1
    begin; raise Foo2.new, "message"; rescue ArgumentError; $g += 10; end  
    assert_equal($g, 11)

    $g = 1
    begin; raise Foo4.new, "message"; rescue IOError; $g += 10; assert_equal($!.message, "message"); end
    assert_equal($g, 11)

    $g = 1
    begin; raise Foo4.new; rescue ArgumentError; $g += 10; end
    assert_equal($g, 11)
    
    $g = 1
    begin; raise Foo2.new; rescue ($g+=100; Foo2); $g += 1000; rescue IOError; $g+=10000; end
    assert_equal($g, 10111)
    
    $g = 1
    begin; divide_by_zero; rescue Foo2; rescue ZeroDivisionError; end 
    assert_equal($g, 1)
end 

test_raise_nonsense_object
test_raise_exception_class
test_raise_exception_class_with_string
test_raise_exception_object_with_string
test_raise_my_exception
test_raise_object_having_exception_method