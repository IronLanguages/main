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

# testing array constructor [] and return values
# LHS is always L(1,-)

def i *a
  a.each { |x| print x.inspect, '; ' }
  puts
end

puts "", '#A1'

# L(1,-) = R(0,*)
a = [*1]; i a
a = [*[]]; i a
a = [*[1]]; i a
a = [*[1,2]]; i a

puts "", '#A2'

# L(1,-) = R(1,-)
a = [1]; i a
a = [[]]; i a
a = [[1]]; i a
a = [[1,2]]; i a

puts "", '#A3'

# L(1,-) = R(1,*)
a = [1,*1]; i a
a = [1,*[]]; i a
a = [1,*[1]]; i a
a = [1,*[1,2]]; i a
a = [[],*[]]; i a
a = [[1],*[1]]; i a
a = [[1,2],*[1,2]]; i a

puts "", '#R0'

#L(1,-) = R(0,-)
def f; return; end; i f

puts "", '#R1'

# L(1,-) = R(0,*)
def f; return *1; end; i f
def f; return *[]; end; i f
def f; return *[1]; end; i f
def f; return *[1,2]; end; i f

puts "", '#R2'

# L(1,-) = R(1,-)
def f; return 1; end; i f
def f; return []; end; i f
def f; return [1]; end; i f
def f; return [1,2]; end; i f

puts "", '#R3'

# L(1,-) = R(1,*)
def f; return 1,*1; end; i f
def f; return 1,*[]; end; i f
def f; return 1,*[1]; end; i f
def f; return 1,*[1,2]; end; i f
def f; return [],*[]; end; i f
def f; return [1],*[1]; end; i f
def f; return [1,2],*[1,2]; end; i f

