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

# reading RSS feed with the help of the attribute injector for XML

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
import DynamicWebServiceHelpers

print 'loading RSS channel'
rss = DynamicWebServiceHelpers.WebService.Load('http://rss.msnbc.msn.com/id/3032091/device/rss/rss.xml')

print 'processing the RSS XML using attribute injectors'
print '%s (%s)' % (rss.channel.title, rss.channel.lastBuildDate)
for i in rss.channel.items: print '    %s (%s)' % (i.title, i.link)
