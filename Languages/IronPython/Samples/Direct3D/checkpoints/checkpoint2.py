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

import System
from System import Drawing
from System.Windows import Forms
from Microsoft import DirectX
from Microsoft.DirectX import Direct3D

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
        self.Device.RenderState.Ambient = Drawing.Color.White


        self.Device.Transform.Projection = DirectX.Matrix.PerspectiveFovLH(System.Math.PI/4.0, 1, 1, 100)
        self.Device.Transform.View = DirectX.Matrix.LookAtLH(DirectX.Vector3(0, 3, -5), DirectX.Vector3(0, 0, 0), DirectX.Vector3(0, 1, 0))


        # ensure we are not paused
        self.Paused = False

        return True


    def Render(self):
        if self.Device is None or self.Paused:
            return
        
        self.Device.Clear(Direct3D.ClearFlags.Target | Direct3D.ClearFlags.ZBuffer, \
            self.Background, 1, 0)
        self.Device.BeginScene()

        # scene render here

        self.Device.EndScene()
        self.Device.Present()


    def Go(self, window):
        while window.Created:
            self.Render()
            Forms.Application.DoEvents()

def main():
    sceneManager = SceneManager()
    window = RenderWindow(sceneManager)

    if not sceneManager.InitGraphics(window.Handle):
        Forms.MessageBox.Show("Could not init Direct3D.")

    else:
        window.Show()
        sceneManager.Go(window)

if __name__ == '__main__':
    main()

