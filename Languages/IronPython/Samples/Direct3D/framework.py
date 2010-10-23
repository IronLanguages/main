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

# This file is an updated version of the framework which is built during
# the course of the tutorial.  While most of it remains the same, I have
# updated a few functions, added a few new ones, and added documentation
# throughout the file.  If you are looking for an exact copy of what would
# you should end up with after working through the tutorial, find
# checkpoint6.py, which should have been included with this tutorial.

import clr
clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("System.Windows.Forms")
clr.AddReferenceByPartialName("Microsoft.DirectX")
clr.AddReferenceByPartialName("Microsoft.DirectX.Direct3D")
clr.AddReferenceByPartialName("Microsoft.DirectX.Direct3DX")

import operator
import math
import thread
import System
from System import Drawing
from System.Windows import Forms
from Microsoft import DirectX
from Microsoft.DirectX import Direct3D

single = System.Single

def Degree(degrees):
    """All functions take in radians, so to use degrees, you must pass it
    through this conversion function."""
    return (math.pi * degrees) / 180.0


def Radian(radians):
    """All functions take in radians, so this function does nothing...other
    than explicitly state that you are using radians."""
    return radians


def Vectorize(v):
    "Converts v from a sequence into a Vector3."
    if operator.isSequenceType(v):
        v = DirectX.Vector3(System.Single(v[0]), System.Single(v[1]), System.Single(v[2]))

    return v
    

def GetFunctionName(f):
    "Creates a formatted function string for display."
    try:
        name = f.__name__
        if hasattr(f, "im_class"):
            name = f.im_class.__name__ + "." + name
        return name
    except:
        # We do NOT want to throw an exception from this function...
        # If anything goes wrong we just want to quietly return a string.
        return ""


def HandleException(desc, exception):
    """This pops up Windows.Forms message box in a seperate thread
    containing the exception that occurred.  This framework tries its
    best to avoid exceptions by removing the offending listener/scene
    object whenever it throws an exception.  This may not be the best
    way to handle exceptions if this were used as a full application,
    but it does a reasonably good job of keeping the framework running
    (and not dieing due to an unhandled exception)."""
    args = str(desc) + "\n" + str(exception), "An exception occurred!"
    thread.start_new_thread(Forms.MessageBox.Show, args)


# Hack: I wanted this tutorial to run under IronPython without having to install
#       CPython along side it.  This code should really be replaced with a
#       reference to the types module in CPython:
#           from types import ClassType
class _TestClass: pass
ClassType = type(_TestClass)
del _TestClass


class RotatableObject(object):
    "Functions for rotating objects.  All units are in radians."
    def __init__(self):
        self.RotationMatrix = DirectX.Matrix.Identity
    
    def ResetOrientation(self):
        self.RotationMatrix = DirectX.Matrix.Identity
    
    def Pitch(self, p):
        "Rotation around the X axis"
        self.RotationMatrix *= DirectX.Matrix.RotationX(p)
    
    def Yaw(self, y):
        "Rotation around the Y axis"
        self.RotationMatrix *= DirectX.Matrix.RotationY(y)
    
    def Roll(self, r):
        "Rotation around the Z axis"
        self.RotationMatrix *= DirectX.Matrix.RotationZ(r)


class PositionableObject(object):
    "Functions for positioning objects."
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
    "A combined position/rotation class for easy access."
    def __init__(self):
        self.Matrix = DirectX.Matrix.Identity
        self.__PositionMatrix = DirectX.Matrix.Identity
        self.__RotationMatrix = DirectX.Matrix.Identity
        self.Invert = False
        
        PositionableObject.__init__(self)
        RotatableObject.__init__(self)
    
    def __GetPositionMatrix(self):
        return self.__PositionMatrix
    
    def __SetPositionMatrix(self, value):
        self.__PositionMatrix = value
        self.Matrix = self.__RotationMatrix * self.__PositionMatrix
        if self.Invert:
            self.Matrix.Invert()
    
    def __GetRotationMatrix(self):
        return self.__RotationMatrix
    
    def __SetRotationMatrix(self, value):
        self.__RotationMatrix = value
        self.Matrix = self.__RotationMatrix * self.__PositionMatrix
        if self.Invert:
            self.Matrix.Invert()
        
    PositionMatrix = property(__GetPositionMatrix, __SetPositionMatrix)
    RotationMatrix = property(__GetRotationMatrix, __SetRotationMatrix)


