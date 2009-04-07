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
# colormat.c
# After initialization, the program will be in
# ColorMaterial mode.  Interaction:  pressing the 
# mouse buttons will change the diffuse reflection values.
require 'opengl'
require 'mathn'
include Gl,Glu,Glut

$diffuseMaterial = [0.5,0.5,0.5,1.0]

# Initialize material property, light source, lighting model,
# and depth buffer.
def init
	mat_specular = [ 1.0, 1.0, 1.0, 1.0 ]
	light_position = [ 1.0, 1.0, 1.0, 0.0 ]
	
	glClearColor(0.0, 0.0, 0.0, 0.0)
	glShadeModel(GL_SMOOTH)
	glEnable(GL_DEPTH_TEST)
	glMaterial(GL_FRONT, GL_DIFFUSE, $diffuseMaterial)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, 25.0)
	glLight(GL_LIGHT0, GL_POSITION, light_position)
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
	
	glColorMaterial(GL_FRONT, GL_DIFFUSE)
	glEnable(GL_COLOR_MATERIAL)
end

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	glutSolidSphere(1.0, 20, 16)
	glutSwapBuffers()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0,  w,  h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= h)
		glOrtho(-1.5, 1.5, -1.5*h/w, 1.5*h/w, -10.0, 10.0)
	else
		glOrtho(-1.5*w/h, 1.5*w/h, -1.5, 1.5, -10.0, 10.0)
	end
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end

mouse = Proc.new do |button, state, x, y|
	case (button)
		when GLUT_LEFT_BUTTON
			if (state == GLUT_DOWN)
				$diffuseMaterial[0] = $diffuseMaterial[0] + 0.1
				if ($diffuseMaterial[0] > 1.0)
					$diffuseMaterial[0] = 0.0
				end
				glColor($diffuseMaterial)
				glutPostRedisplay()
			end
		when GLUT_MIDDLE_BUTTON
			if (state == GLUT_DOWN)
				$diffuseMaterial[1] = $diffuseMaterial[1] + 0.1
				if ($diffuseMaterial[1] > 1.0)
					$diffuseMaterial[1] = 0.0
				end
				glColor($diffuseMaterial)
				glutPostRedisplay()
			end
		when GLUT_RIGHT_BUTTON
			if (state == GLUT_DOWN)
				$diffuseMaterial[2] = $diffuseMaterial[2] + 0.1
				if ($diffuseMaterial[2] > 1.0)
					$diffuseMaterial[2] = 0.0
				end
				glColor($diffuseMaterial)
				glutPostRedisplay()
			end
	end
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
			exit(0)
	end
end

glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(500, 500) 
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
init()
glutDisplayFunc(display) 
glutReshapeFunc(reshape)
glutMouseFunc(mouse)
glutKeyboardFunc(keyboard)
glutMainLoop()
