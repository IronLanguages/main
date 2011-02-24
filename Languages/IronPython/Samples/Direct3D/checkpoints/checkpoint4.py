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


class MeshRenderable(object):
    def __init__(self, device, name, file):
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
        return DirectX.Matrix.Identity
        

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
        self.Objects = {}
        self.Listeners = {"OnFrame" : [],
                          "OnSceneBegin" : [],
                          "OnSceneCreate" : []}
        
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


class SceneCreator(object):
    def OnSceneCreate(self, sm):
        self.Tiger = sm.LoadMesh("Tiger", "tiger.x")
        return True


def main(listeners = [SceneCreator]):
    sceneManager = SceneManager()
    window = RenderWindow(sceneManager)

    if not sceneManager.InitGraphics(window.Handle):
        Forms.MessageBox.Show("Could not init Direct3D.")

    else:
        for obj in listeners:
            if callable(obj):
                obj = obj()
            sceneManager.AddListener(obj)
        
        window.Show()
        sceneManager.Go(window)

if __name__ == '__main__':
    main()
