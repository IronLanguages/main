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

# tests failed from the long run exception tests.

def f_2
    begin
        1/0
    rescue (raise IOError); 
    else; 
    ensure; 
        puts 1; puts $!.class; raise SyntaxError.new
    end
    puts $!.class
end
$g = 0; begin; print(f_2); rescue Exception; puts 2; print $!.class; end; puts " : f_2"

def f_15100
    begin
        raise IOError
    rescue TypeError; 
    ensure; 
        puts $!.class;
    end
end
$g = 0; begin; print(f_15100); rescue Exception; print $!.class; end; puts " : f_15100"

def f_16
    begin
        raise Exception
    rescue ($!=StandardError.new;IOError)
        puts 1, $!.class
    end
    puts 2, $!.class
end
$g = 0; begin; print(f_16); rescue Exception; puts 3, $!.class; end; puts " : f_16"

def f_132
    print 1, $!.class
    begin
        #empty_stmt
    rescue (print 2, $!.class; raise TypeError; IOError)
        puts $!.class
    else
        print 3, $!.class
        raise "4"
        print 5, $!.class
    ensure
        print 6, $!.class
        #empty_stmt
        print 7, $!.class
    end
    print 8, $!.class
end
$g = 0; begin; print(f_132); rescue Exception; print $!.class; end; puts " : f_132"

def f1
    begin
    else
        $! = TypeError.new
    ensure; 
        puts 3, $!.class
        $! = IOError.new
        puts 5, $!.class 
    end
    puts 4, $!.class
end

f1

