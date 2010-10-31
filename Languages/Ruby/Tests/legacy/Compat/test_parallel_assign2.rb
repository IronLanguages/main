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
def y; yield 1; end; y { |a,| i a }
def y; yield []; end; y { |a,| i a }
def y; yield [1]; end; y { |a,| i a }
def y; yield [1,2]; end; y { |a,| i a }

puts "==="

def y; yield 1; end; y { |a,x| i a }
def y; yield []; end; y { |a,x| i a }
def y; yield [1]; end; y { |a,x| i a }
def y; yield [1,2]; end; y { |a,x| i a }

puts "", '#2'

# L(0,*) = R(0,*)
def y; yield *1; end; y { |a,| i a }
def y; yield *[]; end; y { |a,| i a }
def y; yield *[1]; end; y { |a,| i a }
def y; yield *[1,2]; end; y { |a,| i a }

puts '==='

def y; yield *1; end; y { |a,x| i a }
def y; yield *[]; end; y { |a,x| i a }
def y; yield *[1]; end; y { |a,x| i a }
def y; yield *[1,2]; end; y { |a,x| i a }

puts "", '#3'

# L(0,*) = R(1,-)
def y; yield 1; end; y { |*a| i a }
def y; yield []; end; y { |*a| i a }
def y; yield [1]; end; y { |*a| i a }
def y; yield [1,2]; end; y { |*a| i a }

puts "", '#4'

# L(0,*) = R(0,*)
def y; yield *1; end; y { |*a| i a }
def y; yield *[]; end; y { |*a| i a }
def y; yield *[1]; end; y { |*a| i a }
def y; yield *[1,2]; end; y { |*a| i a }

puts "", '#5'

# L(0,*) = R(1,*)
def y; yield 2,*1; end; y { |*a| i a }
def y; yield 2,*[]; end; y { |*a| i a }
def y; yield 2,*[1]; end; y { |*a| i a }
def y; yield 2,*[1,2]; end; y { |*a| i a }

puts "", '#6'

# L(1,-) = R(0,*)
def y; yield *1; end; y { |a| i a }
def y; yield *[]; end; y { |a| i a }
def y; yield *[1]; end; y { |a| i a }
def y; yield *[1,2]; end; y { |a| i a }

puts "", '#6.1'

# L((),-) = R(0,*)
def y; yield *1; end; y { |(a,)| i a }
def y; yield *[]; end; y { |(a,)| i a }
def y; yield *[1]; end; y { |(a,)| i a }
def y; yield *[1,2]; end; y { |(a,)| i a }

puts "", '#7'

# L(1,-) = R(1,-)
def y; yield 1; end; y { |a| i a }
def y; yield []; end; y { |a| i a }
def y; yield [1]; end; y { |a| i a }
def y; yield [1,2]; end; y { |a| i a }

puts "", '#7.1'

# L(1,-) = R(1,*)
def y; yield 1,*2; end; y { |a| i a }
def y; yield 1,*[]; end; y { |a| i a }
def y; yield 1,*[2]; end; y { |a| i a }
def y; yield 1,*[2,3]; end; y { |a| i a }

puts "", '#7.2'

# L((),-) = R(1,-)
def y; yield 1; end; y { |(a,)| i a }
def y; yield []; end; y { |(a,)| i a }
def y; yield [1]; end; y { |(a,)| i a }
def y; yield [1,2]; end; y { |(a,)| i a }

puts "", '#7.3'

# L((),-) = R(1,*)
def y; yield 1,*2; end; y { |(a,)| i a }
def y; yield 1,*[]; end; y { |(a,)| i a }
def y; yield 1,*[2]; end; y { |(a,)| i a }
def y; yield 1,*[2,3]; end; y { |(a,)| i a }

puts "", '#8'

# L(1,*) = R(1,*)
def y; yield 2,*1; end; y { |b,*a| i b,a }
def y; yield 2,*[]; end; y { |b,*a| i b,a }
def y; yield 2,*[1]; end; y { |b,*a| i b,a }
def y; yield 2,*[1,2]; end; y { |b,*a| i b,a }

puts "", '#9'

