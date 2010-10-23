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

def f0()
    a = 0; puts 'repro: a = nil'; a = nil; p a
    a = 0; puts 'repro: a = []'; a = []; p a
    a = 0; puts 'repro: a = [nil]'; a = [nil]; p a
    a = 0; puts 'repro: a = 1'; a = 1; p a
    a = 0; puts 'repro: a = 1,2'; a = 1,2; p a
    a = 0; puts 'repro: a = 1,2,3'; a = 1,2,3; p a
    a = 0; puts 'repro: a = [1]'; a = [1]; p a
    a = 0; puts 'repro: a = [1,2]'; a = [1,2]; p a
    a = 0; puts 'repro: a = [1,2,3]'; a = [1,2,3]; p a
    a = 0; puts 'repro: a = *1'; a = *1; p a
    a = 0; puts 'repro: a = *nil'; a = *nil; p a
    a = 0; puts 'repro: a = *[]'; a = *[]; p a
    a = 0; puts 'repro: a = *[nil]'; a = *[nil]; p a
    a = 0; puts 'repro: a = *[1]'; a = *[1]; p a
    a = 0; puts 'repro: a = *[1,2]'; a = *[1,2]; p a
    a = 0; puts 'repro: a = 1,*[2,3]'; a = 1,*[2,3]; p a
    a = 0; puts 'repro: a = *[[]]'; a = *[[]]; p a
    a = 0; puts 'repro: a = 1,*[]'; a = 1,*[]; p a
    a = 0; puts 'repro: a = [1,2],3'; a = [1,2],3; p a
    a = 0; puts 'repro: a = nil,1'; a = nil,1; p a
end
def f1()
    a,b = 0; puts 'repro: a,b = nil'; a,b = nil; p a,b
    a,b = 0; puts 'repro: a,b = []'; a,b = []; p a,b
    a,b = 0; puts 'repro: a,b = [nil]'; a,b = [nil]; p a,b
    a,b = 0; puts 'repro: a,b = 1'; a,b = 1; p a,b
    a,b = 0; puts 'repro: a,b = 1,2'; a,b = 1,2; p a,b
    a,b = 0; puts 'repro: a,b = 1,2,3'; a,b = 1,2,3; p a,b
    a,b = 0; puts 'repro: a,b = [1]'; a,b = [1]; p a,b
    a,b = 0; puts 'repro: a,b = [1,2]'; a,b = [1,2]; p a,b
    a,b = 0; puts 'repro: a,b = [1,2,3]'; a,b = [1,2,3]; p a,b
    a,b = 0; puts 'repro: a,b = *1'; a,b = *1; p a,b
    a,b = 0; puts 'repro: a,b = *nil'; a,b = *nil; p a,b
    a,b = 0; puts 'repro: a,b = *[]'; a,b = *[]; p a,b
    a,b = 0; puts 'repro: a,b = *[nil]'; a,b = *[nil]; p a,b
    a,b = 0; puts 'repro: a,b = *[1]'; a,b = *[1]; p a,b
    a,b = 0; puts 'repro: a,b = *[1,2]'; a,b = *[1,2]; p a,b
    a,b = 0; puts 'repro: a,b = 1,*[2,3]'; a,b = 1,*[2,3]; p a,b
    a,b = 0; puts 'repro: a,b = *[[]]'; a,b = *[[]]; p a,b
    a,b = 0; puts 'repro: a,b = 1,*[]'; a,b = 1,*[]; p a,b
    a,b = 0; puts 'repro: a,b = [1,2],3'; a,b = [1,2],3; p a,b
    a,b = 0; puts 'repro: a,b = nil,1'; a,b = nil,1; p a,b
