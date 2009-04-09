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
# tess.c
# This program demonstrates polygon tessellation.
# Two tesselated objects are drawn.  The first is a
# rectangle with a triangular hole.  The second is a
# smooth shaded, self-intersecting star.
#
# Note the exterior rectangle is drawn with its vertices
# in counter-clockwise order, but its interior clockwise.
# Note the combineCallback is needed for the self-intersecting
# star.  Also note that removing the TessProperty for the 
# star will make the interior unshaded (WINDING_ODD).
require 'opengl'
include Gl,Glu,Glut

$startList = nil

display = proc do
	glClear(GL_COLOR_BUFFER_BIT)
	glColor(1.0, 1.0, 1.0)
	glCallList($startList)
	glCallList($startList + 1)
	glutSwapBuffers()
end

$beginCallback = proc do |which|
	glBegin(which)
end

$errorCallback = proc do |errorCode|
	print "Tessellation Error: #{gluErrorString(errorCode)}"
	exit(0)
end

$endCallback = proc do
	glEnd()
end

$vertexCallback = proc do |vertex|
	glColor(vertex[3], vertex[4], vertex[5])
	glVertex(vertex[0], vertex[1], vertex[2])
end

# combineCallback is used to create a new vertex when edges
# intersect.  coordinate location is trivial to calculate,
# but weight[4] may be used to average color, normal, or texture
# coordinate data.  In this program, color is weighted.
$combineCallback = proc do |coords, vertex_data, weight|
	vertex = []
	vertex[0] = coords[0]
	vertex[1] = coords[1]
	vertex[2] = coords[2]
	for i in 3...6
		vertex[i] = weight[0] * vertex_data[0][i] + weight[1] * vertex_data[1][i] + weight[2] * vertex_data[2][i] + weight[3] * vertex_data[3][i]
	end
	vertex
end

def init
	rect = [[50.0, 50.0, 0.0],
		[200.0, 50.0, 0.0],
		[200.0, 200.0, 0.0],
		[50.0, 200.0, 0.0]]
	tri = [[75.0, 75.0, 0.0],
		[125.0, 175.0, 0.0],
		[175.0, 75.0, 0.0]]
	star= [[250.0, 50.0, 0.0, 1.0, 0.0, 1.0],
		[325.0, 200.0, 0.0, 1.0, 1.0, 0.0],
		[400.0, 50.0, 0.0, 0.0, 1.0, 1.0],
		[250.0, 150.0, 0.0, 1.0, 0.0, 0.0],
		[400.0, 150.0, 0.0, 0.0, 1.0, 0.0]]

	glClearColor(0.0, 0.0, 0.0, 0.0)
	
	$startList = glGenLists(2)
	
	tobj = gluNewTess()
	gluTessCallback(tobj, GLU_TESS_VERTEX, proc do |v| glVertex(v) end )
	gluTessCallback(tobj, GLU_TESS_BEGIN, $beginCallback)
	gluTessCallback(tobj, GLU_TESS_END, $endCallback)
	gluTessCallback(tobj, GLU_TESS_ERROR, $errorCallback)

	# rectangle with triangular hole inside
	glNewList($startList, GL_COMPILE)
	glShadeModel(GL_FLAT)    
	gluTessBeginPolygon(tobj, nil)
	gluTessBeginContour(tobj)
	gluTessVertex(tobj, rect[0], rect[0])
	gluTessVertex(tobj, rect[1], rect[1])
	gluTessVertex(tobj, rect[2], rect[2])
	gluTessVertex(tobj, rect[3], rect[3])
	gluTessEndContour(tobj)
	gluTessBeginContour(tobj)
	gluTessVertex(tobj, tri[0], tri[0])
	gluTessVertex(tobj, tri[1], tri[1])
	gluTessVertex(tobj, tri[2], tri[2])
	gluTessEndContour(tobj)
	gluTessEndPolygon(tobj)
	glEndList()

	gluTessCallback(tobj, GLU_TESS_VERTEX, $vertexCallback)
	gluTessCallback(tobj, GLU_TESS_BEGIN, $beginCallback)
	gluTessCallback(tobj, GLU_TESS_END, $endCallback)
	gluTessCallback(tobj, GLU_TESS_ERROR, $errorCallback)
	gluTessCallback(tobj, GLU_TESS_COMBINE, $combineCallback)


# smooth shaded, self-intersecting star
	glNewList($startList + 1, GL_COMPILE)
	glShadeModel(GL_SMOOTH)    
	gluTessProperty(tobj, GLU_TESS_WINDING_RULE, GLU_TESS_WINDING_POSITIVE)
	gluTessBeginPolygon(tobj, nil)
	gluTessBeginContour(tobj)
	gluTessVertex(tobj, star[0], star[0])
	gluTessVertex(tobj, star[1], star[1])
	gluTessVertex(tobj, star[2], star[2])
	gluTessVertex(tobj, star[3], star[3])
	gluTessVertex(tobj, star[4], star[4])
	gluTessEndContour(tobj)
	gluTessEndPolygon(tobj)
	glEndList()
	gluDeleteTess(tobj)
end

reshape = proc do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	gluOrtho2D(0.0, w, 0.0, h)
end

keyboard = proc do |key, x, y|
	case (key)
		when ?\e
			exit(0)
	end
end

glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow()
init()
glutDisplayFunc(display)
glutReshapeFunc(reshape)
glutKeyboardFunc(keyboard)
glutMainLoop()
