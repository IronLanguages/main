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

import math

earthRadius = 6378137
earthCircumference = earthRadius * 2 * math.pi
pixelsPerTile = 256
projectionOriginOffset = earthCircumference / 2
minLat = -85.0511287798
maxLat = 85.0511287798

def DegreesToRadians(deg):
    return deg * math.pi / 180

def LLToMeters(lat, lon):
    lat = DegreesToRadians(lat)
    lon = DegreesToRadians(lon)
    sinLat = math.sin(lat)
    x = earthRadius * lon
    y = earthRadius / 2 * math.log((1+sinLat)/(1-sinLat))
    return (x, y)

def MaxTiles(level):
    return 1 << level

def MetersPerTile(level):
    return earthCircumference / MaxTiles(level)

def MetersPerPixel(level):
    return MetersPerTile(level) / pixelsPerTile

def MetersToPixel(meters, level):
    metersPerPixel = MetersPerPixel(level)
    x = (projectionOriginOffset + meters[0]) / metersPerPixel + 0.5
    y = (projectionOriginOffset - meters[1]) / metersPerPixel + 0.5
    return (x, y)

def LLToPixel(lat, lon, level):
    return MetersToPixel(LLToMeters(lat, lon), level)

def PixelToTile(pixel):
    x = int(pixel[0] / pixelsPerTile)
    y = int(pixel[1] / pixelsPerTile)
    return (x, y)

def LLToTile(lat, lon, level):
    if lat < minLat:
        lat = minLat
    elif lat > maxLat:
        lat = maxLat
    return PixelToTile(LLToPixel(lat, lon, level))

def TileToQuadkey(tile, level):
    quadkey = ""
    for i in range(level, 0, -1):
        mask = 1 << (i-1)
        cell = 0
        if tile[0] & mask != 0:
            cell += 1
        if tile[1] & mask != 0:
            cell += 2
        quadkey += str(cell)
    return quadkey

def LLToQuadkey(lat, lon, level):
    return TileToQuadkey(LLToTile(lat, lon, level), level)