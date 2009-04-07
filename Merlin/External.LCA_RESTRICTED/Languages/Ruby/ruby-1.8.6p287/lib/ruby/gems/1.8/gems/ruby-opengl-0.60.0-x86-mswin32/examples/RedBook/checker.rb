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
# checker.c
# This program texture maps a checkerboard image onto
# two rectangles.  This program clamps the texture, if
# the texture coordinates fall outside 0.0 and 1.0.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

#	Create checkerboard texture
CheckImageWidth=64
CheckImageHeight=64
$checkImage = []

def makeCheckImage
	for i in 0..CheckImageWidth-1
		for j in 0..CheckImageHeight-1
			c = if ((i&0x8==0) != (j&0x8==0)) then 255 else 0 end
			$checkImage[(i+CheckImageWidth*j)*3+0] = c
			$checkImage[(i+CheckImageWidth*j)*3+1] = c
			$checkImage[(i+CheckImageWidth*j)*3+2] = c
		end
	end
end

def myinit
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glEnable(GL_DEPTH_TEST)
	glDepthFunc(GL_LESS)
	
	makeCheckImage()
	glPixelStore(GL_UNPACK_ALIGNMENT, 1)
	glTexImage2D(GL_TEXTURE_2D, 0, 3, CheckImageWidth, 
		CheckImageHeight, 0, GL_RGB, GL_UNSIGNED_BYTE, 
		$checkImage.pack("C*"))
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP)
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP)
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR)
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR)
	glTexEnv(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_DECAL)
	glEnable(GL_TEXTURE_2D)
	glShadeModel(GL_FLAT)
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	glBegin(GL_QUADS)
	glTexCoord(0.0, 0.0); glVertex(-2.0, -1.0, 0.0)
	glTexCoord(0.0, 1.0); glVertex(-2.0, 1.0, 0.0)
	glTexCoord(1.0, 1.0); glVertex(0.0, 1.0, 0.0)
	glTexCoord(1.0, 0.0); glVertex(0.0, -1.0, 0.0)
	
	glTexCoord(0.0, 0.0); glVertex(1.0, -1.0, 0.0)
	glTexCoord(0.0, 1.0); glVertex(1.0, 1.0, 0.0)
	glTexCoord(1.0, 1.0); glVertex(2.41421, 1.0, -1.41421)
	glTexCoord(1.0, 0.0); glVertex(2.41421, -1.0, -1.41421)
	glEnd()
	glutSwapBuffers()
end

myReshape = Proc.new do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	gluPerspective(60.0, 1.0*w/h, 1.0, 30.0)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
	glTranslate(0.0, 0.0, -3.6)
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0)
	end
end

glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow("checker")
myinit()
glutReshapeFunc(myReshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()
