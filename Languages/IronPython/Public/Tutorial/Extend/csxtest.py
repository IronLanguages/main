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

# Task 1

import clr
clr.AddReferenceToFile("csextend.dll")
import Simple
dir(Simple)
s = Simple(10)
print s

# Task 2

import clr
clr.AddReferenceToFile("csextend.dll")
import Simple
dir(Simple)
s = Simple(10)
for i in s: print i

# Task 3

import clr
clr.AddReferenceToFile("csextend.dll")
import Simple
dir(Simple)
a = Simple(10)
b = Simple(20)
a + b

# Task 4

import clr
clr.AddReferenceToFile("csextend.dll")
import Simple
a = Simple(10)
def X(i):
    return i + 100

a.Transform(X)
