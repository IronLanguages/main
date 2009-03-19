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
# texgen.c
# This program draws a texture mapped teapot with 
# automatically generated texture coordinates.  The
# texture is rendered as stripes on the teapot.
# Initially, the object is drawn with texture coordinates
# based upon the object coordinates of the vertex
# and distance from the plane x = 0.  Pressing the 'e'
# key changes the coordinate generation to eye coordinates
# of the vertex.  Pressing the 'o' key switches it back
# to the object coordinates.  Pressing the 's' key 
# changes the plane to a slanted one (x + y + z = 0).
# Pressing the 'x' key switches it back to x = 0.

require 'opengl'
require 'mathn'
include Gl,Glu,Glut

StripeImageWidth =  32
$stripeImage = []

$texName

def makeStripeImage
	for j in (0..StripeImageWidth-1)
		$stripeImage[4*j] = if (j<=4) then 255 else 0 end
		$stripeImage[4*j+1] = if (j>4) then 255 else 0 end
		$stripeImage[4*j+2] = 0
		$stripeImage[4*j+3] = 255
	end
end

# planes for texture coordinate generation
$xequalzero = [1.0, 0.0, 0.0, 0.0]
$slanted = [1.0, 1.0, 1.0, 0.0]
$currentCoeff = nil
$currentPlane = nil
$currentGenMode = nil

def init
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glEnable(GL_DEPTH_TEST)
	glShadeModel(GL_SMOOTH)
	
	makeStripeImage()
	glPixelStorei(GL_UNPACK_ALIGNMENT, 1)
	
	$texName = glGenTextures(1)
	glBindTexture(GL_TEXTURE_2D, $texName[0])
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT)
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR)
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR)
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, StripeImageWidth, 1, 0,
		GL_RGBA, GL_UNSIGNED_BYTE, $stripeImage.pack("C*"))
	glTexEnvf(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE)
	$currentCoeff = $xequalzero
	$currentGenMode = GL_OBJECT_LINEAR
	$currentPlane = GL_OBJECT_PLANE
	glTexGen(GL_S, GL_TEXTURE_GEN_MODE, $currentGenMode)
	glTexGen(GL_S, $currentPlane, $currentCoeff)
	
	glEnable(GL_TEXTURE_GEN_S)
	glEnable(GL_TEXTURE_2D)
	glEnable(GL_CULL_FACE)
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
	glEnable(GL_AUTO_NORMAL)
	glEnable(GL_NORMALIZE)
	glFrontFace(GL_CW)
	glCullFace(GL_BACK)
	glMaterial(GL_FRONT, GL_SHININESS, 64.0)
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	glPushMatrix()
	glRotate(45.0, 0.0, 0.0, 1.0)
	glBindTexture(GL_TEXTURE_2D, $texName[0])
	glutSolidTeapot(2.0)
	glPopMatrix()
	glutSwapBuffers()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= h)
		glOrtho(-3.5, 3.5, -3.5*h/w,3.5*h/w, -3.5, 3.5)
	else
		glOrtho(-3.5*w/h, 3.5*w/h, -3.5, 3.5, -3.5, 3.5)
	end
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?e,?E
			$currentGenMode = GL_EYE_LINEAR
			$currentPlane = GL_EYE_PLANE
			glTexGen(GL_S, GL_TEXTURE_GEN_MODE, $currentGenMode)
			glTexGen(GL_S, $currentPlane, $currentCoeff)
			glutPostRedisplay()
		when ?o, ?O
			$currentGenMode = GL_OBJECT_LINEAR
			$currentPlane = GL_OBJECT_PLANE
			glTexGen(GL_S, GL_TEXTURE_GEN_MODE, $currentGenMode)
			glTexGen(GL_S, $currentPlane, $currentCoeff)
			glutPostRedisplay()
		when ?s,?S
			$currentCoeff = $slanted
			glTexGen(GL_S, $currentPlane, $currentCoeff)
			glutPostRedisplay()
		when ?x,?X
			$currentCoeff = $xequalzero
			glTexGen(GL_S, $currentPlane, $currentCoeff)
			glutPostRedisplay()
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
glutKeyboardFunc(keyboard)
glutMainLoop()
