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

#IF_EXPR = if CONDITION then
#       STMT
#    ELSIF_EXPR
#    ELSE_EXPR
#    end

#UNLESS_EXPR = unless CONDITION then
#    STMT
#    ELSE_EXPR
#    end 

#ELSIF_EXPR = <empty> | elsif CONDITION then
#                           STMT 
#                       ELSIF_EXPR
#ELSE_EXPR = <empty> | else 
#                           STMT 

#IF_MODIFIER_EXPR = STMT if CONDITION
#UNLESS_MODIFIER_EXPR = STMT unless CONDITION

#STMT = <empty> | print B | break | redo | next | retry | IF_EXPR | UNLESS_EXPR | IF_MODIFIER_EXPR | UNLESS_MODIFIER_EXPR | begin; print B; end

max_depth = 4
def space(d): return "  " * d

def if_expr_generator(depth):
    if depth > max_depth: return 
    
    for x in stmt_generator(depth+1):
        for y in elsif_expr_generator(depth+1):
            for z in else_expr_generator(depth+1):
                s = space(depth) + "if A then\n"
                s += x
                s += y
                s += z 
                s += space(depth) + "end"
                yield s + "\n"

def unless_expr_generator(depth):
    if depth > max_depth: return 
    
    for x in stmt_generator(depth+1):
        for y in else_expr_generator(depth+1):
            s = space(depth) + "unless A then\n"
            s += x 
            s += y 
            s += space(depth) + "end"
            yield s + "\n"

def elsif_expr_generator(depth):
    if depth > max_depth: return 
    
    yield space(depth) + "#empty_elsif\n"
    for x in stmt_generator(depth+1):
        for y in elsif_expr_generator(depth+1):
            s = space(depth) + "elsif A then\n"
            s += x 
            s += y 
            yield s + "\n"

def else_expr_generator(depth):
    if depth > max_depth: return 
    
    yield space(depth) + "#empty_else\n"
    for x in stmt_generator(depth+1):
        s = space(depth) + "else\n"
        s += x 
        yield s + "\n"

def stmt_generator(depth):        
    if depth > max_depth: return 
    
    for x in [  "#empty_stmt", 
                "print B",  
                "return B",
                "print B; break; print B", 
                "print B; redo; print B", 
                "print B; next; print B", 
                "print B; retry; print B", 
                "begin; print B; end;"
                ]:
        yield space(depth) + x + "\n"
        
    for x in if_expr_generator(depth+1):
        yield x + "\n"
    
    for x in unless_expr_generator(depth+1):
        yield x + "\n"

    yield space(depth) + "print B unless A\n"
    yield space(depth) + "print B if A\n"
    
fc = FileCreator("test_if_unless", 500, "require 'compat_common.rb'")

count = 0
for x in if_expr_generator(1):
    string, number = replace_A(replace_B(x))
    cc = concat_char(number)
    
    fc.save_block("def f_%s(%s)" % (count, cc), 
        string,
        "rescue",
        "  print 0",
        "end",
        "Bool_Sequence.new(2**%s).each { |%s| begin; print f_%s(%s); rescue; print -1; end; puts ' :f_%s' }\n" % (number, cc, count, cc, count)
    )
    
    count += 1

fc.close()
fc.print_file_list()