end
def f2()
    a = 0; puts 'repro: *a = nil'; *a = nil; p a
    a = 0; puts 'repro: *a = []'; *a = []; p a
    a = 0; puts 'repro: *a = [nil]'; *a = [nil]; p a
    a = 0; puts 'repro: *a = 1'; *a = 1; p a
    a = 0; puts 'repro: *a = 1,2'; *a = 1,2; p a
    a = 0; puts 'repro: *a = 1,2,3'; *a = 1,2,3; p a
    a = 0; puts 'repro: *a = [1]'; *a = [1]; p a
    a = 0; puts 'repro: *a = [1,2]'; *a = [1,2]; p a
    a = 0; puts 'repro: *a = [1,2,3]'; *a = [1,2,3]; p a
    a = 0; puts 'repro: *a = *1'; *a = *1; p a
    a = 0; puts 'repro: *a = *nil'; *a = *nil; p a
    a = 0; puts 'repro: *a = *[]'; *a = *[]; p a
    a = 0; puts 'repro: *a = *[nil]'; *a = *[nil]; p a
    a = 0; puts 'repro: *a = *[1]'; *a = *[1]; p a
    a = 0; puts 'repro: *a = *[1,2]'; *a = *[1,2]; p a
    a = 0; puts 'repro: *a = 1,*[2,3]'; *a = 1,*[2,3]; p a
    a = 0; puts 'repro: *a = *[[]]'; *a = *[[]]; p a
    a = 0; puts 'repro: *a = 1,*[]'; *a = 1,*[]; p a
    a = 0; puts 'repro: *a = [1,2],3'; *a = [1,2],3; p a
    a = 0; puts 'repro: *a = nil,1'; *a = nil,1; p a
end
def f3()
    a,b = 0; puts 'repro: a,*b = nil'; a,*b = nil; p a,b
    a,b = 0; puts 'repro: a,*b = []'; a,*b = []; p a,b
    a,b = 0; puts 'repro: a,*b = [nil]'; a,*b = [nil]; p a,b
    a,b = 0; puts 'repro: a,*b = 1'; a,*b = 1; p a,b
    a,b = 0; puts 'repro: a,*b = 1,2'; a,*b = 1,2; p a,b
    a,b = 0; puts 'repro: a,*b = 1,2,3'; a,*b = 1,2,3; p a,b
    a,b = 0; puts 'repro: a,*b = [1]'; a,*b = [1]; p a,b
    a,b = 0; puts 'repro: a,*b = [1,2]'; a,*b = [1,2]; p a,b
    a,b = 0; puts 'repro: a,*b = [1,2,3]'; a,*b = [1,2,3]; p a,b
    a,b = 0; puts 'repro: a,*b = *1'; a,*b = *1; p a,b
    a,b = 0; puts 'repro: a,*b = *nil'; a,*b = *nil; p a,b
    a,b = 0; puts 'repro: a,*b = *[]'; a,*b = *[]; p a,b
    a,b = 0; puts 'repro: a,*b = *[nil]'; a,*b = *[nil]; p a,b
    a,b = 0; puts 'repro: a,*b = *[1]'; a,*b = *[1]; p a,b
    a,b = 0; puts 'repro: a,*b = *[1,2]'; a,*b = *[1,2]; p a,b
    a,b = 0; puts 'repro: a,*b = 1,*[2,3]'; a,*b = 1,*[2,3]; p a,b
    a,b = 0; puts 'repro: a,*b = *[[]]'; a,*b = *[[]]; p a,b
    a,b = 0; puts 'repro: a,*b = 1,*[]'; a,*b = 1,*[]; p a,b
    a,b = 0; puts 'repro: a,*b = [1,2],3'; a,*b = [1,2],3; p a,b
    a,b = 0; puts 'repro: a,*b = nil,1'; a,*b = nil,1; p a,b
end
def f4()
    a = 0; puts 'repro: (a,) = nil'; (a,) = nil; p a
    a = 0; puts 'repro: (a,) = []'; (a,) = []; p a
    a = 0; puts 'repro: (a,) = [nil]'; (a,) = [nil]; p a
    a = 0; puts 'repro: (a,) = 1'; (a,) = 1; p a
    a = 0; puts 'repro: (a,) = 1,2'; (a,) = 1,2; p a
    a = 0; puts 'repro: (a,) = 1,2,3'; (a,) = 1,2,3; p a
    a = 0; puts 'repro: (a,) = [1]'; (a,) = [1]; p a
    a = 0; puts 'repro: (a,) = [1,2]'; (a,) = [1,2]; p a
    a = 0; puts 'repro: (a,) = [1,2,3]'; (a,) = [1,2,3]; p a
    a = 0; puts 'repro: (a,) = *1'; (a,) = *1; p a
    a = 0; puts 'repro: (a,) = *nil'; (a,) = *nil; p a
    a = 0; puts 'repro: (a,) = *[]'; (a,) = *[]; p a
    a = 0; puts 'repro: (a,) = *[nil]'; (a,) = *[nil]; p a
    a = 0; puts 'repro: (a,) = *[1]'; (a,) = *[1]; p a
    a = 0; puts 'repro: (a,) = *[1,2]'; (a,) = *[1,2]; p a
    a = 0; puts 'repro: (a,) = 1,*[2,3]'; (a,) = 1,*[2,3]; p a
    a = 0; puts 'repro: (a,) = *[[]]'; (a,) = *[[]]; p a
    a = 0; puts 'repro: (a,) = 1,*[]'; (a,) = 1,*[]; p a
    a = 0; puts 'repro: (a,) = [1,2],3'; (a,) = [1,2],3; p a
    a = 0; puts 'repro: (a,) = nil,1'; (a,) = nil,1; p a
