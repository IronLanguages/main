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
# alpha3D.c
# This program demonstrates how to intermix opaque and
# alpha blended polygons in the same scene, by using 
# glDepthMask.  Press the 'a' key to animate moving the 
# transparent object through the opaque object.  Press 
# the 'r' key to reset the scene.
require 'opengl'
include Gl,Glu,Glut

MAXZ=1.0
MINZ=-2.0
ZINC=0.1

$solidZ = MAXZ
$transparentZ = MINZ
$sphereList=nil
$cubeList=nil

def init
	mat_specular = [ 1.0, 1.0, 1.0, 0.15 ]
	mat_shininess = [ 100.0 ]
	position = [ 0.5, 0.5, 1.0, 0.0 ]
	
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, mat_shininess)
	glLight(GL_LIGHT0, GL_POSITION, position)
	
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
	glEnable(GL_DEPTH_TEST)
	
	$sphereList = glGenLists(1)
	glNewList($sphereList, GL_COMPILE)
	glutSolidSphere(0.4, 16, 16)
	glEndList()
	
	$cubeList = glGenLists(1)
	glNewList($cubeList, GL_COMPILE)
	glutSolidCube(0.6)
	glEndList()
end

display = proc do
	mat_solid = [ 0.75, 0.75, 0.0, 1.0 ]
	mat_zero = [ 0.0, 0.0, 0.0, 1.0 ]
	mat_transparent = [ 0.0, 0.8, 0.8, 0.6 ]
	mat_emission = [ 0.0, 0.3, 0.3, 0.6 ]
	
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	
	glPushMatrix()
	glTranslate(-0.15, -0.15, $solidZ)
	glMaterial(GL_FRONT, GL_EMISSION, mat_zero)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_solid)
	glCallList($sphereList)
	glPopMatrix()
	
	glPushMatrix()
	glTranslate(0.15, 0.15, $transparentZ)
	glRotate(15.0, 1.0, 1.0, 0.0)
	glRotate(30.0, 0.0, 1.0, 0.0)
	glMaterial(GL_FRONT, GL_EMISSION, mat_emission)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_transparent)
	glEnable(GL_BLEND)
	glDepthMask(GL_FALSE)
	glBlendFunc(GL_SRC_ALPHA, GL_ONE)
	glCallList($cubeList)
	glDepthMask(GL_TRUE)
	glDisable(GL_BLEND)
	glPopMatrix()
	
	glutSwapBuffers()
end

reshape = proc do |w, h|
	glViewport(0, 0,  w,  h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= h)
		glOrtho(-1.5, 1.5, -1.5*h.to_f/w, 1.5*h.to_f/w, -10.0, 10.0)
	else
		glOrtho(-1.5*w.to_f/h, 1.5*w.to_f/h, -1.5, 1.5, -10.0, 10.0)
	end
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

animate = proc do
	if ($solidZ <= MINZ || $transparentZ >= MAXZ)
		glutIdleFunc(nil)
	else
		$solidZ -= ZINC
		$transparentZ += ZINC
		glutPostRedisplay()
	end
end

keyboard = proc do |key, x, y|
	case (key)
		when ?a,?A
			$solidZ = MAXZ
			$transparentZ = MINZ
			glutIdleFunc(animate)
		when ?r, ?R
			$solidZ = MAXZ
			$transparentZ = MINZ
			glutPostRedisplay()
		when ?\e
			exit(0)
	end
end

glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutReshapeFunc(reshape)
glutKeyboardFunc(keyboard)
glutDisplayFunc(display)
glutMainLoop()
