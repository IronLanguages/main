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

callee = """
A
*A
A,A
A,*A
A,A,A
A,A,*A
""".split()

caller = """
nil
*nil
*[nil]
nil,nil
nil,nil,nil
nil,*[nil]
B,nil
[]
B
B,B
B,B,B
B,[]
B,[B]
B,[B,B]
[[]]
[B]
[B,B]
[B,B,B]
[B,B,*[B]]
*[]
*B
B,*B
B,B,*B
B,*[]
B,*[B]
B,*[B,B]
*[[]]
*[B]
*[B,B]
*[B,B,B]
*[B,[B,B]]
*[B,*[B,B]]
nil,*B
""".split()

print '''
def p *a
  a.each { |x| print x.inspect, '; ' }
  puts
end
'''

count = 0
for x in callee:
    parameters, vars_num = replace_A(x)
    vars_text = concat_char(vars_num)
    
    print "def test_%s" % count
    count += 1
    
    print "    def f(%s)" % parameters
    print "        return %s" % vars_text
    print "    end"
    
    for y in caller:
        arguments = replace_B(y)
        print "    begin; puts \"repro:%s\"; p f(%s); rescue ArgumentError; puts('except'); end;" % ("f(%s)" % arguments, arguments);
    
    print "end\n"

for x in range(count):
    print "test_%s" % x