end
def f5()
    a,b = 0; puts 'repro: (a,b) = nil'; (a,b) = nil; p a,b
    a,b = 0; puts 'repro: (a,b) = []'; (a,b) = []; p a,b
    a,b = 0; puts 'repro: (a,b) = [nil]'; (a,b) = [nil]; p a,b
    a,b = 0; puts 'repro: (a,b) = 1'; (a,b) = 1; p a,b
    a,b = 0; puts 'repro: (a,b) = 1,2'; (a,b) = 1,2; p a,b
    a,b = 0; puts 'repro: (a,b) = 1,2,3'; (a,b) = 1,2,3; p a,b
    a,b = 0; puts 'repro: (a,b) = [1]'; (a,b) = [1]; p a,b
    a,b = 0; puts 'repro: (a,b) = [1,2]'; (a,b) = [1,2]; p a,b
    a,b = 0; puts 'repro: (a,b) = [1,2,3]'; (a,b) = [1,2,3]; p a,b
    a,b = 0; puts 'repro: (a,b) = *1'; (a,b) = *1; p a,b
    a,b = 0; puts 'repro: (a,b) = *nil'; (a,b) = *nil; p a,b
    a,b = 0; puts 'repro: (a,b) = *[]'; (a,b) = *[]; p a,b
    a,b = 0; puts 'repro: (a,b) = *[nil]'; (a,b) = *[nil]; p a,b
    a,b = 0; puts 'repro: (a,b) = *[1]'; (a,b) = *[1]; p a,b
    a,b = 0; puts 'repro: (a,b) = *[1,2]'; (a,b) = *[1,2]; p a,b
    a,b = 0; puts 'repro: (a,b) = 1,*[2,3]'; (a,b) = 1,*[2,3]; p a,b
    a,b = 0; puts 'repro: (a,b) = *[[]]'; (a,b) = *[[]]; p a,b
    a,b = 0; puts 'repro: (a,b) = 1,*[]'; (a,b) = 1,*[]; p a,b
    a,b = 0; puts 'repro: (a,b) = [1,2],3'; (a,b) = [1,2],3; p a,b
    a,b = 0; puts 'repro: (a,b) = nil,1'; (a,b) = nil,1; p a,b
end
def f6()
    a,b = 0; puts 'repro: (a,*b) = nil'; (a,*b) = nil; p a,b
    a,b = 0; puts 'repro: (a,*b) = []'; (a,*b) = []; p a,b
    a,b = 0; puts 'repro: (a,*b) = [nil]'; (a,*b) = [nil]; p a,b
    a,b = 0; puts 'repro: (a,*b) = 1'; (a,*b) = 1; p a,b
    a,b = 0; puts 'repro: (a,*b) = 1,2'; (a,*b) = 1,2; p a,b
    a,b = 0; puts 'repro: (a,*b) = 1,2,3'; (a,*b) = 1,2,3; p a,b
    a,b = 0; puts 'repro: (a,*b) = [1]'; (a,*b) = [1]; p a,b
    a,b = 0; puts 'repro: (a,*b) = [1,2]'; (a,*b) = [1,2]; p a,b
    a,b = 0; puts 'repro: (a,*b) = [1,2,3]'; (a,*b) = [1,2,3]; p a,b
    a,b = 0; puts 'repro: (a,*b) = *1'; (a,*b) = *1; p a,b
    a,b = 0; puts 'repro: (a,*b) = *nil'; (a,*b) = *nil; p a,b
    a,b = 0; puts 'repro: (a,*b) = *[]'; (a,*b) = *[]; p a,b
    a,b = 0; puts 'repro: (a,*b) = *[nil]'; (a,*b) = *[nil]; p a,b
    a,b = 0; puts 'repro: (a,*b) = *[1]'; (a,*b) = *[1]; p a,b
    a,b = 0; puts 'repro: (a,*b) = *[1,2]'; (a,*b) = *[1,2]; p a,b
    a,b = 0; puts 'repro: (a,*b) = 1,*[2,3]'; (a,*b) = 1,*[2,3]; p a,b
    a,b = 0; puts 'repro: (a,*b) = *[[]]'; (a,*b) = *[[]]; p a,b
    a,b = 0; puts 'repro: (a,*b) = 1,*[]'; (a,*b) = 1,*[]; p a,b
    a,b = 0; puts 'repro: (a,*b) = [1,2],3'; (a,*b) = [1,2],3; p a,b
    a,b = 0; puts 'repro: (a,*b) = nil,1'; (a,*b) = nil,1; p a,b
