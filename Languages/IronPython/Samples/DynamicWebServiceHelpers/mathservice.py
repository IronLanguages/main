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

# trivial web service sample

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
import DynamicWebServiceHelpers

print 'loading web service'
ws = DynamicWebServiceHelpers.WebService.Load('http://www.etecnologia.net/samples/webservices/mathservice.asmx')

print 'calling a method on the web service'
x = ws.Add(40, 2.5)

print 'result is %s' % (x)