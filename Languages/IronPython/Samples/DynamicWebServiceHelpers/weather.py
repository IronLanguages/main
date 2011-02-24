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

# web service returning weather data as complex objects

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
import DynamicWebServiceHelpers

print 'loading web service'
ws = DynamicWebServiceHelpers.WebService.Load('http://www.webservicex.net/WeatherForecast.asmx')

print 'calling web service to get forecast'
f = ws.GetWeatherByZipCode('98052')

print 'Forecast for %s, %s ...' % (f.PlaceName, f.StateCode)
for d in f.Details:
    print '    %s: %s - %s' % (d.Day, d.MinTemperatureF, d.MaxTemperatureF)
