#
# Copyright (c) 1993-1997, Silicon Graphics, Inc.
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
# OpenGL(R) is a registered trademark of Silicon Graphics, Inc.
#
# double.c
# This is a simple double buffered program.
# Pressing the left mouse button rotates the rectangle.
# Pressing the middle mouse button stops the rotation.
require 'opengl'
require 'rational'
include Gl,Glu,Glut

$spin = 0.0

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT)
	glPushMatrix()
	glRotate($spin, 0.0, 0.0, 1.0)
	glColor(1.0, 1.0, 1.0)
	glRect(-25.0, -25.0, 25.0, 25.0)
	glPopMatrix()
	glutSwapBuffers()
end

$spinDisplay = Proc.new do
	$spin = $spin + 4.0
	$spin = $spin - 360.0 if ($spin > 360.0) 
	glutPostRedisplay()
end

def init
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glShadeModel(GL_FLAT)
end

reshape = Proc.new do |w, h|
	glViewport(0, 0, w,  h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	glOrtho(-50.0, 50.0, -50.0, 50.0, -1.0, 1.0)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

mouse = Proc.new do |button, state, x, y|
	case button
		when GLUT_LEFT_BUTTON
			glutIdleFunc($spinDisplay) if (state == GLUT_DOWN)
		when GLUT_MIDDLE_BUTTON
			glutIdleFunc(nil) if (state == GLUT_DOWN)
	end
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0)
	end
end
   
#  Request double buffer display mode.
#  Register mouse input callback functions
glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(500, 500) 
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutDisplayFunc(display) 
glutReshapeFunc(reshape) 
glutKeyboardFunc(keyboard)
glutMouseFunc(mouse)
glutMainLoop()
