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
        pass

    def InitGraphics(self, handle):
        return True

    def Render(self):
        pass

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

