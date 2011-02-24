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

from compat_common import *

left_side = '''
A
A,A
*A
A,*A
(A,)
(A,A)
(A,*A)
(*A)
(A,A),A
'''.split()

right_side = '''
nil
[]
[nil]
B
B,B
B,B,B
[B]
[B,B]
[B,B,B]
*B
*nil
*[]
*[nil]
*[B]
*[B,B]
B,*[B,B]
*[[]]
B,*[]
[B,B],B
nil,B
'''.split()

lines = []
fc = 0

print '''
def p *a
  a.each { |x| print x.inspect, '; ' }
  puts
end
'''

for x in left_side:
    left, vars_num = replace_A(x)
    vars_text = concat_char(vars_num)
        
    print "def f%s()" % fc        
    for y in right_side:
        right = replace_B(y)
        
        print "    %s = 0; puts 'repro: %s = %s'; %s = %s; p %s" % (vars_text, left, right, left, right, vars_text)

    print "end"        
    fc += 1

for x in range(fc):
    print "f%s()" % x

