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
# accanti.c

require 'opengl'
require 'jitter'
include Gl,Glu,Glut

#  Initialize lighting and other values.
def myinit
	mat_ambient = [ 1.0, 1.0, 1.0, 1.0 ]
	mat_specular = [ 1.0, 1.0, 1.0, 1.0 ]
	light_position = [ 0.0, 0.0, 10.0, 1.0 ]
	lm_ambient = [ 0.2, 0.2, 0.2, 1.0 ]
	
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, 50.0)
	glLight(GL_LIGHT0, GL_POSITION, light_position)
	glLightModel(GL_LIGHT_MODEL_AMBIENT, lm_ambient)
	
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
	glDepthFunc(GL_LESS)
	glEnable(GL_DEPTH_TEST)
	glShadeModel(GL_FLAT)
	
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glClearAccum(0.0, 0.0, 0.0, 0.0)
end

def displayObjects
	torus_diffuse = [ 0.7, 0.7, 0.0, 1.0 ]
	cube_diffuse = [ 0.0, 0.7, 0.7, 1.0 ]
	sphere_diffuse = [ 0.7, 0.0, 0.7, 1.0 ]
	octa_diffuse = [ 0.7, 0.4, 0.4, 1.0 ]
	
	glPushMatrix()
	glRotate(30.0, 1.0, 0.0, 0.0)
	
	glPushMatrix()
	glTranslate(-0.80, 0.35, 0.0) 
	glRotate(100.0, 1.0, 0.0, 0.0)
	glMaterial(GL_FRONT, GL_DIFFUSE, torus_diffuse)
	glutSolidTorus(0.275, 0.85, 16, 16)
	glPopMatrix()
	
	glPushMatrix()
	glTranslate(-0.75, -0.50, 0.0)
	glRotate(45.0, 0.0, 0.0, 1.0)
	glRotate(45.0, 1.0, 0.0, 0.0)
	glMaterial(GL_FRONT, GL_DIFFUSE, cube_diffuse)
	glutSolidCube(1.5)
	glPopMatrix()
	
	glPushMatrix()
	glTranslate(0.75, 0.60, 0.0) 
	glRotate(30.0, 1.0, 0.0, 0.0)
	glMaterial(GL_FRONT, GL_DIFFUSE, sphere_diffuse)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	glPushMatrix()
	glTranslate(0.70, -0.90, 0.25)
	glMaterial(GL_FRONT, GL_DIFFUSE, octa_diffuse)
	glutSolidOctahedron()
	glPopMatrix()
	
	glPopMatrix()
end

ACSIZE=8

myDisplay = proc do
	viewport = glGetDoublev(GL_VIEWPORT)
	
	glClear(GL_ACCUM_BUFFER_BIT)
	for jitter in 0...ACSIZE
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
		glPushMatrix()
		#	Note that 4.5 is the distance in world space between
		#	left and right and bottom and top.
		#	This formula converts fractional pixel movement to 
		#	world coordinates.
		glTranslate($j8[jitter][0]*4.5/viewport[2], $j8[jitter][1]*4.5/viewport[3], 0.0)
		displayObjects()
		glPopMatrix()
		glAccum(GL_ACCUM, 1.0/ACSIZE)
	end
	glAccum(GL_RETURN, 1.0)
	glutSwapBuffers()
end

myReshape = proc do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= h) 
		glOrtho(-2.25, 2.25, -2.25*h/w, 2.25*h/w, -10.0, 10.0)
	else 
		glOrtho(-2.25*w/h, 2.25*w/h, -2.25, 2.25, -10.0, 10.0)
	end
	glMatrixMode(GL_MODELVIEW)
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0);
	end
end

# Main Loop
# Open window with initial window size, title bar, 
# RGBA display mode, and handle input events.
#
glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_ACCUM | GLUT_DEPTH)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
myinit()
glutReshapeFunc(myReshape)
glutDisplayFunc(myDisplay)
glutKeyboardFunc(keyboard)
glutMainLoop()
