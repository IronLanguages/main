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
# varray.c
# This program demonstrates vertex arrays.
require 'opengl'
include Gl,Glu,Glut

POINTER=1
INTERLEAVED=2

DRAWARRAY=1
ARRAYELEMENT=2
DRAWELEMENTS=3

$setupMethod = POINTER
$derefMethod = DRAWARRAY

def setupPointers
	$vertices = [25, 25,
		100, 325,
		175, 25,
		175, 325,
		250, 25,
		325, 325].pack("i*")
	$colors = [1.0, 0.2, 0.2,
		0.2, 0.2, 1.0,
		0.8, 1.0, 0.2,
		0.75, 0.75, 0.75,
		0.35, 0.35, 0.35,
		0.5, 0.5, 0.5].pack("f*")
	
	glEnableClientState(GL_VERTEX_ARRAY)
	glEnableClientState(GL_COLOR_ARRAY)
	
	glVertexPointer(2, GL_INT, 0, $vertices)
	glColorPointer(3, GL_FLOAT, 0, $colors)
end

def  setupInterleave
	$intertwined =
	[1.0, 0.2, 1.0, 100.0, 100.0, 0.0,
		1.0, 0.2, 0.2, 0.0, 200.0, 0.0,
		1.0, 1.0, 0.2, 100.0, 300.0, 0.0,
		0.2, 1.0, 0.2, 200.0, 300.0, 0.0,
		0.2, 1.0, 1.0, 300.0, 200.0, 0.0,
		0.2, 0.2, 1.0, 200.0, 100.0, 0.0].pack("f*")
	
	glInterleavedArrays(GL_C3F_V3F, 0, $intertwined)
end

def init
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glShadeModel(GL_SMOOTH)
	setupPointers()
end

display = proc do
	glClear(GL_COLOR_BUFFER_BIT)
	if ($derefMethod == DRAWARRAY) 
		glDrawArrays(GL_TRIANGLES, 0, 6)
	elsif ($derefMethod == ARRAYELEMENT)
		glBegin(GL_TRIANGLES)
		glArrayElement(2)
		glArrayElement(3)
		glArrayElement(5)
		glEnd()
	elsif ($derefMethod == DRAWELEMENTS)
		$indices = [0, 1, 3, 4].pack("I*")
		glDrawElements(GL_POLYGON, 4, GL_UNSIGNED_INT, $indices)
	end
	glutSwapBuffers()
end

reshape = proc do|w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	gluOrtho2D(0.0, w, 0.0, h)
end

mouse = proc do |button, state, x, y|
	case (button)
		when GLUT_LEFT_BUTTON
			if (state == GLUT_DOWN)
				if ($setupMethod == POINTER)
					$setupMethod = INTERLEAVED
					setupInterleave()
				elsif ($setupMethod == INTERLEAVED)
					$setupMethod = POINTER
					setupPointers()
				end
				glutPostRedisplay()
			end
		when GLUT_MIDDLE_BUTTON,GLUT_RIGHT_BUTTON
			if (state == GLUT_DOWN)
				if ($derefMethod == DRAWARRAY) 
					$derefMethod = ARRAYELEMENT
				elsif ($derefMethod == ARRAYELEMENT) 
					$derefMethod = DRAWELEMENTS
				elsif ($derefMethod == DRAWELEMENTS) 
					$derefMethod = DRAWARRAY
				end
				glutPostRedisplay()
			end
	end
end

keyboard = proc do |key, x, y|
	case (key)
		when ?\e
			exit(0)
	end
end

glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(350, 350)
glutInitWindowPosition(100, 100)
glutCreateWindow()
init()
glutDisplayFunc(display) 
glutReshapeFunc(reshape)
glutMouseFunc(mouse)
glutKeyboardFunc(keyboard)
glutMainLoop()
