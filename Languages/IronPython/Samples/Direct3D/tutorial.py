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
clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("System.Windows.Forms")
clr.AddReferenceByPartialName("Microsoft.DirectX")
clr.AddReferenceByPartialName("Microsoft.DirectX.Direct3D")
clr.AddReferenceByPartialName("Microsoft.DirectX.Direct3DX")

import System
from System import Drawing
from System.Windows import Forms
from Microsoft import DirectX
from Microsoft.DirectX import Direct3D


def Vectorize(v):
    if type(v)==list:
        v = DirectX.Vector3(v[0], v[1], v[2])
    return v
    
        

class RotatableObject(object):
    def __init__(self):
        self.RotationMatrix = DirectX.Matrix.Identity
    
    def ResetOrientation(self):
        self.RotationMatrix = DirectX.Matrix.Identity
    
    def Pitch(self, p):
        self.RotationMatrix *= DirectX.Matrix.RotationX(p)
    
    def Yaw(self, y):
        self.RotationMatrix *= DirectX.Matrix.RotationY(y)
    
    def Roll(self, r):
        self.RotationMatrix *= DirectX.Matrix.RotationZ(r)
    
    def LookAt(self, pos, up=DirectX.Vector3(0, 1, 0)):
        pos, up = Vectorize(pos), Vectorize(up)
        self.RotationMatrix = DirectX.Matrix.LookAtLH(self.Position, pos, up) * \
                              DirectX.Matrix.Translation(-self.Position)

class PositionableObject(object):
    def __init__(self):
        self.__Position = DirectX.Vector3(0, 0, 0)
        self.PositionMatrix = DirectX.Matrix.Identity
    
    def __GetPosition(self):
        return self.__Position
    
    def __SetPosition(self, pos):
        pos = Vectorize(pos)
        self.__Position = pos
        self.PositionMatrix = DirectX.Matrix.Translation(self.__Position)
        
    def __DelPosition(self):
        self.__Position = DirectX.Vector3(0, 0, 0)
        self.PositionMatrix = DirectX.Matrix.Identity
    
    def Translate(self, amount):
        amount = Vectorize(amount)
        self.__Position += amount
        self.PositionMatrix = DirectX.Matrix.Translation(self.__Position)
    
    Position = property(__GetPosition, __SetPosition, __DelPosition)


class SceneObject(PositionableObject, RotatableObject):
    def __init__(self):
        self.Matrix = DirectX.Matrix.Identity
        self.__PositionMatrix = DirectX.Matrix.Identity
        self.__RotationMatrix = DirectX.Matrix.Identity
        
        PositionableObject.__init__(self)
        RotatableObject.__init__(self)
    
    def __GetPositionMatrix(self):
        return self.__PositionMatrix
    
    def __SetPositionMatrix(self, value):
        self.__PositionMatrix = value
        self.Matrix = self.__RotationMatrix * self.__PositionMatrix
    
    def __GetRotationMatrix(self):
        return self.__RotationMatrix
    
    def __SetRotationMatrix(self, value):
        self.__RotationMatrix = value
        self.Matrix = self.__RotationMatrix * self.__PositionMatrix
        
    PositionMatrix = property(__GetPositionMatrix, __SetPositionMatrix)
    RotationMatrix = property(__GetRotationMatrix, __SetRotationMatrix)
    
        


class Camera(object):
    def __init__(self, name):
        self.Name = name
        self.__LookAtVector = DirectX.Vector3(0, 0, -1)
        self.__Position = DirectX.Vector3(0, 0, 0)
    
    def __UpdateMatrix(self):
        self.Matrix = DirectX.Matrix.LookAtLH(self.__Position, self.__LookAtVector, DirectX.Vector3(0, 1, 0))
    
    def __GetLookAtVector(self):
        return self.__LookAtVector
    
    def __SetLookAtVector(self, v):
        v = Vectorize(v)
        self.__LookAtVector = v
        self.__UpdateMatrix()
    
    def __GetPosition(self):
        return self.__Position
    
    def __SetPosition(self, pos):
        pos = Vectorize(pos)
        self.__Position = pos
        self.__UpdateMatrix()
    
    Position = property(__GetPosition, __SetPosition)
    LookAtVector = property(__GetLookAtVector, __SetLookAtVector)
    
    def LookAt(self, loc):
        self.LookAtVector = loc
    
    def GetViewMatrix(self):
        return self.Matrix
        