class Camera(SceneObject):
    def __init__(self, name):
        SceneObject.__init__(self)
        self.Name = name
        self.Invert = True
        self.LookAtMatrix = DirectX.Matrix.Identity

    def LookAt(self, position, up=DirectX.Vector3(0, 1, 0)):
        """Takes in a position to look at and an up vector to use to look at
        it with.  The up vector will normally be the positive Y axis, but in
        the case where the position is directly above or below the position
        of the camera, you will need to set which direction up is in this
        case."""
        zero = DirectX.Vector3(0, 0, 0)
        direction = Vectorize(position) - self.Position
        up = Vectorize(up)
        self.LookAtMatrix = DirectX.Matrix.LookAtLH(zero, direction, up)
        self.RotationMatrix = DirectX.Matrix.Identity
        
    def ResetOrientation(self):
        self.RotationMatrix = DirectX.Matrix.Identity
        self.LookAtMatrix = DirectX.Matrix.Identity
        
    def GetViewMatrix(self):
        return self.Matrix * self.LookAtMatrix


class MeshRenderable(SceneObject):
    "A scene object which represents a mesh."
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
    "A scene object for basic meshes which are provided by DirectX"
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
            type = self.ObjectTypes[str(type).lower()]
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
    """A simple Forms window to host the app.  This should really be changed to
    allow for different resolutions."""
    def __init__(self, sceneManager):
        self.SceneManager = sceneManager
        self.Text = "IronPython Direct3D"
        self.ClientSize = Drawing.Size(640, 480)
        self.InputManager = None
        
    def OnKeyDown(self, args):
        if self.InputManager is not None:
            self.InputManager.InjectKeyUp(args.KeyCode)
            
    def OnKeyUp(self, args):
        if self.InputManager is not None:
            self.InputManager.InjectKeyDown(args.KeyCode)
            
    def OnKeyPress(self, args):
        if self.InputManager is not None:
            self.InputManager.InjectKey(args.KeyChar)
            

    def OnResize(self, args):
        self.SceneManager.Paused = not self.Visible or \
            (self.WindowState == Forms.FormWindowState.Minimized)

class InputManager(object):
    def __init__(self):
        self.KeyListeners = {'OnKeyDown' : [],
                             'OnKeyUp' : [],
                             'OnKey' : []
                             }

    def AddEventListener(self, event, listener):
        if not callable(listener):
            raise TypeError, "listener must be callable"
        self.KeyListeners[event].Add(listener)
        
    def AddListener(self, listener):
        if operator.isSequenceType(listener):
            for obj in listener:
                self.AddKeyListener(obj)

        else:
            if issubclass(type(listener), type) or type(listener) == ClassType:
                listener = listener()

            for key in self.KeyListeners.Keys:
                if hasattr(listener, key):
                    self.KeyListeners[key].Add(getattr(listener, key))


    def RemoveListener(self, listener):
        if operator.isSequenceType(listener):
            for obj in listener:
                self.RemoveKeyListener(obj)

        else:
            if issubclass(type(listener), type) or type(listener) == ClassType:
                for l in self.Listeners.Values:
                    for func in l:
                        if func.im_class == listener:
                            l.Remove(func)
                            
        for f in self.Listeners[event]:
            if f.im_class == type:
                self.Listeners[event].Remove(f)
            else:
                for key in self.KeyListeners.Keys:
                    if hasattr(listener, key):
                        self.KeyListeners[key].Remove(getattr(listener, key))

    def __FireKeyEvent(self, seq, key):
        for f in seq:
            remove = False
            try:
                remove = f(key)
            except Exception, e:
                HandleException("An exception occurred while calling the KeyListener: " + GetFunctionName(f) + ".", e)
                remove = True

            if remove:
                try:
                    seq.Remove(f)
                except:
                    pass

    def InjectKey(self, key):
        self.__FireKeyEvent(self.KeyListeners['OnKey'], key)

    def InjectKeyDown(self, key):
        self.__FireKeyEvent(self.KeyListeners['OnKeyUp'], key)

    def InjectKeyUp(self, key):
        self.__FireKeyEvent(self.KeyListeners['OnKeyDown'], key)
        
