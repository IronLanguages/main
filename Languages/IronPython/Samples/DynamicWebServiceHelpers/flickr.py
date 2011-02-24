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

# sample calling Flickr REST web service

# requires API key from flickr.com
from sys import argv
if len(argv)==1:
    print "This sample needs a Flickr API key passed to it from the command line!"
    from sys import exit
    exit(1)
else:
    apiKey = argv[1]

searchString = 'IronPython'

if apiKey is None:
    raise RuntimeError, 'Flickr API key is required to run this sample'

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
import DynamicWebServiceHelpers

print 'loading web service'
ws = DynamicWebServiceHelpers.WebService.Load('http://www.flickr.com/services/rest/')

print 'calling web service'
r = ws.Invoke(method='flickr.photos.search', text=searchString, api_key=apiKey)

if r.stat == 'ok':
    print 'Flickr search for "%s" (%s results, %s pages):' % (searchString, r.photos.total, r.photos.pages)
    if r.photos.total > 0:
        for p in r.photos:
            link = 'http://static.flickr.com/%s/%s_%s.jpg' % (p.server, p.id, p.secret)
            print '    title: %s, id: %s, link: %s' % (p.title, p.id, link)
else:
    if r.stat == 'fail':
        print 'error: ' + r.err.msg
