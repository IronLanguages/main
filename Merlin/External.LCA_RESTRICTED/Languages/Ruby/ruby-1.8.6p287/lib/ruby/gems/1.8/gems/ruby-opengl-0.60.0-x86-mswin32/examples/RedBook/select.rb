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
#
# select.c
# This is an illustration of the selection mode and 
# name stack, which detects whether objects which collide 
# with a viewing volume.  First, four triangles and a 
# rectangular box representing a viewing volume are drawn 
# (drawScene routine).  The green triangle and yellow 
# triangles appear to lie within the viewing volume, but 
# the red triangle appears to lie outside it.  Then the 
# selection mode is entered (selectObjects routine).  
# Drawing to the screen ceases.  To see if any collisions 
# occur, the four triangles are called.  In this example, 
# the green triangle causes one hit with the name 1, and 
# the yellow triangles cause one hit with the name 3.
require 'opengl'
include Gl,Glu,Glut

# draw a triangle with vertices at (x1, y1), (x2, y2) 
# and (x3, y3) at z units away from the origin.
def drawTriangle(x1, y1, x2, y2, x3, y3, z)
	glBegin(GL_TRIANGLES)
	glVertex(x1, y1, z)
	glVertex(x2, y2, z)
	glVertex(x3, y3, z)
	glEnd()
end

# draw a rectangular box with these outer x, y, and z values
def drawViewVolume(x1, x2, y1, y2, z1, z2)
	glColor(1.0, 1.0, 1.0)
	glBegin(GL_LINE_LOOP)
	glVertex(x1, y1, -z1)
	glVertex(x2, y1, -z1)
	glVertex(x2, y2, -z1)
	glVertex(x1, y2, -z1)
	glEnd()
	
	glBegin(GL_LINE_LOOP)
	glVertex(x1, y1, -z2)
	glVertex(x2, y1, -z2)
	glVertex(x2, y2, -z2)
	glVertex(x1, y2, -z2)
	glEnd()
	
	glBegin(GL_LINES)# 4 lines
	glVertex(x1, y1, -z1)
	glVertex(x1, y1, -z2)
	glVertex(x1, y2, -z1)
	glVertex(x1, y2, -z2)
	glVertex(x2, y1, -z1)
	glVertex(x2, y1, -z2)
	glVertex(x2, y2, -z1)
	glVertex(x2, y2, -z2)
	glEnd()
end

# drawScene draws 4 triangles and a wire frame
# which represents the viewing volume.
def drawScene()
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	gluPerspective(40.0, 4.0/3.0, 1.0, 100.0)
	
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
	gluLookAt(7.5, 7.5, 12.5, 2.5, 2.5, -5.0, 0.0, 1.0, 0.0)
	glColor(0.0, 1.0, 0.0)# green triangle
	drawTriangle(2.0, 2.0, 3.0, 2.0, 2.5, 3.0, -5.0)
	glColor(1.0, 0.0, 0.0)# red triangle
	drawTriangle(2.0, 7.0, 3.0, 7.0, 2.5, 8.0, -5.0)
	glColor(1.0, 1.0, 0.0)# yellow triangles
	drawTriangle(2.0, 2.0, 3.0, 2.0, 2.5, 3.0, 0.0)
	drawTriangle(2.0, 2.0, 3.0, 2.0, 2.5, 3.0, -10.0)
	drawViewVolume(0.0, 5.0, 0.0, 5.0, 0.0, 10.0)
end

# processHits prints out the contents of the selection array
def processHits(hits, buffer)
	printf("hits = %d\n", hits)
	ptr = buffer.unpack("I*")
	p = 0
	for i in 0...hits 	# for each hit
		names = ptr[p]
		printf(" number of names for hit = %d\n", names); p+=1
		printf("  z1 is %g",  ptr[p].to_f/0xffffffff); p+=1
		printf(" z2 is %g\n",  ptr[p].to_f/0xffffffff); p+=1
		printf("   the name is ")
		for j in 0...names  # for each name
			printf("%d ", ptr[p]); p+=1
		end
		printf("\n")
	end
end

# selectObjects "draws" the triangles in selection mode, 
# assigning names for the triangles.  Note that the third
# and fourth triangles share one name, so that if either 
# or both triangles intersects the viewing/clipping volume, 
# only one hit will be registered.

BUFSIZE=512

def selectObjects
	selectBuf = glSelectBuffer(BUFSIZE)
	glRenderMode(GL_SELECT)
	
	glInitNames()
	glPushName(0)
	
	glPushMatrix()
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	glOrtho(0.0, 5.0, 0.0, 5.0, 0.0, 10.0)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
	glLoadName(1)
	drawTriangle(2.0, 2.0, 3.0, 2.0, 2.5, 3.0, -5.0)
	glLoadName(2)
	drawTriangle(2.0, 7.0, 3.0, 7.0, 2.5, 8.0, -5.0)
	glLoadName(3)
	drawTriangle(2.0, 2.0, 3.0, 2.0, 2.5, 3.0, 0.0)
	drawTriangle(2.0, 2.0, 3.0, 2.0, 2.5, 3.0, -10.0)
	glPopMatrix()
	glFlush()
	
	hits = glRenderMode(GL_RENDER)
	processHits(hits, selectBuf)
end 

def init
	glEnable(GL_DEPTH_TEST)
	glShadeModel(GL_FLAT)
end

display = proc do
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	drawScene()
	selectObjects()
	glFlush()
end

keyboard = proc do |key, x, y|
	case (key)
		when ?\e
			exit(0)
	end
end

# Main Loop

GLUT.Init
glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow $0
init()
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()
