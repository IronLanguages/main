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

# exception_stmt = 
#   "begin"
#       statement 
#   rescue_block
#   else_block
#   ensure_block
#   "end"

# rescue_block = 
#       <empty> 
#   |   "rescue" 
#           statement 
#   |   "rescue (1+'1')" 
#           statement

# else_block = 
#       <empty> 
#   |   "else" 
#           statement

# ensure = <empty> 
#   |   "ensure" 
#           statement

# statement = 
#       <empty>  
#   |   "print B" 
#   |   raise "B" 
#   |   1/0 
#   |   "retry" 
#   |   "next" 
#   |   "return" 
#   |   puts $!.class 
#   |   exception_stmt 

max_depth = 4

def exception_stmt_generator(depth):
    if depth > max_depth: 
        return 

    for rescue_b in rescue_block_generator(depth):
        for else_b in else_block_generator(depth):
            # warning: else without rescue is useless
            #if rescue_b.lstrip().startswith("#empty") and not else_b.lstrip().startswith("#empty"): 
            #    continue

            for ensure_b in ensure_block_generator(depth):
                for body_b in statement_generator(depth+1):
                    s = space(depth) + "print B, $!.class\n"
                    s += space(depth) + "begin\n"
                    s += body_b 
                    s += rescue_b 
                    s += else_b
                    s += ensure_b
                    s += space(depth) + "end\n"
                    s += space(depth) + "print B, $!.class\n"
                    yield s

def rescue_block_generator(depth):
    if depth > max_depth: return
        
    yield space(depth) + "#empty_rescue\n"
    yield space(depth) + "rescue (print B, $!.class; raise TypeError; IOError)\n" + space(depth+1) + "puts $!.class\n"
    
    for stmt_b in statement_generator(depth+1):
        s = space(depth) + "rescue => ex\n"
        s += space(depth+1) +"print B, ex.class, $!.class\n"
        s += stmt_b
        s += space(depth+1) +"print B, ex.class, $!.class\n"
        yield s
        
    for stmt_b in statement_generator(depth+1):
        s = space(depth) + "rescue IOError => ex\n"
        s += space(depth+1) +"print B, ex.class, $!.class\n"
        s += stmt_b
        s += space(depth+1) +"print B, ex.class, $!.class\n"
        yield s        

def else_block_generator(depth):
    if depth > max_depth: return
        
    yield space(depth) + "#empty_else\n"
    for stmt_b in statement_generator(depth+1):
        s = space(depth) + "else\n" 
        s += space(depth+1) + "print B, $!.class\n"
        s += stmt_b
        s += space(depth+1) + "print B, $!.class\n"
        yield s

def ensure_block_generator(depth):
    if depth > max_depth: return
    
    yield space(depth) + "#empty_ensure\n"
    for stmt_b in statement_generator(depth+1):
        s = space(depth) + "ensure\n" 
        s += space(depth+1) + "print B, $!.class\n"
        s += stmt_b
        s += space(depth+1) + "print B, $!.class\n"
        yield s

def statement_generator(depth):
    if depth > max_depth: return
    
    for x in [
            "#empty_stmt",
            "raise \"B\"",
            "raise SyntaxError.new",
            "1/0",
            "return B",
            "break B",
            "while true; print B; raise \"B\";end",
            "$g += 1; if $g < 4; print B; retry; print B; end;"
             ]:
        yield space(depth) + x + "\n"
    
    for x in exception_stmt_generator(depth+1):
        yield x


fc = FileCreator("test_exception1", 400)
count = 0

for x in exception_stmt_generator(1):
    line1 = "def f_%s\n" % count + replace_B(x) + "end\n"
    line1 += "$g = 0; begin; print(f_%s); rescue Exception; print $!.class; end; puts \" : f_%s\"\n" % (count, count)
    count += 1
    
    fc.save_block(replace_B(line1))

fc.close()
fc.print_file_list()    
    
