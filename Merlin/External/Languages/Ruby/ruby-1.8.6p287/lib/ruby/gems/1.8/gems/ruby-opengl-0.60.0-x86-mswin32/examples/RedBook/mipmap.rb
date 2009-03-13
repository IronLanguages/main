#
# Copyright (c) Mark J. Kilgard, 1994. */
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
# mipmap.c
# This program demonstrates using mipmaps for texture maps.
# To overtly show the effect of mipmaps, each mipmap reduction
# level has a solidly colored, contrasting texture image.
# Thus, the quadrilateral which is drawn is drawn with several
# different colors.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

$mipmapImage32 = []
$mipmapImage16 = []
$mipmapImage8 = []
$mipmapImage4 = []
$mipmapImage2 = []
$mipmapImage1 = []

def makeImages
	for i in 0..31
		for j in 0..31
			$mipmapImage32[(j*32+i)*3+0] = 255
			$mipmapImage32[(j*32+i)*3+1] = 255
			$mipmapImage32[(j*32+i)*3+2] = 0
		end
	end
	for i in 0..15
		for j in 0..15
			$mipmapImage16[(j*16+i)*3+0] = 255
			$mipmapImage16[(j*16+i)*3+1] = 0
			$mipmapImage16[(j*16+i)*3+2] = 255
		end
	end
	for i in 0..7
		for j in 0..7
			$mipmapImage8[(j*8+i)*3+0] = 255
			$mipmapImage8[(j*8+i)*3+1] = 0
			$mipmapImage8[(j*8+i)*3+2] = 0
		end
	end
	for i in 0..3
		for j in 0..3
			$mipmapImage4[(j*4+i)*3+0] = 0
			$mipmapImage4[(j*4+i)*3+1] = 255
			$mipmapImage4[(j*4+i)*3+2] = 0
		end
	end
	for i in 0..1
		for j in 0..1
			$mipmapImage2[(j*2+i)*3+0] = 0
			$mipmapImage2[(j*2+i)*3+1] = 0
			$mipmapImage2[(j*2+i)*3+2] = 255
		end
	end
	$mipmapImage1[0] = 255
	$mipmapImage1[1] = 255
	$mipmapImage1[2] = 255
end

def myinit
	glEnable(GL_DEPTH_TEST)
	glDepthFunc(GL_LESS)
	glShadeModel(GL_FLAT)
	
	glTranslate(0.0, 0.0, -3.6)
	makeImages()
	glPixelStore(GL_UNPACK_ALIGNMENT, 1)
	glTexImage2D(GL_TEXTURE_2D, 0, 3, 32, 32, 0, GL_RGB, GL_UNSIGNED_BYTE, $mipmapImage32.pack("C*"))
	glTexImage2D(GL_TEXTURE_2D, 1, 3, 16, 16, 0, GL_RGB, GL_UNSIGNED_BYTE, $mipmapImage16.pack("C*"))
	glTexImage2D(GL_TEXTURE_2D, 2, 3, 8, 8, 0, GL_RGB, GL_UNSIGNED_BYTE, $mipmapImage8.pack("C*"))
	glTexImage2D(GL_TEXTURE_2D, 3, 3, 4, 4, 0, GL_RGB, GL_UNSIGNED_BYTE, $mipmapImage4.pack("C*"))
	glTexImage2D(GL_TEXTURE_2D, 4, 3, 2, 2, 0, GL_RGB, GL_UNSIGNED_BYTE, $mipmapImage2.pack("C*"))
	glTexImage2D(GL_TEXTURE_2D, 5, 3, 1, 1, 0, GL_RGB, GL_UNSIGNED_BYTE, $mipmapImage1.pack("C*"))
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT)
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT)
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST)
	glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, 
	GL_NEAREST_MIPMAP_NEAREST)
	glTexEnv(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_DECAL)
	glEnable(GL_TEXTURE_2D)
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	glBegin(GL_QUADS)
	glTexCoord(0.0, 0.0); glVertex(-2.0, -1.0, 0.0)
	glTexCoord(0.0, 8.0); glVertex(-2.0, 1.0, 0.0)
	glTexCoord(8.0, 8.0); glVertex(2000.0, 1.0, -6000.0)
	glTexCoord(8.0, 0.0); glVertex(2000.0, -1.0, -6000.0)
	glEnd()
	glutSwapBuffers()
end

myReshape = Proc.new do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	gluPerspective(60.0, 1.0*w/h, 1.0, 30000.0)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0);
	end
end

glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
myinit()
glutReshapeFunc(myReshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()