end
def f7()
    a = 0; puts 'repro: (*a) = nil'; (*a) = nil; p a
    a = 0; puts 'repro: (*a) = []'; (*a) = []; p a
    a = 0; puts 'repro: (*a) = [nil]'; (*a) = [nil]; p a
    a = 0; puts 'repro: (*a) = 1'; (*a) = 1; p a
    a = 0; puts 'repro: (*a) = 1,2'; (*a) = 1,2; p a
    a = 0; puts 'repro: (*a) = 1,2,3'; (*a) = 1,2,3; p a
    a = 0; puts 'repro: (*a) = [1]'; (*a) = [1]; p a
    a = 0; puts 'repro: (*a) = [1,2]'; (*a) = [1,2]; p a
    a = 0; puts 'repro: (*a) = [1,2,3]'; (*a) = [1,2,3]; p a
    a = 0; puts 'repro: (*a) = *1'; (*a) = *1; p a
    a = 0; puts 'repro: (*a) = *nil'; (*a) = *nil; p a
    a = 0; puts 'repro: (*a) = *[]'; (*a) = *[]; p a
    a = 0; puts 'repro: (*a) = *[nil]'; (*a) = *[nil]; p a
    a = 0; puts 'repro: (*a) = *[1]'; (*a) = *[1]; p a
    a = 0; puts 'repro: (*a) = *[1,2]'; (*a) = *[1,2]; p a
    a = 0; puts 'repro: (*a) = 1,*[2,3]'; (*a) = 1,*[2,3]; p a
    a = 0; puts 'repro: (*a) = *[[]]'; (*a) = *[[]]; p a
    a = 0; puts 'repro: (*a) = 1,*[]'; (*a) = 1,*[]; p a
    a = 0; puts 'repro: (*a) = [1,2],3'; (*a) = [1,2],3; p a
    a = 0; puts 'repro: (*a) = nil,1'; (*a) = nil,1; p a
end
def f8()
    a,b,c = 0; puts 'repro: (a,b),c = nil'; (a,b),c = nil; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = []'; (a,b),c = []; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = [nil]'; (a,b),c = [nil]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = 1'; (a,b),c = 1; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = 1,2'; (a,b),c = 1,2; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = 1,2,3'; (a,b),c = 1,2,3; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = [1]'; (a,b),c = [1]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = [1,2]'; (a,b),c = [1,2]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = [1,2,3]'; (a,b),c = [1,2,3]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = *1'; (a,b),c = *1; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = *nil'; (a,b),c = *nil; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = *[]'; (a,b),c = *[]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = *[nil]'; (a,b),c = *[nil]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = *[1]'; (a,b),c = *[1]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = *[1,2]'; (a,b),c = *[1,2]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = 1,*[2,3]'; (a,b),c = 1,*[2,3]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = *[[]]'; (a,b),c = *[[]]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = 1,*[]'; (a,b),c = 1,*[]; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = [1,2],3'; (a,b),c = [1,2],3; p a,b,c
    a,b,c = 0; puts 'repro: (a,b),c = nil,1'; (a,b),c = nil,1; p a,b,c
end
f0()
f1()
f2()
f3()
f4()
f5()
f6()
f7()
f8()
