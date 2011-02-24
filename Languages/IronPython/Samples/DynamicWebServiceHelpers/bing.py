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

# sample calling Bing search web service

def CreateSearchRequest():
    import SearchRequest, SourceType, AdultOption, SearchOption, WebRequest, WebSearchOption
    request = SearchRequest()

    # Common request fields (required)
    request.AppId = appId
    request.Query = searchString
    request.Sources = (SourceType.Web,)

    # Common request fields (optional)
    request.Version = "2.0"
    request.Market = "en-us"
    request.Adult = AdultOption.Moderate
    request.AdultSpecified = True
    request.Options = (SearchOption.EnableHighlighting,)

    # Web-specific request fields (optional)
    request.Web = WebRequest()
    request.Web.Count = 5
    request.Web.CountSpecified = True
    request.Web.Offset = 0
    request.Web.OffsetSpecified = True
    request.Web.Options = (WebSearchOption.DisableHostCollapsing, WebSearchOption.DisableQueryAlterations)
    return request

# requires AppId from http://bing.com/developers
from sys import argv
if len(argv)==1:
    print "This sample needs a Bing AppId passed to it from the command line!"
    from sys import exit
    exit(1)
else:
    appId = argv[1]

searchString = 'IronPython'

if appId is None:
    raise RuntimeError, 'Bing AppId is required to run this sample'

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
import DynamicWebServiceHelpers

print 'loading web service'
ws = DynamicWebServiceHelpers.WebService.Load('http://api.search.live.net/search.wsdl')
clr.AddReference(clr.GetClrType(type(ws)).Assembly)

print 'calling web service'
results = ws.Search(CreateSearchRequest())

print
for result in results.Web.Results:
    print "Title=%s\nDescription=%s\nUrl=%s\n" % (result.Title, result.Description, result.Url)