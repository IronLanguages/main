#
# Copyright (c) Mark J. Kilgard, 1994.
#
# (c) Copyright 1993, Silicon Graphics, Inc.
# ALL RIGHTS RESERVED 
# Permission to use, copy, modify, and distribute this software for 
# any purpose and without fee is hereby granted, provided that the above
# copyright notice appear in all copies and that both the copyright notice
# and this permission notice appear in supporting documentation, and that 
# the name of Silicon Graphics, Inc. not be used in advertising
# or publicity pertaining to distribution of the software without specific,
# written prior permission. 
#
# THE MATERIAL EMBODIED ON THIS SOFTWARE IS PROVIDED TO YOU "AS-IS"
# AND WITHOUT WARRANTY OF ANY KIND, EXPRESS, IMPLIED OR OTHERWISE,
# INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF MERCHANTABILITY OR
# FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL SILICON
# GRAPHICS, INC.  BE LIABLE TO YOU OR ANYONE ELSE FOR ANY DIRECT,
# SPECIAL, INCIDENTAL, INDIRECT OR CONSEQUENTIAL DAMAGES OF ANY
# KIND, OR ANY DAMAGES WHATSOEVER, INCLUDING WITHOUT LIMITATION,
# LOSS OF PROFIT, LOSS OF USE, SAVINGS OR REVENUE, OR THE CLAIMS OF
# THIRD PARTIES, WHETHER OR NOT SILICON GRAPHICS, INC.  HAS BEEN
# ADVISED OF THE POSSIBILITY OF SUCH LOSS, HOWEVER CAUSED AND ON
# ANY THEORY OF LIABILITY, ARISING OUT OF OR IN CONNECTION WITH THE
# POSSESSION, USE OR PERFORMANCE OF THIS SOFTWARE.
# 
# US Government Users Restricted Rights 
# Use, duplication, or disclosure by the Government is subject to
# restrictions set forth in FAR 52.227.19(c)(2) or subparagraph
# (c)(1)(ii) of the Rights in Technical Data and Computer Software
# clause at DFARS 252.227-7013 and/or in similar or successor
# clauses in the FAR or the DOD or NASA FAR Supplement.
# Unpublished-- rights reserved under the copyright laws of the
# United States.  Contractor/manufacturer is Silicon Graphics,
# Inc., 2011 N.  Shoreline Blvd., Mountain View, CA 94039-7311.
#
# OpenGL(TM) is a trademark of Silicon Graphics, Inc.
#
# fog.c
# This program draws 5 red teapots, each at a different 
# z distance from the eye, in different types of fog.  
# Pressing the left mouse button chooses between 3 types of 
# fog:  exponential, exponential squared, and linear.  
# In this program, there is a fixed density value, as well 
# as fixed start and end values for the linear fog.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

$fogMode = 0

selectFog = Proc.new do |mode|
	if (mode == GL_LINEAR)
		glFog(GL_FOG_START, 1.0)
		glFog(GL_FOG_END, 5.0)
		# falls through
		glFog(GL_FOG_MODE, mode)
		glutPostRedisplay()
	end
	if (mode == GL_EXP2 || mode == GL_EXP)
		glFog(GL_FOG_MODE, mode)
		glutPostRedisplay()
	end
	exit(0) if (mode == 0)
end

# Initialize z-buffer, projection matrix, light source, 
# and lighting model.  Do not specify a material property here.
def myinit
	position = [0.0, 3.0, 3.0, 0.0]
	local_view = [0.0]
	
	glEnable(GL_DEPTH_TEST)
	glDepthFunc(GL_LESS)
	
	glLight(GL_LIGHT0, GL_POSITION, position)
	glLightModel(GL_LIGHT_MODEL_LOCAL_VIEWER, local_view)
	
	glFrontFace(GL_CW)
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
	glEnable(GL_AUTO_NORMAL)
	glEnable(GL_NORMALIZE)
	glEnable(GL_FOG)
	fogColor = [0.5, 0.5, 0.5, 1.0]
	
	fogMode = GL_EXP
	glFog(GL_FOG_MODE, fogMode)
	glFog(GL_FOG_COLOR, fogColor)
	glFog(GL_FOG_DENSITY, 0.35)
	glHint(GL_FOG_HINT, GL_DONT_CARE)
	glClearColor(0.5, 0.5, 0.5, 1.0)
end

def renderRedTeapot(x, y, z)
	mat = []
	glPushMatrix()
	glTranslate(x, y, z)
	mat[0] = 0.1745
	mat[1] = 0.01175
	mat[2] = 0.01175
	mat[3] = 1.0
	glMaterial(GL_FRONT, GL_AMBIENT, mat)
	mat[0] = 0.61424
	mat[1] = 0.04136
	mat[2] = 0.04136
	glMaterial(GL_FRONT, GL_DIFFUSE, mat)
	mat[0] = 0.727811
	mat[1] = 0.626959
	mat[2] = 0.626959
	glMaterial(GL_FRONT, GL_SPECULAR, mat)
	glMaterial(GL_FRONT, GL_SHININESS, 0.6 * 128.0)
	glutSolidTeapot(1.0)
	glPopMatrix()
end

# display() draws 5 teapots at different z positions.
display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	renderRedTeapot(-4.0, -0.5, -1.0)
	renderRedTeapot(-2.0, -0.5, -2.0)
	renderRedTeapot(0.0, -0.5, -3.0)
	renderRedTeapot(2.0, -0.5, -4.0)
	renderRedTeapot(4.0, -0.5, -5.0)
	glFlush()
end

myReshape = Proc.new do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= (h * 3))
		glOrtho(-6.0, 6.0, -2.0 * h * 3 / w, 2.0 * h * 3 / w, 0.0, 10.0)
	elsif
		glOrtho(-6.0 * w / h * 3, 6.0 * w / h * 3, -2.0, 2.0, 0.0, 10.0)
	end
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0)
	end
end

# Main Loop
# Open window with initial window size, title bar, 
# RGBA display mode, depth buffer, and handle input events.
glutInit
glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(800, 300)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
myinit()
glutReshapeFunc(myReshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutCreateMenu(selectFog)
glutAddMenuEntry("Fog EXP", GL_EXP)
glutAddMenuEntry("Fog EXP2", GL_EXP2)
glutAddMenuEntry("Fog LINEAR", GL_LINEAR)
glutAddMenuEntry("Quit", 0)
glutAttachMenu(GLUT_RIGHT_BUTTON)
glutMainLoop()
    