# L(1,*) = R(0,*)
def y; yield *1; end; y { |a,*b| i a,b }
def y; yield *[]; end; y { |a,*b| i a,b }
def y; yield *[1]; end; y { |a,*b| i a,b }
def y; yield *[1,2]; end; y { |a,*b| i a,b }

puts "", '#10'

# L(1,*) = R(1,-)
def y; yield 1; end; y { |a,*b| i a,b }
def y; yield []; end; y { |a,*b| i a,b }
def y; yield [1]; end; y { |a,*b| i a,b }
def y; yield [1,2]; end; y { |a,*b| i a,b }

puts "", '#11'

def y; yield 1; end; y { |(a,)| i a }
def y; yield []; end; y { |(a,)| i a }
def y; yield [1]; end; y { |(a,)| i a }
def y; yield [1,2]; end; y { |(a,)| i a }
def y; yield [[]]; end; y { |(a,)| i a }
def y; yield [[1]]; end; y { |(a,)| i a }
def y; yield [[1,2]]; end; y { |(a,)| i a }

puts '==='

def y; yield 1; end; y { |(a,x)| i a }
def y; yield []; end; y { |(a,x)| i a }
def y; yield [1]; end; y { |(a,x)| i a }
def y; yield [1,2]; end; y { |(a,x)| i a }
def y; yield [[]]; end; y { |(a,x)| i a }
def y; yield [[1]]; end; y { |(a,x)| i a }
def y; yield [[1,2]]; end; y { |(a,x)| i a }

puts "", '#12'

# L(1+L(0,*),-) = R(2,-)
def y; yield 1,[2,*1]; end; y { |b,(*a)| i a,b }
def y; yield 1,[2,*[]]; end; y { |b,(*a)| i a,b }
def y; yield 1,[2,*[1]]; end; y { |b,(*a)| i a,b }
def y; yield 1,[2,*[1,2]]; end; y { |b,(*a)| i a,b }

puts "", '#13'

# L(n,-) = R(1,-)
def y; yield 1; end; y { |a,b| i a,b }
def y; yield []; end; y { |a,b| i a,b }
def y; yield [1]; end; y { |a,b| i a,b }
def y; yield [1,2]; end; y { |a,b| i a,b }

puts "", '#14'

# L(n,*) = R(1,-)
def y; yield 1; end; y { |a,b,*c| i a,b,c }
def y; yield []; end; y { |a,b,*c| i a,b,c }
def y; yield [1]; end; y { |a,b,*c| i a,b,c }
def y; yield [1,2]; end; y { |a,b,*c| i a,b,c }

puts "", '#15'

# L(n,-) = R(0,*)
def y; yield *1; end; y { |a,b| i a,b }
def y; yield *[]; end; y { |a,b| i a,b }
def y; yield *[1]; end; y { |a,b| i a,b }
def y; yield *[1,2]; end; y { |a,b| i a,b }

puts "", '#16'

# L(n,*) = R(0,*)
def y; yield *1; end; y { |a,b,*c| i a,b,c }
def y; yield *[]; end; y { |a,b,*c| i a,b,c }
def y; yield *[1]; end; y { |a,b,*c| i a,b,c }
def y; yield *[1,2]; end; y { |a,b,*c| i a,b,c }

puts "", '#17'

# L((), -) = R(0,*)
def y; yield *[]; end; y { |(z,)| i z }
def y; yield *nil; end; y { |(z,)| i z }
def y; yield *1; end; y { |(z,)| i z }
def y; yield *[1]; end; y { |(z,)| i z }
def y; yield *[1,2]; end; y { |(z,)| i z }

puts "", '#18'

# L(1, -) = R(0,*)
def y; yield *[]; end; y { |(z,)| i z }
def y; yield *nil; end; y { |(z,)| i z }
def y; yield *1; end; y { |(z,)| i z }
def y; yield *[1]; end; y { |(z,)| i z }
def y; yield *[1,2]; end; y { |(z,)| i z }

puts "", '#19'

# L(1,-) = R(2,-)
def y; yield 1,2; end; y { |z,| i z }
def y; yield 1,2; end; y { |(z,)| i z }
def y; yield 1,2; end; y { |((z,))| i z }
def y; yield 1,2; end; y { |(((z,)))| i z }
