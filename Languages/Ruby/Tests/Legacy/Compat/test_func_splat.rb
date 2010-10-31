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


def p *a
  a.each { |x| print x.inspect, '; ' }
  puts
end

def test_0
    def f(a)
        return a
    end
    begin; puts "repro:f(nil)"; p f(nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*nil)"; p f(*nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[nil])"; p f(*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil)"; p f(nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil,nil)"; p f(nil,nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*[nil])"; p f(nil,*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,nil)"; p f(1,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([])"; p f([]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1)"; p f(1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2)"; p f(1,2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,3)"; p f(1,2,3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[])"; p f(1,[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2])"; p f(1,[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2,3])"; p f(1,[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([[]])"; p f([[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1])"; p f([1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2])"; p f([1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,3])"; p f([1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,*[3]])"; p f([1,2,*[3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[])"; p f(*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*1)"; p f(*1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*2)"; p f(1,*2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,*3)"; p f(1,2,*3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[])"; p f(1,*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2])"; p f(1,*[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2,3])"; p f(1,*[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[[]])"; p f(*[[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1])"; p f(*[1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2])"; p f(*[1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2,3])"; p f(*[1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,[2,3]])"; p f(*[1,[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,*[2,3]])"; p f(*[1,*[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*1)"; p f(nil,*1); rescue ArgumentError; puts('except'); end;
end

def test_1
    def f(*a)
        return a
    end
    begin; puts "repro:f(nil)"; p f(nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*nil)"; p f(*nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[nil])"; p f(*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil)"; p f(nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil,nil)"; p f(nil,nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*[nil])"; p f(nil,*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,nil)"; p f(1,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([])"; p f([]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1)"; p f(1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2)"; p f(1,2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,3)"; p f(1,2,3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[])"; p f(1,[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2])"; p f(1,[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2,3])"; p f(1,[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([[]])"; p f([[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1])"; p f([1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2])"; p f([1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,3])"; p f([1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,*[3]])"; p f([1,2,*[3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[])"; p f(*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*1)"; p f(*1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*2)"; p f(1,*2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,*3)"; p f(1,2,*3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[])"; p f(1,*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2])"; p f(1,*[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2,3])"; p f(1,*[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[[]])"; p f(*[[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1])"; p f(*[1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2])"; p f(*[1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2,3])"; p f(*[1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,[2,3]])"; p f(*[1,[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,*[2,3]])"; p f(*[1,*[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*1)"; p f(nil,*1); rescue ArgumentError; puts('except'); end;
end

def test_2
    def f(a,b)
        return a,b
    end
    begin; puts "repro:f(nil)"; p f(nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*nil)"; p f(*nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[nil])"; p f(*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil)"; p f(nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil,nil)"; p f(nil,nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*[nil])"; p f(nil,*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,nil)"; p f(1,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([])"; p f([]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1)"; p f(1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2)"; p f(1,2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,3)"; p f(1,2,3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[])"; p f(1,[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2])"; p f(1,[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2,3])"; p f(1,[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([[]])"; p f([[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1])"; p f([1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2])"; p f([1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,3])"; p f([1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,*[3]])"; p f([1,2,*[3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[])"; p f(*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*1)"; p f(*1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*2)"; p f(1,*2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,*3)"; p f(1,2,*3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[])"; p f(1,*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2])"; p f(1,*[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2,3])"; p f(1,*[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[[]])"; p f(*[[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1])"; p f(*[1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2])"; p f(*[1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2,3])"; p f(*[1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,[2,3]])"; p f(*[1,[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,*[2,3]])"; p f(*[1,*[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*1)"; p f(nil,*1); rescue ArgumentError; puts('except'); end;
end

def test_3
    def f(a,*b)
        return a,b
    end
    begin; puts "repro:f(nil)"; p f(nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*nil)"; p f(*nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[nil])"; p f(*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil)"; p f(nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil,nil)"; p f(nil,nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*[nil])"; p f(nil,*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,nil)"; p f(1,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([])"; p f([]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1)"; p f(1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2)"; p f(1,2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,3)"; p f(1,2,3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[])"; p f(1,[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2])"; p f(1,[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2,3])"; p f(1,[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([[]])"; p f([[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1])"; p f([1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2])"; p f([1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,3])"; p f([1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,*[3]])"; p f([1,2,*[3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[])"; p f(*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*1)"; p f(*1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*2)"; p f(1,*2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,*3)"; p f(1,2,*3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[])"; p f(1,*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2])"; p f(1,*[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2,3])"; p f(1,*[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[[]])"; p f(*[[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1])"; p f(*[1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2])"; p f(*[1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2,3])"; p f(*[1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,[2,3]])"; p f(*[1,[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,*[2,3]])"; p f(*[1,*[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*1)"; p f(nil,*1); rescue ArgumentError; puts('except'); end;
end

def test_4
    def f(a,b,c)
        return a,b,c
    end
    begin; puts "repro:f(nil)"; p f(nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*nil)"; p f(*nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[nil])"; p f(*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil)"; p f(nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil,nil)"; p f(nil,nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*[nil])"; p f(nil,*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,nil)"; p f(1,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([])"; p f([]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1)"; p f(1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2)"; p f(1,2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,3)"; p f(1,2,3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[])"; p f(1,[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2])"; p f(1,[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2,3])"; p f(1,[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([[]])"; p f([[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1])"; p f([1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2])"; p f([1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,3])"; p f([1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,*[3]])"; p f([1,2,*[3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[])"; p f(*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*1)"; p f(*1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*2)"; p f(1,*2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,*3)"; p f(1,2,*3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[])"; p f(1,*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2])"; p f(1,*[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2,3])"; p f(1,*[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[[]])"; p f(*[[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1])"; p f(*[1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2])"; p f(*[1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2,3])"; p f(*[1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,[2,3]])"; p f(*[1,[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,*[2,3]])"; p f(*[1,*[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*1)"; p f(nil,*1); rescue ArgumentError; puts('except'); end;
end

def test_5
    def f(a,b,*c)
        return a,b,c
    end
    begin; puts "repro:f(nil)"; p f(nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*nil)"; p f(*nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[nil])"; p f(*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil)"; p f(nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,nil,nil)"; p f(nil,nil,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*[nil])"; p f(nil,*[nil]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,nil)"; p f(1,nil); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([])"; p f([]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1)"; p f(1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2)"; p f(1,2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,3)"; p f(1,2,3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[])"; p f(1,[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2])"; p f(1,[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,[2,3])"; p f(1,[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([[]])"; p f([[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1])"; p f([1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2])"; p f([1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,3])"; p f([1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f([1,2,*[3]])"; p f([1,2,*[3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[])"; p f(*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*1)"; p f(*1); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*2)"; p f(1,*2); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,2,*3)"; p f(1,2,*3); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[])"; p f(1,*[]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2])"; p f(1,*[2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(1,*[2,3])"; p f(1,*[2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[[]])"; p f(*[[]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1])"; p f(*[1]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2])"; p f(*[1,2]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,2,3])"; p f(*[1,2,3]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,[2,3]])"; p f(*[1,[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(*[1,*[2,3]])"; p f(*[1,*[2,3]]); rescue ArgumentError; puts('except'); end;
    begin; puts "repro:f(nil,*1)"; p f(nil,*1); rescue ArgumentError; puts('except'); end;
end

test_0
test_1
test_2
test_3
test_4
test_5
