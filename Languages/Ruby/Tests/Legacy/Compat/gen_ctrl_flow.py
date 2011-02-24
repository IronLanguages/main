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
#    STMT
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

#STMT = <empty> | print B | return | IF_EXPR | UNLESS_EXPR | IF_MODIFIER_EXPR | UNLESS_MODIFIER_EXPR | begin; print B; end

#WHILE_EXPR = while CONDITION do
#               STMT_FOR_LOOP
#             end 
#WHILE_MODIFIER_EXPR = STMT_FOR_LOOP while CONDITION

#UNTIL_EXPR = until CONDITION do 
#               STMT_FOR_LOOP
#             end 
#UNTIL_MODIFIER_EXPR = STMT_FOR_LOOP until CONDITION

#FOR_EXPR = for x in EXPR do
#               STMT_FOR_LOOP
#           end

#STMT_FOR_LOOP = STMT | break | redo | next | retry
