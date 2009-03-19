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
#  list.c
#  This program demonstrates how to make and execute a 
#  display list.  Note that attributes, such as current 
#  color and matrix, are changed.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

$listName = 0

def init
	$listName = glGenLists(1)
	glNewList($listName, GL_COMPILE)
	glColor(1.0, 0.0, 0.0)  # /*  current color red  */
	glBegin(GL_TRIANGLES)
	glVertex(0.0, 0.0)
	glVertex(1.0, 0.0)
	glVertex(0.0, 1.0)
	glEnd()
	glTranslate(1.5, 0.0, 0.0) # /*  move position  */
	glEndList()
	glShadeModel(GL_FLAT)
end

def drawLine
	glBegin(GL_LINES)
	glVertex(0.0, 0.5)
	glVertex(15.0, 0.5)
	glEnd()
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT)
	glColor(0.0, 1.0, 0.0)  # current color green
	for i in (0..9 ) # draw 10 triangles
		glCallList($listName)
	end
	drawLine()  # is this line green?  NO!
							# where is the line drawn?
	glutSwapBuffers()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= h) 
		GLU.Ortho2D(0.0, 2.0, -0.5 * h/w, 1.5 * h/w)
	else
		GLU.Ortho2D(0.0, 2.0 * w/h, -0.5, 1.5) 
	end
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
# RGBA display mode, and handle input events.
glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(650, 100)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutReshapeFunc(reshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()
