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
# feedback.c
# This program demonstrates use of OpenGL feedback.  First,
# a lighting environment is set up and a few lines are drawn.
# Then feedback mode is entered, and the same lines are 
# drawn.  The results in the feedback buffer are printed.
require 'opengl'
include Gl,Glu,Glut

#  Initialize lighting.
def init
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
end

# Draw a few lines and two points, one of which will 
# be clipped.  If in feedback mode, a passthrough token 
# is issued between the each primitive.
def drawGeometry(mode)
	glBegin(GL_LINE_STRIP)
	glNormal(0.0, 0.0, 1.0)
	glVertex(30.0, 30.0, 0.0)
	glVertex(50.0, 60.0, 0.0)
	glVertex(70.0, 40.0, 0.0)
	glEnd()
	if (mode == GL_FEEDBACK)
		glPassThrough(1.0)
	end
	glBegin(GL_POINTS)
	glVertex(-100.0, -100.0, -100.0)  # will be clipped
	glEnd()
	if (mode == GL_FEEDBACK)
		glPassThrough(2.0)
	end
	glBegin(GL_POINTS)
	glNormal(0.0, 0.0, 1.0)
	glVertex(50.0, 50.0, 0.0)
	glEnd()
end

# Write contents of one vertex to stdout.
def print3DcolorVertex(size, count, buffer)
	printf("  ")
	for i in 0...7
		printf("%4.2f ", buffer[size-count])
		count -= 1
	end
	printf("\n")
	return count
end

# Write contents of entire buffer.  (Parse tokens!)
def  printBuffer(size, buffer)
	count = size
	while (count!=0)
		token = buffer[size-count]
		count-=1
		if (token == GL_PASS_THROUGH_TOKEN)
			printf("GL_PASS_THROUGH_TOKEN\n")
			printf("  %4.2f\n", buffer[size-count])
			count-=1
		elsif (token == GL_POINT_TOKEN)
			printf("GL_POINT_TOKEN\n")
			count = print3DcolorVertex(size, count, buffer)
		elsif (token == GL_LINE_TOKEN)
			printf("GL_LINE_TOKEN\n")
			count = print3DcolorVertex(size, count, buffer)
			count = print3DcolorVertex(size, count, buffer)
		elsif (token == GL_LINE_RESET_TOKEN)
			printf("GL_LINE_RESET_TOKEN\n")
			count = print3DcolorVertex(size, count, buffer)
			count = print3DcolorVertex(size, count, buffer)
		end
	end
end

display = proc do
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	glOrtho(0.0, 100.0, 0.0, 100.0, 0.0, 1.0)
	
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glClear(GL_COLOR_BUFFER_BIT)
	drawGeometry(GL_RENDER)
	
	feedBuffer = glFeedbackBuffer(1024, GL_3D_COLOR)
	glRenderMode(GL_FEEDBACK)
	drawGeometry(GL_FEEDBACK)
	
	size = glRenderMode(GL_RENDER)
	printBuffer(size, feedBuffer.unpack("f#{size}"))
	glutSwapBuffers()
end

keyboard = proc do |key, x, y|
	case (key)
		when ?\e
		exit(0)
	end
end

glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(100, 100)
glutInitWindowPosition(100, 100)
glutCreateWindow()
init()
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()
