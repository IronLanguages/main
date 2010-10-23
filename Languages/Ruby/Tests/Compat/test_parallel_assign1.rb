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

def i *a
  a.each { |x| print x.inspect, '; ' }
  puts
end

puts "", '#1'

# L(1+,-) = R(1,-)
r = (a, = 1); i a, r
r = (a, = []); i a, r
r = (a, = [1]); i a, r
r = (a, = [1,2]); i a, r

puts '==='

r = (a,x = 1); i a, r
r = (a,x = []); i a, r
r = (a,x = [1]); i a, r
r = (a,x = [1,2]); i a, r

puts "", '#2'

# L(1+,-) = R(0,*)
r = (a, = *1); i a, r
r = (a, = *[]); i a, r
r = (a, = *[1]); i a, r
r = (a, = *[1,2]); i a, r

puts '==='

r = (a,x = *1); i a, r
r = (a,x = *[]); i a, r
r = (a,x = *[1]); i a, r
r = (a,x = *[1,2]); i a, r

puts "", '#3'

# L(0,*) = R(1,-)
r = (*a = 1); i a, r
r = (*a = []); i a, r
r = (*a = [1]); i a, r
r = (*a = [1,2]); i a, r

puts "", '#4'

# L(0,*) = R(0,*)
r = (*b = *1); i b, r
r = (*b = *[]); i b, r
r = (*b = *[1]); i b, r
r = (*b = *[1,2]); i b, r

puts "", '#5'

# L(0,*) = R(1,*)
r = (*a = 2,*1); i a, r
r = (*a = 2,*[]); i a, r
r = (*a = 2,*[1]); i a, r
r  = (*a = 2,*[1,2]); i a, r

puts "", '#6'

# L(1,-) = R(0,*)
r = (a = *1); i a, r
r = (a = *[]); i a, r
r = (a = *[1]); i a, r
r = (a = *[1,2]); i a, r

puts "", '#6.1'

# L((),-) = R(0,*)
r = ((a,) = *1); i a, r
r = ((a,) = *[]); i a, r
r = ((a,) = *[1]); i a, r
r = ((a,) = *[1,2]); i a, r

puts "", '#7'

# L(1,-) = R(1,-)
r = (a = 1); i a, r
r = (a = []); i a, r
r = (a = [1]); i a, r
r = (a = [1,2]); i a, r

puts "", '#7.1'

# L(1,-) = R(1,*)
r = (a = 1,*2); i a, r
r = (a = 1,*[]); i a, r
r = (a = 1,*[2]); i a, r
r = (a = 1,*[2,3]); i a, r

puts "", '#7.2'

# L(1,-) = R(1,-)
r = ((a,) = 1); i a, r
r = ((a,) = []); i a, r
r = ((a,) = [1]); i a, r
r = ((a,) = [1,2]); i a, r

puts "", '#7.3'

# L((),-) = R(1,*)
r = ((a,) = 1,*2); i a, r
r = ((a,) = 1,*[]); i a, r
r = ((a,) = 1,*[2]); i a, r
r = ((a,) = 1,*[2,3]); i a, r

puts "", '#8'

# L(1,*) = R(1,*)
r = (b,*a = 2,*1); i b,a, r
r = (b,*a = 2,*[]); i b,a, r
r = (b,*a = 2,*[1]); i b,a, r
r = (b,*a = 2,*[1,2]); i b,a, r

puts "", '#9'

# L(1,*) = R(0,*)
r = (a,*b = *1); i a,b, r
r = (a,*b = *[]); i a,b, r
r = (a,*b = *[1]); i a,b, r
r = (a,*b = *[1,2]); i a,b, r

puts "", '#10'

# L(1,*) = R(1,-)
r = (a,*b = 1); i a,b, r
r = (a,*b = []); i a,b, r
r = (a,*b = [1]); i a,b, r
r = (a,*b = [1,2]); i a,b, r

puts "", '#11'

# L(L(1+,-),-) = R(2,-)
r = ((b,) = 1); i b, r
r = ((b,) = []); i b, r
r = ((b,) = [1]); i b, r
r = ((b,) = [1,2]); i b, r
r = ((b,) = [[]]); i b, r
r = ((b,) = [[1]]); i b, r
r = ((b,) = [[1,2]]); i b, r

puts "==="

# L(L(1,-),-) = R(2,-)
r = ((b,x) = 1); i b, r
r = ((b,x) = []); i b, r
r = ((b,x) = [1]); i b, r
r = ((b,x) = [1,2]); i b, r
r = ((b,x) = [[]]); i b, r
r = ((b,x) = [[1]]); i b, r
r = ((b,x) = [[1,2]]); i b, r

puts "", '#12'

# L(1+L(0,*),-) = R(2,-)
r = (b,(*a) = 1,[2,*1]); i b,a, r
r = (b,(*a) = 1,[2,*[]]); i b,a, r
r = (b,(*a) = 1,[2,*[1]]); i b,a, r
r = (b,(*a) = 1,[2,*[1,2]]); i b,a, r

puts "", '#13'

# L(n,-) = R(1,-)
r = (a,b = 1); i a,b, r
r = (a,b = []); i a,b, r
r = (a,b = [1]); i a,b, r
r = (a,b = [1,2]); i a,b, r

puts "", '#14'

# L(n,*) = R(1,-)
r = (a,b,*c = 1); i a,b,c, r
r = (a,b,*c = []); i a,b,c, r
r = (a,b,*c = [1]); i a,b,c, r
r = (a,b,*c = [1,2]); i a,b,c, r

puts "", '#15'

# L(n,-) = R(0,*)
r = (a,b = *1); i a,b, r
r = (a,b = *[]); i a,b, r
r = (a,b = *[1]); i a,b, r
r = (a,b = *[1,2]); i a,b, r

puts "", '#16'

# L(n,*) = R(0,*)
r = (a,b,*c = *1); i a,b,c, r
r = (a,b,*c = *[]); i a,b,c, r
r = (a,b,*c = *[1]); i a,b,c, r
r = (a,b,*c = *[1,2]); i a,b,c, r

puts "", '#17'

# L((), -) = R(0,*)
r = ((z,) = *[]); i z,r
r = ((z,) = *nil); i z,r
r = ((z,) = *1); i z,r
r = ((z,) = *[1]); i z,r
r = ((z,) = *[1,2]); i z,r
r = ((z,) = *nil); i z,r
r = ((z,) = 1,*[]); i z,r

puts "", '#18'

# L(1, -) = R(0,*)
r = (z = *[]); i z,r
r = (z = *nil); i z,r
r = (z = *1); i z,r
r = (z = *[1]); i z,r
r = (z = *[1,2]); i z,r

puts "", '#19'

# L(1,-) = R(2,-)
r = (a = 1,2); i a,r
r = ((a,) = 1,2); i a,r
r = (((a,)) = 1,2); i a,r
r = ((((a,))) = 1,2); i a,r