class SceneManager(object):
    "The class which manages and renders all objects in the scene."
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
        "Creates a basic object from the given paramters."
        mesh = BasicObject(self.Device, type, name, color, *args)
        self.Objects[mesh.Name] = mesh
        return mesh
        
            
            
    def LoadMesh(self, name, filename):
        "Loads a mesh and places it into an object based on the filename."
        mesh = MeshRenderable(self.Device, name, filename)
        self.Objects[mesh.Name] = mesh
        return mesh
        


    def CreateCamera(self, name):
        "Creates a Camera with the given name."
        cam = Camera(name)
        self.Objects[cam.Name] = cam
        
        if self.ActiveCamera is None:
            self.ActiveCamera = cam
        
        return cam
    

    def InitGraphics(self, handle):
        """Creates the Direct3D device which is used to render the scene.  Pops
        up a message box and returns False on failure (returns True on success).
        """
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
                try:
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
                
                except Exception, e:
                    # We will only handle this if the mesh has a Name attribute (all meshes are supposed to
                    # have a Name, but it's possible we have been given a bogus object keyed to something
                    # other than its name).  This keeps us from infinitely poping up error messages if we
                    # actually cannot find the offending object to remove.
                    if hasattr(mesh, "Name"):
                        name = str(mesh.Name)

                        if name in self.Objects:
                            HandleException("An exception occurred while trying to render object " + name + ".", e)
                            del self.Objects[name]

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
    
    def AddEventListener(self, event, function):
        if not callable(function):
            raise TypeError, "Listeners must be callable."
        self.Listeners[event].append(function)
        

    def RemoveListenerByInstance(self, event, function):
        try:
            self.Listeners[event].Remove(function)
            
        except:
            # We will ignore any errors here...  This could potentially throw
            # exceptions if we ask to remove the function after the function
            # has already been removed.
            pass


    def RemoveListenerByType(self, event, type):
        for f in self.Listeners[event]:
            if hasattr(f, 'im_class'):
                if f.im_class == type:
                    self.Listeners[event].Remove(f)

    def RemoveListener(self, obj):
        """Takes in either a class or an instance of a class.  If obj is a class
        then this removes all instances of that class from all events.  If the
        obj parameter is an instance then it removes only that instance from all
        events it is registered for.  If obj is a sequence type then RemoveListener
        will be recursively called with the contents of the list (do NOT pass
        recursive sequences to this function or you will end up with an infinite
        loop)."""
        if operator.isSequenceType(obj):
            for x in obj:
                self.RemoveListener(x)
        else:
            for key in self.Listeners.Keys:
                self.RemoveListenerByInstance(key, obj)
                key = str(key)
                if hasattr(obj, key):
                    if issubclass(type(obj), type) or type(obj) == ClassType:
                        self.RemoveListenerByType(key, obj)
                    else:
                        f = getattr(obj, key)
                        self.RemoveListenerByInstance(key, f)
            

    def AddListener(self, obj):
        """Registers an object for all listeners that it has functions for.  If
        obj is an instance of a class, this will register that instance.  If obj
        is a class, then this object will attempt to construct an instance of it
        (without any parameters to the constructor) and then register that
        instance (this throws an exception if the constructor required args).
        Finally if obj is a sequence type this function will recursively call
        itself with the contents of the sequence.  Do not pass in a recursive
        list to this method."""
        if operator.isSequenceType(obj):
            for x in obj:
                self.AddListener(x)
                
        else:
            if issubclass(type(obj), type) or type(obj) == ClassType:
                obj = obj()

            for key in self.Listeners.Keys:
                if hasattr(obj, str(key)):
                    function = getattr(obj, str(key))
                    self.AddEventListener(key, function)

    def __FireEvent(self, event, arg):
        listeners = self.Listeners[event]
        for f in listeners:
            remove = False
            try:
                remove = f(arg)
            except Exception, e:
                HandleException("An exception occurred while firing an event for " + GetFunctionName(f) + ":", e)
                remove = True
            
            if remove:
                listeners.Remove(f)



class Root(object):
    """A borg class which keeps track of the RenderWindow and SceneManager
    classes."""
    __State = {}
    def __init__(self):
        self.__dict__ = self.__State

    def Init(self):
        self.SceneManager = SceneManager()
        self.Window = RenderWindow(self.SceneManager)
        self.InputManager = InputManager()
        self.Window.InputManager = self.InputManager

        # Register to handle close message
        self.InputManager.AddListener(self)
        self.Initialized = True
        
    def Main(self, listeners = []):
        """Runs the application, registering the list of listeners given by the
        listeners parameter.  Listeners can be an instance of a Listener class,
        it could be a class object of a listener class (in which case an
        instance will be created by calling the constructor with no params),
        or it could be a list of either of those."""
        if not hasattr(self, "Initialized"):
            self.Init()

        if not self.SceneManager.InitGraphics(self.Window.Handle):
            Forms.MessageBox.Show("Could not init Direct3D.")

        else:
            if listeners is not None:
                self.SceneManager.AddListener(listeners)
            
            self.Window.Show()
            self.SceneManager.Go(self.Window)

    def ThreadMain(self, listeners = []):
        "Starts main in a background thread."
        if not hasattr(self, "Initialized"):
            self.Init()
        
        args = (listeners,)
        thread.start_new_thread(self.Main, args)

    def OnKeyDown(self, key):
        if key == Forms.Keys.Escape:
            self.Window.Dispose()
            return True

if __name__ == '__main__':
    Forms.MessageBox.Show("The framework should not be run directly.\nRun one of the demos included with the tutorial instead.", "Error")
