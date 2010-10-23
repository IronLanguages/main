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

copyrightSymbol = u'\u00A9'
copyright_Navteq = copyrightSymbol + ' 2006 NAVTEQ'
copyright_AND = copyrightSymbol + ' AND'
copyright_NASA = 'Image courtesy of NASA'
copyright_HarrisCorp = copyrightSymbol + ' Harris Corp, Earthstar Geographics LLC'
copyright_USGS = 'Image courtesy of USGS'
copyright_EarthData = copyrightSymbol + ' EarthData'
copyright_Getmapping = copyrightSymbol + ' Getmapping plc'

roadProviders = [copyright_Navteq, copyright_AND]
aerialProviders = [copyright_NASA, copyright_HarrisCorp, copyright_USGS, copyright_EarthData, copyright_Getmapping]

creditEntry = {}
for vendor in roadProviders:
    creditEntry[vendor] = []
for vendor in aerialProviders:
    creditEntry[vendor] = []

# each tuple is minz, maxz, minlat, minlong, maxlat, maxlon

creditEntry[copyright_Navteq] += [(1, 9, -90, -180, 90, 180)]
creditEntry[copyright_Navteq] += [(10, 19, 16, -180, 90, -50)]
creditEntry[copyright_Navteq] += [(10, 19, 27, -32, 40, -13)]
creditEntry[copyright_Navteq] += [(10, 19, 35, -11, 72, 20)]
creditEntry[copyright_Navteq] += [(10, 19, 21, -20, 72, 32)]

creditEntry[copyright_AND] += [(10, 19, -90, -180, 90, 180)]

creditEntry[copyright_NASA] += [(1, 8, -90, -180, 90, 180)]

creditEntry[copyright_HarrisCorp] += [(9, 13, -90, -180, 90, 180)]

creditEntry[copyright_USGS] += [(14, 19, 17.99, -150.11, 61.39, -65.57)]

creditEntry[copyright_EarthData] += [(14, 19, 21.25, -158.30, 21.72, -157.64)]
creditEntry[copyright_EarthData] += [(14, 19, 39.99, -80.53, 40.87, -79.43)]
creditEntry[copyright_EarthData] += [(14, 19, 34.86, -90.27, 35.39, -89.60)]

creditEntry[copyright_Getmapping] += [(14, 19, 49.94, -6.35, 58.71, 1.78)]

def GetCaption(type, level, tile):
    global creditEntry
    captions = []
    if type == 'r' or type == 'h':
        for vendor in roadProviders:
            for c in creditEntry[vendor]:
                if IsMatch(level, tile, c):
                    captions += [vendor]
                    break
    if type == 'a' or type == 'h':
        for vendor in aerialProviders:
            for c in creditEntry[vendor]:
                if IsMatch(level, tile, c):
                    captions += [vendor]
                    break
    
    return " - ".join(captions)


def IsMatch(level, tile, creditEntry):
    import quadkey
    if level < creditEntry[0] or level > creditEntry[1]:
        return False
    lowerLeftTile = quadkey.LLToTile(creditEntry[2], creditEntry[3], level)
    upperRightTile = quadkey.LLToTile(creditEntry[4], creditEntry[5], level)
    if tile[0] % quadkey.MaxTiles(level) < lowerLeftTile[0] or tile[0] % quadkey.MaxTiles(level) > upperRightTile[0]:
        return False
    if tile[1] % quadkey.MaxTiles(level) > lowerLeftTile[1] or tile[1] % quadkey.MaxTiles(level) < upperRightTile[1]:
        return False
    return True