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

import clr
from framework import *

class GravityAffector(object):
    def __init__(self, obj, mass, radius, initialVelocity):
        self.Object = obj
        self.Radius = radius
        self.Affectors = []
        self.Mass = mass
        self.Velocity = Vectorize(initialVelocity)
        self.Acceleration = DirectX.Vector3(0, 0, 0)

    def ApplyGravity(self):
        self.Acceleration = DirectX.Vector3(0, 0, 0)
        
        for obj in self.Affectors:
            if obj != self.Object:
                direction = obj.Object.Position - self.Object.Position
                distSquared = direction.LengthSq()
                direction.Normalize()
                
                if distSquared > self.Radius:
                    self.Acceleration += direction * single(obj.Mass / distSquared)
                    
    def OnFrame(self, elapsed):
        self.Velocity += self.Acceleration * single(elapsed)
        self.Object.Translate(self.Velocity * single(elapsed))

        
class GravityManager(object):
    def __init__(self, objs = []):
        self.Objects = objs
    
    def OnFrame(self, elapsed):
        for obj in self.Objects:
            obj.ApplyGravity()
            
        for obj in self.Objects:
            obj.OnFrame(elapsed)

            
class MidpointAutoTrack(object):
    def __init__(self, obj, others, offset):
        self.Object = obj
        self.Others = others
        self.Offset = Vectorize(offset)
        
    def OnFrame(self, elapsed): 
        l = None
        pos = DirectX.Vector3(0, 0, 0)
        
        if type(self.Others)==list:
            l = len(self.Others)
        
        if l is None:
            pos = self.Others.Position
            
        elif l == 1:
            pos = self.Others[0].Position
            
        elif l > 1:
            for o in self.Others:
                pos += o.Position
            
            pos = DirectX.Vector3(pos.X/(l-1), 
                                  pos.Y/(l-1), 
                                  pos.Z/(l-1))
            
        self.Object.Position = pos + self.Offset
        self.Object.LookAt(pos)


class GravityDemoCreator(object):
    def OnSceneCreate(self, sm):
        objs = []

        # add: color, mass, position, velocity
        objs.Add( (Drawing.Color.Red,    1, [-1, 0, 0], [0.05, 0.05, 0]) )
        objs.Add( (Drawing.Color.Green,  1, [0, 1, 0],  [0.1, 0, 0]) )
        objs.Add( (Drawing.Color.Blue,   1, [1, 0, 1],  [-0.1, 0, 0]) )
        objs.Add( (Drawing.Color.White, 1,  [0, 0, 1], [0, 0, 0]) )
        objs.Add( (Drawing.Color.Orange, 1, [1, 1, 1], [0, 0, 0]) )
        objs.Add( (Drawing.Color.Brown, 1,  [-1, -1, -1], [0, 0, 0]) )
        objs.Add( (Drawing.Color.Purple, 1, [-1, 1, -1], [0, 0, 0]) )

        radius = 0.05
        affectors = []
        finalObjects = []
        
        i = 0
        for color, mass, position, velocity in objs:
            sphere = sm.LoadBasicObject("sphere", "planet " + str(i), color, radius, 25, 25)
            sphere.Position = position
            
            affectors.Add( GravityAffector(sphere, mass, radius, velocity) )
            finalObjects.Add(sphere)
            
            i += 1
            
        for g in affectors:
            g.Affectors = affectors
        
        gm = GravityManager(affectors)
        sm.AddListener(gm)
        
        cam = sm.CreateCamera("Player Cam")
        sm.AddListener(MidpointAutoTrack(cam, finalObjects, [0, 0, 15]))
        
        return True
    
if __name__ == '__main__':
    Root().Main([GravityDemoCreator])
