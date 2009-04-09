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
# accpersp.c
# Use the accumulation buffer to do full-scene antialiasing
# on a scene with perspective projection, using the special
# routines accFrustum() and accPerspective().

require 'opengl'
include Gl,Glu,Glut

$j8 = [
	[-0.334818,  0.435331],
	[ 0.286438, -0.393495],
	[ 0.459462,  0.141540],
	[-0.414498, -0.192829],
	[-0.183790,  0.082102],
	[-0.079263, -0.317383],
	[ 0.102254,  0.299133],
	[ 0.164216, -0.054399]
		]

# accFrustum()
# The first 6 arguments are identical to the glFrustum() call.
#  
# pixdx and pixdy are anti-alias jitter in pixels. 
# Set both equal to 0.0 for no anti-alias jitter.
# eyedx and eyedy are depth-of field jitter in pixels. 
# Set both equal to 0.0 for no depth of field effects.
#
# focus is distance from eye to plane in focus. 
# focus must be greater than, but not equal to 0.0.
#
# Note that accFrustum() calls glTranslatef().  You will 
# probably want to insure that your ModelView matrix has been 
# initialized to identity before calling accFrustum().

def accFrustum(left, right, bottom, top, nnear, ffar, pixdx, pixdy, eyedx, eyedy, focus)
   viewport = glGetDoublev(GL_VIEWPORT)
	
   xwsize = right - left
   ywsize = top - bottom
	
   dx = -(pixdx*xwsize/viewport[2] + eyedx*nnear/focus)
   dy = -(pixdy*ywsize/viewport[3] + eyedy*nnear/focus)
	
   glMatrixMode(GL_PROJECTION)
   glLoadIdentity()
   glFrustum(left + dx, right + dx, bottom + dy, top + dy, nnear, ffar)
   glMatrixMode(GL_MODELVIEW)
   glLoadIdentity()
   glTranslate(-eyedx, -eyedy, 0.0)
end

# accPerspective()
# 
# The first 4 arguments are identical to the gluPerspective() call.
# pixdx and pixdy are anti-alias jitter in pixels. 
# Set both equal to 0.0 for no anti-alias jitter.
# eyedx and eyedy are depth-of field jitter in pixels. 
# Set both equal to 0.0 for no depth of field effects.
#
# focus is distance from eye to plane in focus. 
# focus must be greater than, but not equal to 0.0.
#
# Note that accPerspective() calls accFrustum().

def  accPerspective(fovy, aspect, nnear, ffar, pixdx, pixdy, eyedx, eyedy, focus)
	fov2 = ((fovy*Math::PI) / 180.0) / 2.0
	
	top = nnear / (Math::cos(fov2) / Math::sin(fov2))
	bottom = -top
	
	right = top * aspect
	left = -right
	
	accFrustum(left, right, bottom, top, nnear, ffar,	pixdx, pixdy, eyedx, eyedy, focus)
end

#  Initialize lighting and other values.
def init
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
	glTranslate(0.0, 0.0, -5.0)
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

display = proc do
	viewport = glGetDoublev(GL_VIEWPORT)
	
	glClear(GL_ACCUM_BUFFER_BIT)
	for jitter in 0...ACSIZE
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
		accPerspective(50.0, viewport[2]/ viewport[3], 1.0, 15.0, $j8[jitter][0], $j8[jitter][1], 0.0, 0.0, 1.0)
		displayObjects()
		glAccum(GL_ACCUM, 1.0/ACSIZE)
	end
	glAccum(GL_RETURN, 1.0)
	glutSwapBuffers()
end

reshape = proc do |w, h|
   glViewport(0, 0,  w,  h)
end

keyboard = proc do |key, x, y|
   case (key)
   when ?\e
     exit(0)
   end
end

#  Main Loop
#  Be certain you request an accumulation buffer.

glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_ACCUM | GLUT_DEPTH)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutReshapeFunc(reshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()
