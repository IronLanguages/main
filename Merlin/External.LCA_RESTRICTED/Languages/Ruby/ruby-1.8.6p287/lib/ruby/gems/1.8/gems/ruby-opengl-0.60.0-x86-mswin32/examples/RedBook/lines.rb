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
# lines.c
# This program demonstrates geometric primitives and
# their attributes.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

def drawOneLine(x1,y1,x2,y2)
	glBegin(GL_LINES)
	glVertex((x1),(y1))
	glVertex((x2),(y2))
	glEnd()
end

def init
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glShadeModel(GL_FLAT)
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT)
	
	# select white for all lines
	glColor(1.0, 1.0, 1.0)
	
	# in 1st row, 3 lines, each with a different stipple
	glEnable(GL_LINE_STIPPLE)
	
	glLineStipple(1, 0x0101) # dotted
	drawOneLine(50.0, 125.0, 150.0, 125.0)
	glLineStipple(1, 0x00FF) # dashed
	drawOneLine(150.0, 125.0, 250.0, 125.0)
	glLineStipple(1, 0x1C47)  # dash/dot/dash
	drawOneLine(250.0, 125.0, 350.0, 125.0)
	
	# in 2nd row, 3 wide lines, each with different stipple
	glLineWidth(5.0)
	glLineStipple(1, 0x0101) # dotted
	drawOneLine(50.0, 100.0, 150.0, 100.0)
	glLineStipple(1, 0x00FF) # dashed
	drawOneLine(150.0, 100.0, 250.0, 100.0)
	glLineStipple(1, 0x1C47) # dash/dot/dash
	drawOneLine(250.0, 100.0, 350.0, 100.0)
	glLineWidth(1.0)
	
	# in 3rd row, 6 lines, with dash/dot/dash stipple
	# as part of a single connected line strip
	glLineStipple(1, 0x1C47)  # dash/dot/dash
	glBegin(GL_LINE_STRIP)
	for i in  0..6
		glVertex(50.0 + (i * 50.0), 75.0)
	end
	glEnd()
	
	# in 4th row, 6 independent lines with same stipple
	for i in 0..5
		drawOneLine(50.0 + (i * 50.0), 50.0, 50.0 + ((i+1) * 50.0), 50.0)
	end
	
	# in 5th row, 1 line, with dash/dot/dash stipple
	# and a stipple repeat factor of 5
	glLineStipple(5, 0x1C47)  # dash/dot/dash
	drawOneLine(50.0, 25.0, 350.0, 25.0)
	
	glDisable(GL_LINE_STIPPLE)
	glutSwapBuffers()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0,  w,  h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	GLU.Ortho2D(0.0,  w, 0.0,  h)
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
			exit(0)
	end
end

glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(400, 150) 
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutDisplayFunc(display) 
glutReshapeFunc(reshape)
glutKeyboardFunc(keyboard)
glutMainLoop()
