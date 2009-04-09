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
# stroke.c 
# This program demonstrates some characters of a 
# stroke (vector) font.  The characters are represented
# by display lists, which are given numbers which 
# correspond to the ASCII values of the characters.
# Use of glCallLists() is demonstrated.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

PT=1
STROKE=2
END_=3

Adata = [
	[[0, 0], PT], [[0, 9], PT], [[1, 10], PT], [[4, 10], PT], 
	[[5, 9], PT], [[5, 0], STROKE], [[0, 5], PT], [[5, 5], END_]
]

Edata = [
	[[5, 0], PT], [[0, 0], PT], [[0, 10], PT], [[5, 10], STROKE],
	[[0, 5], PT], [[4, 5], END_]
]

Pdata = [
	[[0, 0], PT], [[0, 10], PT],  [[4, 10], PT], [[5, 9], PT], [[5, 6], PT], 
	[[4, 5], PT], [[0, 5], END_]
]

Rdata = [
	[[0, 0], PT], [[0, 10], PT],  [[4, 10], PT], [[5, 9], PT], [[5, 6], PT], 
	[[4, 5], PT], [[0, 5], STROKE], [[3, 5], PT], [[5, 0], END_]
]

Sdata = [
	[[0, 1], PT], [[1, 0], PT], [[4, 0], PT], [[5, 1], PT], [[5, 4], PT], 
	[[4, 5], PT], [[1, 5], PT], [[0, 6], PT], [[0, 9], PT], [[1, 10], PT], 
	[[4, 10], PT], [[5, 9], END_]
]

# drawLetter() interprets the instructions from the array
# for that letter and renders the letter with line segments.
def drawLetter(l)
	i = 0
	glBegin(GL_LINE_STRIP)
	while true
		case (l[i][1])
			when PT
				glVertex(l[i][0])
			when STROKE
				glVertex(l[i][0])
				glEnd()
				glBegin(GL_LINE_STRIP)
			when END_
				glVertex(l[i][0])
				glEnd()
				glTranslate(8.0, 0.0, 0.0)
				return
		end
		i += 1
	end
end

# Create a display list for each of 6 characters
def myinit
	glShadeModel(GL_FLAT)
	
	base = glGenLists(128)
	glListBase(base)
	glNewList(base+'A'[0], GL_COMPILE); drawLetter(Adata); glEndList()
	glNewList(base+'E'[0], GL_COMPILE); drawLetter(Edata); glEndList()
	glNewList(base+'P'[0], GL_COMPILE); drawLetter(Pdata); glEndList()
	glNewList(base+'R'[0], GL_COMPILE); drawLetter(Rdata); glEndList()
	glNewList(base+'S'[0], GL_COMPILE); drawLetter(Sdata); glEndList()
	glNewList(base+' '[0], GL_COMPILE); glTranslate(8.0, 0.0, 0.0); glEndList()
end

$test1 = "A SPARE SERAPE APPEARS AS"
$test2 = "APES PREPARE RARE PEPPERS"

def printStrokedString(s)
	glCallLists(GL_BYTE,s)
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT)
	glColor(1.0, 1.0, 1.0)
	glPushMatrix()
	glScale(2.0, 2.0, 2.0)
	glTranslate(10.0, 30.0, 0.0)
	printStrokedString($test1)
	glPopMatrix()
	glPushMatrix()
	glScale(2.0, 2.0, 2.0)
	glTranslate(10.0, 13.0, 0.0)
	printStrokedString($test2)
	glPopMatrix()
	glutSwapBuffers()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	glOrtho(0.0, w, 0.0, h, -1.0, 1.0)
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0);
	end
end

# Main Loop
# Open window with initial window size, title bar, 
# RGBA display mode, and handle input events.
glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(440, 120)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
myinit()
glutDisplayFunc(display)
glutReshapeFunc(reshape)
glutKeyboardFunc(keyboard)
glutMainLoop()
