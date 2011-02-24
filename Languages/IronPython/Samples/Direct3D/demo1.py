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

class DirectionalMover(object):
    def __init__(self, obj, direction, speed, distance):
        self.Object = obj
        self.Speed = speed
        self.Distance = distance
        self.Direction = Vectorize(direction)
        self.Direction.Normalize()

    def OnFrame(self, elapsed):
        toTravel = self.Speed * elapsed

        if self.Distance < toTravel:
            self.Object.Translate(self.Direction * single(self.Distance))
            return True
        else:
            self.Distance -= toTravel
            self.Object.Translate(self.Direction * single(toTravel))


class DemoSceneCreator(object):
    def OnSceneCreate(self, sm):
        cam = sm.CreateCamera("Player Cam")
        cam.Position = [0, 3, -5]
        
        box = sm.LoadBasicObject("box", "box 1", Drawing.Color.Red, 0.25, 0.25, 0.25)
        box.Position = [-1, 0, 0]
        
        box = sm.LoadBasicObject("box", "box 2", Drawing.Color.Green, 0.25, 0.25, 0.25)
        box.Position = [0, 0, 0]
        cam.LookAt(box.Position)
        
        box = sm.LoadBasicObject("box", "box 3", Drawing.Color.Blue, 0.25, 0.25, 0.25)
        box.Position = [1, 0, 0]

        return True

    def OnSceneBegin(self, sm):
        dm = DirectionalMover(sm.Objects["box 1"], [1, 0, 0], 0.3, 3)
        sm.AddListener(dm)
        
        dm = DirectionalMover(sm.Objects["box 2"], [0, 1, 0], 0.2, 2)
        sm.AddListener(dm)
        
        dm = DirectionalMover(sm.Objects["box 3"], [0, 0, 1], 1, 10)
        sm.AddListener(dm)

        return True


if __name__ == '__main__':
    Root().Main([DemoSceneCreator])
