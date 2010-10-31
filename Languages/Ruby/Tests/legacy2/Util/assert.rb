def log_error(message)
    puts message
    raise
end 

def assert(boolean, message)
    if boolean != true
        log_error(message)
    end 
end

def assert_unreachable()
    assert(false, "should not be reachable")
end 

def assert_should_have_thrown(expected_exception)
    assert(false, "should have thrown: " + expected_exception.to_s)
end 

def assert_equal(left, right, message="left: #{left}, right: #{right}")
    assert(left == right, message)
end 

def assert_pair_equal *args
    arg_count = args.length 
    log_error("Odd argument count : {#arg_count}") unless arg_count % 2 == 0
    0.step arg_count, 2 do |i|
        assert_equal(args[i], args[i+1], message="pair #{i} not equal")
    end 
end

def assert_nil(obj, message=nil)
    assert_equal(obj, nil, message)
end 

def assert_not_nil(obj, message = "should be not nil")
    assert(obj != nil, message)
end 

def assert_isinstanceof(obj, kclass, message=nil)
    assert_equal(obj.class, kclass, message)
end 

def assert_raise(expected_exception, expected_message=nil)
    begin
        yield 
    rescue Exception => actual_exception_obj
        assert_isinstanceof(
                 actual_exception_obj, expected_exception, 
                "expect #{expected_exception}, but get #{actual_exception_obj.class}"
                )
        if expected_message
        #    assert_equal(actual_exception_obj.message, expected_message)
        end 
    else
        assert_should_have_thrown(expected_exception)
    end 
end

def assert_return(expected) 
    actual = yield
    assert_equal(expected, actual)
end 

def assert_true(&b)
    assert_return(true, &b) 
end 

def assert_false(&b)
    assert_return(false, &b) 
end 

def divide_by_zero
    1/0
end 

def empty_func
end 

def runtest(m, skiplist=[])
    m.private_methods.each do |m|
        if m[0,5] == "test_" 
            puts "testing #{m}... "
            if skiplist.include? m
                puts "skipped"
            else 
                m.method(m).call 
            end
        end 
    end
end 