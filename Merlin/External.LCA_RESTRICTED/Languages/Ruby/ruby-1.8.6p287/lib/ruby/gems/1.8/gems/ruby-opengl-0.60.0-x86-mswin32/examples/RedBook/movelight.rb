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
#  movelight.c
#  This program demonstrates when to issue lighting and
#  transformation commands to render a model with a light
#  which is moved by a modeling transformation (rotate or
#  translate).  The light position is reset after the modeling
#  transformation is called.  The eye position does not change.
#
#  A sphere is drawn using a grey material characteristic.
#  A single light source illuminates the object.
#
#  Interaction:  pressing the left mouse button alters
#  the modeling transformation (x rotation) by 30 degrees.
#  The scene is then redrawn with the light in a new position.

require 'opengl'
require 'rational'
require 'mathn'
include Gl,Glu,Glut

$spin = 0

#  Initialize material property, light source, lighting model,
#  and depth buffer.
def init
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glShadeModel(GL_SMOOTH)
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
	glEnable(GL_DEPTH_TEST)
end

#  Here is where the light position is reset after the modeling
#  transformation (glRotated) is called.  This places the
#  light at a new position in world coordinates.  The cube
#  represents the position of the light.
display = Proc.new do
	position = [ 0.0, 0.0, 1.5, 1.0 ]
	
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	glPushMatrix()
	GLU.LookAt(0.0, 0.0, 5.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0)
	
	glPushMatrix()
	glRotate($spin, 1.0, 0.0, 0.0)
	glLight(GL_LIGHT0, GL_POSITION, position)
	
	glTranslate(0.0, 0.0, 1.5)
	glDisable(GL_LIGHTING)
	glColor(0.0, 1.0, 1.0)
	glutWireCube(0.1)
	glEnable(GL_LIGHTING)
	glPopMatrix()
	
	glutSolidTorus(0.275, 0.85, 8, 15)
	glPopMatrix()
	glutSwapBuffers()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0,  w,  h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	gluPerspective(40.0,  w/h, 1.0, 20.0)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

mouse = Proc.new do |button, state, x, y|
	case button
		when GLUT_LEFT_BUTTON
			if (state == GLUT_DOWN)
				$spin = ($spin + 30) % 360
				glutPostRedisplay()
			end
	end
end

keyboard = Proc.new do |key, x, y|
	case key
		when ?\e
			exit(0)
	end
end

# main
glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(500, 500) 
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutDisplayFunc(display) 
glutReshapeFunc(reshape)
glutMouseFunc(mouse)
glutKeyboardFunc(keyboard)
glutMainLoop()
