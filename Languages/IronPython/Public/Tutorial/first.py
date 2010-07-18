#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

def add(a, b):
    "add(a, b) -> returns a + b"
    return a + b

def factorial(n):
    "factorial(n) -> returns factorial of n"
    if n <= 1: return 1
    return n * factorial(n-1)

hi = "Hello from IronPython!"
