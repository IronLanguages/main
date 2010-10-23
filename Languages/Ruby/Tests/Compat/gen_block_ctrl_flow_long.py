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
   
f = open("template_block_ctrl_flow.rb")
line = "".join(f.readlines())
f.close()

import sys

if len(sys.argv) > 1 and sys.argv[1] == "main":
    mapping = {
            'puts "A"' : "normal", 
            'raise IOError' : "raise", 
            'return' : "return", 
            'break' : "break",
            'next' : "next",
            '$g += 1; retry if $g < 4;' : "retry",
            '$g += 1; redo if $g < 4;' : "redo",
    }        
    
    for (x, y) in mapping.iteritems():
        f = file("test_block_ctrl_flow_%s.rb" % y, "w")
        new_line = replace_B(line.replace("ctrl_flow", x))
        f.writelines(new_line)
        f.close()
else: 
    extras = [
            'return 41',
            'break 42',

            'if 2 == 1; return "B"; end',
            'if 4 == 4; return "B"; end',
            
            'true and return "B"', 
            'false or return "B"',
            
            #'eval("next")',
            #'eval("break")',
            #'eval("return")',
            #'eval("$g += 1; retry if $g < 4;")',
            #'eval("$g += 1; redo if $g < 4;")',
            #'eval("if 1==2; return; end")', 
            
            #'myeval("break")', 
    ]

    fc = FileCreator("test_block_ctrl_flow", 1)

    for x in extras:
        new_line = replace_B(line.replace("ctrl_flow", x))
        fc.save_block(new_line)

    fc.close()   
    fc.print_file_list()
