#
# Copyright (c) Mark J. Kilgard, 1994. */
#
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
#  pickdepth.c
#  Picking is demonstrated in this program.  In 
#  rendering mode, three overlapping rectangles are 
#  drawn.  When the left mouse button is pressed, 
#  selection mode is entered with the picking matrix.  
#  Rectangles which are drawn under the cursor position
#  are "picked."  Pay special attention to the depth 
#  value range, which is returned.
require 'opengl'
include Gl,Glu,Glut

def myinit
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glDepthFunc(GL_LESS)
	glEnable(GL_DEPTH_TEST)
	glShadeModel(GL_FLAT)
	glDepthRange(0.0, 1.0)  #/* The default z mapping */
end

# The three rectangles are drawn.  In selection mode, 
# each rectangle is given the same name.  Note that 
# each rectangle is drawn with a different z value.
def drawRects(mode)
	if (mode == GL_SELECT)
		glLoadName(1)
	end
	glBegin(GL_QUADS)
	glColor(1.0, 1.0, 0.0)
	glVertex(2, 0, 0)
	glVertex(2, 6, 0)
	glVertex(6, 6, 0)
	glVertex(6, 0, 0)
	glEnd()
	if (mode == GL_SELECT)
		glLoadName(2)
	end
	glBegin(GL_QUADS)
	glColor(0.0, 1.0, 1.0)
	glVertex(3, 2, -1)
	glVertex(3, 8, -1)
	glVertex(8, 8, -1)
	glVertex(8, 2, -1)
	glEnd()
	if (mode == GL_SELECT)
		glLoadName(3)
	end
	glBegin(GL_QUADS)
	glColor(1.0, 0.0, 1.0)
	glVertex(0, 2, -2)
	glVertex(0, 7, -2)
	glVertex(5, 7, -2)
	glVertex(5, 2, -2)
	glEnd()
end

# processHits() prints out the contents of the 
# selection array.
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
			printf("%d ", ptr[p]) ; p+=1
		end
		printf("\n")
	end
end

# pickRects() sets up selection mode, name stack, 
# and projection matrix for picking.  Then the objects 
# are drawn.
BUFSIZE=512

pickRects = proc do |button, state, x, y|
	if (button == GLUT_LEFT_BUTTON && state == GLUT_DOWN)
		viewport = glGetDoublev(GL_VIEWPORT)
		
		selectBuf = glSelectBuffer(BUFSIZE)
		glRenderMode(GL_SELECT)
		
		glInitNames()
		glPushName(~0)
		
		glMatrixMode(GL_PROJECTION)
		glPushMatrix()
		glLoadIdentity()
		# create 5x5 pixel picking region near cursor location
		gluPickMatrix( x, viewport[3] - y, 5.0, 5.0, viewport)
		glOrtho(0.0, 8.0, 0.0, 8.0, -0.5, 2.5)
		drawRects(GL_SELECT)
		glPopMatrix()
		glFlush()
		
		hits = glRenderMode(GL_RENDER)
		processHits(hits, selectBuf)
	end
end

display = proc do
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	drawRects(GL_RENDER)
	glutSwapBuffers()
end

myReshape = proc do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	glOrtho(0.0, 8.0, 0.0, 8.0, -0.5, 2.5)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0)
	end
end

# Main Loop
# Open window with initial window size, title bar, 
# RGBA display mode, depth buffer, and handle input events.
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInit()
glutCreateWindow()
myinit()
glutMouseFunc(pickRects)
glutReshapeFunc(myReshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()
