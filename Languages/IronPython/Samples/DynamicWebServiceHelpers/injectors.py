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

# attribute injectors

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
import DynamicWebServiceHelpers

# string pluralization

def print_noun(x):
    print '%s ... %s ... %s ... %s' % (x, x.ToPlural(), 
        x.ToPlural().ToSingular(), x.ToPlural().ToSingular().ToPlural())

print_noun('deer')
print_noun('rhino')
print_noun('werewolf')

# XML access

text = '''<?xml version="1.0" encoding="utf-8" ?>
<sample>
<description>XML sample</description>
<item id="1"><text>one</text><details><description>item #1</description></details></item>
<item id="2"><text>two</text><details><description>item #2</description></details></item>
<item id="3"><text>three</text><details><description>item #3</description></details></item>
</sample>'''

print '\nXML:\n%s' % text

x = DynamicWebServiceHelpers.SimpleXml.Load(text)
print x.description
for i in x.items:
    print 'id=%s text=%s description: %s' % (i.id, i.text, i.details.description)
