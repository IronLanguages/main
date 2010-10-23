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

def take_block
    x = yield 
    $g += 1000 
    x
end 

def take_arg_and_block(arg)
    x = yield arg
    $g += 1000
    x
end 

def call_method_which_take_block(&p)
    x = take_block(&p)
    $g += 10000
    x
end

def call_method_which_take_arg_and_block(arg, &p)
    x = take_arg_and_block(arg, &p)
    $g += 10000
    x
end

def take_block_in_loop
    for i in [1, 2, 3]
        x = yield
        $g += 1000
    end 
    x
end 

def take_block_return_block &p
    p
end 