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
# image.c
# This program demonstrates drawing pixels and shows the effect
# of glDrawPixels(), glCopyPixels(), and glPixelZoom().
# Interaction: moving the mouse while pressing the mouse button
# will copy the image in the lower-left corner of the window
# to the mouse position, using the current pixel zoom factors.
# There is no attempt to prevent you from drawing over the original
# image.  If you press the 'r' key, the original image and zoom
# factors are reset.  If you press the 'z' or 'Z' keys, you change
# the zoom factors.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

# Create checkerboard image
CheckImageWidth=64
CheckImageHeight=64
$checkImage=[]

$zoomFactor = 1.0
$height = 0.0

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

def init
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glShadeModel(GL_FLAT)
	makeCheckImage()
	glPixelStorei(GL_UNPACK_ALIGNMENT, 1)
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT)
	glRasterPos(0, 0)
	glDrawPixels(CheckImageWidth, CheckImageHeight, GL_RGB, GL_UNSIGNED_BYTE, $checkImage.pack("C*"))
	glFlush()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0,  w,  h)
	$height = h
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	gluOrtho2D(0.0,  w, 0.0, h)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

$screeny = 0.0
motion = Proc.new do |x, y|
	$screeny = $height - y
	glRasterPos(x, $screeny)
	glPixelZoom($zoomFactor, $zoomFactor)
	glCopyPixels(0, 0, CheckImageWidth, CheckImageHeight, GL_COLOR)
	glPixelZoom(1.0, 1.0)
	glFlush()
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?r,?R
			$zoomFactor = 1.0
			glutPostRedisplay()
			printf("zoomFactor reset to 1.0\n")
		when ?z
			$zoomFactor = $zoomFactor + 0.5
			if ($zoomFactor >= 3.0) 
				$zoomFactor = 3.0
			end
			printf("zoomFactor is now %4.1f\n", $zoomFactor)
		when ?Z
			$zoomFactor = $zoomFactor - 0.5
			if ($zoomFactor <= 0.5) 
				$zoomFactor = 0.5
			end
			printf("zoomFactor is now %4.1f\n", $zoomFactor)
		when ?\e
			exit(0)
	end
end

glutInit
glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB)
glutInitWindowSize(250, 250)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutDisplayFunc(display)
glutReshapeFunc(reshape)
glutKeyboardFunc(keyboard)
glutMotionFunc(motion)
glutMainLoop()