class MeshRenderable(SceneObject):
    def __init__(self, device, name, file):
        SceneObject.__init__(self)
        self.Name = name
        materials = clr.Reference[System.Array[Direct3D.ExtendedMaterial]](())
        self.Mesh = Direct3D.Mesh.FromFile(file,
                                           Direct3D.MeshFlags.SystemMemory,
                                           device, materials)
        materials = materials.Value                                   
        
        self.Materials = []
        self.Textures = []
        for i in range(materials.Length):
            # load material, set color
            material = materials[i].Material3D
            material.AmbientColor = material.DiffuseColor
            
            # load texture, if possible
            texture = None
            texFile = materials[i].TextureFilename
            if texFile is not None and texFile.Length:
                texture = Direct3D.TextureLoader.FromFile(device, texFile)
                
            
            # insert the material and texture into the lists
            self.Materials.append(material)
            self.Textures.append(texture)
    
    def GetWorldMatrix(self):
        return self.Matrix

class BasicObject(SceneObject):
    ObjectTypes = {'cylinder' : Direct3D.Mesh.Cylinder,
                   'polygon' : Direct3D.Mesh.Polygon,
                   'sphere' : Direct3D.Mesh.Sphere,
                   'teapot' : Direct3D.Mesh.Teapot,
                   'torus' : Direct3D.Mesh.Torus,
                   'box' : Direct3D.Mesh.Box
                   }
    def __init__(self, device, type, name, color, *params):
        SceneObject.__init__(self)
        if not callable(type):
            type = self.ObjectTypes[type]
        self.Name = name
        self.Mesh = type(device, *params)
        
        color = Direct3D.ColorValue.FromColor(color)
        material = Direct3D.Material()
        material.AmbientColor = color

        self.Materials = [material]
        self.Textures = []
    
    def GetWorldMatrix(self):
        return self.Matrix


class RenderWindow(Forms.Form):
    def __init__(self, sceneManager):
        self.SceneManager = sceneManager
        self.Text = "IronPython Direct3D"
        self.ClientSize = Drawing.Size(640, 480)
        

    def OnKeyPress(self, args):
        if int(args.KeyChar) == int(System.Windows.Forms.Keys.Escape):
            self.Dispose()
            

    def OnResize(self, args):
        self.SceneManager.Paused = not self.Visible or \
            (self.WindowState == Forms.FormWindowState.Minimized)


