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

# lifetime of $!

assert_nil($!)

# exception raised in try block and get rescued
def test_raised_and_rescued
    def get_rescued
        begin 
            raise TypeError
        rescue 
            assert_isinstanceof($!, TypeError)
        ensure 
            assert_nil($!)
        end
    end 
    
    get_rescued
    assert_nil($!)
end 

# exception raised but not gets rescued
def test_raised_not_rescued
    def not_rescued
        begin
            raise ArgumentError
        ensure
            assert_isinstanceof($!, ArgumentError)
        end
    end 

    begin 
        not_rescued
    rescue 
        assert_isinstanceof($!, ArgumentError)
    ensure 
        assert_nil($!)
    end 
    
    assert_nil($!)
end 


# exception raised in else block
def test_raised_in_else_block
    def raised_in_else
        begin 
            empty_func
        rescue
        else
            raise NameError
        ensure 
            assert_isinstanceof($!, NameError)
        end 
    end 
    
    begin
        raised_in_else
    rescue 
        assert_isinstanceof($!, NameError)
    ensure 
        assert_nil($!)
    end 
end 

# several exceptions thrown in a try-block-chain
def test_multiple_exception_in_try_blocks 
    begin 
        begin 
            raise ArgumentError
        rescue
            assert_isinstanceof($!, ArgumentError)
        end 
        assert_nil($!)
        raise TypeError
    rescue 
        assert_isinstanceof($!, TypeError)
    end
    assert_nil($!) 
end 

# several exceptions thrown in a rescue chain
# $! works like a stack
def test_multiple_exception_in_rescue_blocks
    assert_nil($!)
    begin 
        raise ArgumentError
    rescue
        assert_isinstanceof($!, ArgumentError)
        begin 
            assert_isinstanceof($!, ArgumentError)
            raise TypeError
        rescue 
            assert_isinstanceof($!, TypeError)
            begin
                assert_isinstanceof($!, TypeError)
                raise NameError
            rescue
                assert_isinstanceof($!, NameError)
            end
            assert_isinstanceof($!, TypeError) 
        end 
        assert_isinstanceof($!, ArgumentError)
    end
    assert_nil($!)
end 

# $! is set manually, it should be kept even after exception is thrown by the runtime.
def test_manually_set
    $! = IOError.new
    begin 
        assert_isinstanceof($!, IOError)
        raise TypeError
    rescue
        assert_isinstanceof($!, TypeError)
    end 
    assert_isinstanceof($!, IOError)
    $! = nil  # restore the "peaceful" wolrd
end 

# assign the exception to local variable.
def test_saved_to_local_variable
    begin 
        raise "hello"
    rescue RuntimeError => local_var
        assert_isinstanceof($!, RuntimeError)
        assert_isinstanceof(local_var, RuntimeError)
    end 
    assert_nil($!)
    assert_isinstanceof(local_var, RuntimeError)
end 

def test_life_time_of_manually_created
    def no_raise
        $! = IOError.new
        $g = 1
        begin 
            empty_func
        rescue 
            $g += 10
        else
            $g += 100
            assert_isinstanceof($!, IOError)
        ensure 
            $g += 1000
            assert_isinstanceof($!, IOError)
        end 
        assert_isinstanceof($!, IOError)
    end 
    
    begin 
        no_raise
        $g += 10000
        assert_isinstanceof($!, IOError)
    rescue 
        $g += 100000
    ensure
        $g += 1000000
        assert_nil($!)     
    end
    assert_nil($!)         
    
    assert_equal($g, 1011101)
end 

test_raised_and_rescued
test_raised_not_rescued
test_raised_in_else_block
test_multiple_exception_in_try_blocks
test_multiple_exception_in_rescue_blocks
test_manually_set
test_saved_to_local_variable

test_life_time_of_manually_created