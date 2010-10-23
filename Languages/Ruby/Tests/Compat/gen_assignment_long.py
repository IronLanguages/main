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

# left = A | (A, ) | (*A) | *(A) | (A, left) | A, left 
#          | *A  | *(A, left)  # has to be the last

def left_generator(depth):
    if depth > 4: return 
    
    for x in ["A", "(A,)", "(*A)", "*A"]:
        yield x

    #for x in left_generator(depth + 1):
    #   yield "*(A, %s)" % x
        
    for x in left_generator(depth + 1):
        yield "A, %s" % x
        
    for x in left_generator(depth + 1):
        yield "(A, %s)" % x
    
    
# right = B | [] | *[] | B, right | *[right] | [right] 
 
def right_generator(depth):
    if depth > 5: return
    
    for x in ["[]", "*[]", "B"]:
        yield x
        
    for x in right_generator(depth+1):
        yield "B, %s" % x

    for x in right_generator(depth+1):
        yield "[%s]" % x
    
    for x in right_generator(depth+1):
        yield "*[%s]" % x

fc = FileCreator("test_assignment", 40, '''
def p *a
  a.each { |x| print x.inspect, '; ' }
  puts
end
''')

fc.func_size = 40

for x in left_generator(0):
    left, vars_num = replace_A(x)
    vars_text = concat_char(vars_num)
    
    for y in right_generator(0):
        right = replace_B(y)
        fc.save_to_function("    %s = 0; puts 'repro: %s = %s'; %s = %s; p %s" % (vars_text, left, right, left, right, vars_text))
        #fc.save_to_function("    %s = 0; def f; yield %s; end; p(f {|%s| [%s] }); " % (vars_text, right, left, vars_text))
        
for y in right_generator(0):
    right = replace_B(y)
    fc.save_to_function("    p(while true; break %s; end)" % (right, ))
    fc.save_to_function("    p([%s])" % (right, ))
        
fc.close()
fc.print_file_list()    