class SceneManager(object):
    def __init__(self):
        self.Device = None
        self.Paused = False
        self.Background = System.Drawing.Color.Black
        self.ActiveCamera = None
        self.Objects = {}
        self.Listeners = {"OnFrame" : [],
                          "OnSceneBegin" : [],
                          "OnSceneCreate" : []}
        
    def LoadBasicObject(self, type, name, color, *args):
        mesh = BasicObject(self.Device, type, name, color, *args)
        self.Objects[mesh.Name] = mesh
        return mesh
            
    def LoadMesh(self, name, filename):
        mesh = MeshRenderable(self.Device, name, filename)
        self.Objects[mesh.Name] = mesh
        return mesh

    def InitGraphics(self, handle):
        params = Direct3D.PresentParameters()
        params.Windowed = True
        params.SwapEffect = Direct3D.SwapEffect.Discard
        params.EnableAutoDepthStencil = True
        params.AutoDepthStencilFormat = Direct3D.DepthFormat.D16

        self.Device = Direct3D.Device(0, Direct3D.DeviceType.Hardware, handle,
                                      Direct3D.CreateFlags.SoftwareVertexProcessing,
                                      params)

        self.Device.RenderState.ZBufferEnable = True
        self.Device.Transform.Projection = DirectX.Matrix.PerspectiveFovLH(System.Math.PI/4.0, 1, 1, 100)
        self.Device.Transform.View = DirectX.Matrix.LookAtLH(DirectX.Vector3(0, 3, -5), DirectX.Vector3(0, 0, 0), DirectX.Vector3(0, 1, 0))
        self.Device.RenderState.Ambient = Drawing.Color.White

        # ensure we are not paused
        self.Paused = False
        return True

    def Render(self):
        if self.Device is None or self.Paused:
            return
        
        if self.ActiveCamera is not None:
            self.Device.Transform.View = self.ActiveCamera.GetViewMatrix()
        self.Device.Clear(Direct3D.ClearFlags.Target | Direct3D.ClearFlags.ZBuffer,
                          self.Background, 1, 0)
        self.Device.BeginScene()
        
        for mesh in (x for x in self.Objects.Values if hasattr(x, "GetWorldMatrix")):
            if callable(mesh.GetWorldMatrix):
                self.Device.Transform.World = mesh.GetWorldMatrix()
                
                materials = mesh.Materials
                textures = mesh.Textures
                
                for i in range(len(materials)):
                    self.Device.Material = materials[i]

                    if i < len(textures):
                        self.Device.SetTexture(0, textures[i])
                    else:
                        self.Device.SetTexture(0, None)

                    mesh.Mesh.DrawSubset(i)


        self.Device.EndScene()
        self.Device.Present()

    def Go(self, window):
        self.__FireEvent("OnSceneCreate", self)
        self.__FireEvent("OnSceneBegin", self)
        lastTick = System.Environment.TickCount
        
        while window.Created:
            currTick = System.Environment.TickCount
            self.__FireEvent("OnFrame", (currTick-lastTick)/1000.0)
            lastTick = currTick
            
            self.Render()
            Forms.Application.DoEvents()
    
    def AddListenerByName(self, key, function):
        if not callable(function):
            raise TypeError, "Object not callable."
        self.Listeners[key].append(function)

    def AddListener(self, obj):
        for key in self.Listeners.Keys:
            if hasattr(obj, str(key)):
                function = getattr(obj, str(key))
                self.AddListenerByName(key, function)

    def __FireEvent(self, event, arg):
        listeners = self.Listeners[event]
        toRemove = []
        for f in listeners:
            if f(arg):
                toRemove.Add(f)
        
        for f in toRemove:
            listeners.Remove(f)

    def CreateCamera(self, name):
        cam = Camera(name)
        self.Objects[cam.Name] = cam
        
        if self.ActiveCamera is None:
            self.ActiveCamera = cam
        
        return cam        


class SceneCreator(object):
    def OnSceneCreate(self, sm):
        self.Tiger = sm.LoadMesh("Tiger", "tiger.x")
        #teapot = sm.LoadBasicObject("teapot", "teapot", Drawing.Color.White)
        
        cam = sm.CreateCamera("Player Cam")
        cam.Position = [0, 3, -5]
        #cam.LookAt(teapot.Position)
        
        cam.LookAt(self.Tiger.Position)
        sm.AddListener(TigerAnimator())
        return True
                

class TigerAnimator(object):
    def OnSceneBegin(self, sceneManager):
        self.Tiger = sceneManager.Objects["Tiger"]
        return True
        
    def OnFrame(self, elapsed):
        self.Tiger.Yaw(elapsed)


class Root(object):
    __State = {}
    def __init__(self):
        self.__dict__ = self.__State

    def ThreadMain(self, listeners = []):
        import thread
        args = (listeners,)
        thread.start_new_thread(self.Main, args)
        
    def Main(self, listeners = []):
        self.SceneManager = SceneManager()
        self.Window = RenderWindow(self.SceneManager)

        if not self.SceneManager.InitGraphics(self.Window.Handle):
            Forms.MessageBox.Show("Could not init Direct3D.")

        else:
            for obj in listeners:
                if callable(obj):
                    obj = obj()
                self.SceneManager.AddListener(obj)
            
            self.Window.Show()
            self.SceneManager.Go(self.Window)

if __name__ == '__main__':
    Root().Main([SceneCreator])

