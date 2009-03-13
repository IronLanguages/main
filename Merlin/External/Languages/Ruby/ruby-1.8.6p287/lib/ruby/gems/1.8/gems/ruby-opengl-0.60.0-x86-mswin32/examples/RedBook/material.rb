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
# material.c
# This program demonstrates the use of the GL lighting model.
# Several objects are drawn using different material characteristics.
# A single light source illuminates the objects.
require 'opengl'
include Gl,Glu,Glut

# Initialize z-buffer, projection matrix, light source, 
# and lighting model.  Do not specify a material property here.
def myinit
	ambient = [ 0.0, 0.0, 0.0, 1.0 ]
	diffuse = [ 1.0, 1.0, 1.0, 1.0 ]
	position = [ 0.0, 3.0, 2.0, 0.0 ]
	lmodel_ambient = [ 0.4, 0.4, 0.4, 1.0 ]
	local_view = [ 0.0 ]
	
	glEnable(GL_DEPTH_TEST)
	glDepthFunc(GL_LESS)
	
	glLight(GL_LIGHT0, GL_AMBIENT, ambient)
	glLight(GL_LIGHT0, GL_DIFFUSE, diffuse)
	glLight(GL_LIGHT0, GL_POSITION, position)
	glLightModel(GL_LIGHT_MODEL_AMBIENT, lmodel_ambient)
	glLightModel(GL_LIGHT_MODEL_LOCAL_VIEWER, local_view)
	
	glEnable(GL_LIGHTING)
	glEnable(GL_LIGHT0)
	
	glClearColor(0.0, 0.1, 0.1, 0.0)
end

# Draw twelve spheres in 3 rows with 4 columns.  
# The spheres in the first row have materials with no ambient reflection.
# The second row has materials with significant ambient reflection.
# The third row has materials with colored ambient reflection.
#
# The first column has materials with blue, diffuse reflection only.
# The second column has blue diffuse reflection, as well as specular
# reflection with a low shininess exponent.
# The third column has blue diffuse reflection, as well as specular
# reflection with a high shininess exponent (a more concentrated highlight).
# The fourth column has materials which also include an emissive component.
#
# glTranslatef() is used to move spheres to their appropriate locations.
display = proc do
	no_mat = [ 0.0, 0.0, 0.0, 1.0 ]
	mat_ambient = [ 0.7, 0.7, 0.7, 1.0 ]
	mat_ambient_color = [ 0.8, 0.8, 0.2, 1.0 ]
	mat_diffuse = [ 0.1, 0.5, 0.8, 1.0 ]
	mat_specular = [ 1.0, 1.0, 1.0, 1.0 ]
	no_shininess = [ 0.0 ]
	low_shininess = [ 5.0 ]
	high_shininess = [ 100.0 ]
	mat_emission = [0.3, 0.2, 0.2, 0.0]
	
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	
	# draw sphere in first row, first column
	# diffuse reflection only no ambient or specular  
	glPushMatrix()
	glTranslate(-3.75, 3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, no_mat)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, no_mat)
	glMaterial(GL_FRONT, GL_SHININESS, no_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in first row, second column
	# diffuse and specular reflection low shininess no ambient
	glPushMatrix()
	glTranslate(-1.25, 3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, no_mat)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, low_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in first row, third column
	# diffuse and specular reflection high shininess no ambient
	glPushMatrix()
	glTranslate(1.25, 3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, no_mat)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, high_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in first row, fourth column
	# diffuse reflection emission no ambient or specular reflection
	glPushMatrix()
	glTranslate(3.75, 3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, no_mat)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, no_mat)
	glMaterial(GL_FRONT, GL_SHININESS, no_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, mat_emission)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in second row, first column
	# ambient and diffuse reflection no specular  
	glPushMatrix()
	glTranslate(-3.75, 0.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, no_mat)
	glMaterial(GL_FRONT, GL_SHININESS, no_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in second row, second column
	# ambient, diffuse and specular reflection low shininess
	glPushMatrix()
	glTranslate(-1.25, 0.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, low_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in second row, third column
	# ambient, diffuse and specular reflection high shininess
	glPushMatrix()
	glTranslate(1.25, 0.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, high_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in second row, fourth column
	# ambient and diffuse reflection emission no specular
	glPushMatrix()
	glTranslate(3.75, 0.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, no_mat)
	glMaterial(GL_FRONT, GL_SHININESS, no_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, mat_emission)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in third row, first column
	# colored ambient and diffuse reflection no specular  
	glPushMatrix()
	glTranslate(-3.75, -3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient_color)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, no_mat)
	glMaterial(GL_FRONT, GL_SHININESS, no_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in third row, second column
	# colored ambient, diffuse and specular reflection low shininess
	glPushMatrix()
	glTranslate(-1.25, -3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient_color)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, low_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in third row, third column
	# colored ambient, diffuse and specular reflection high shininess
	glPushMatrix()
	glTranslate(1.25, -3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient_color)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, mat_specular)
	glMaterial(GL_FRONT, GL_SHININESS, high_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, no_mat)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	# draw sphere in third row, fourth column
	# colored ambient and diffuse reflection emission no specular
	glPushMatrix()
	glTranslate(3.75, -3.0, 0.0)
	glMaterial(GL_FRONT, GL_AMBIENT, mat_ambient_color)
	glMaterial(GL_FRONT, GL_DIFFUSE, mat_diffuse)
	glMaterial(GL_FRONT, GL_SPECULAR, no_mat)
	glMaterial(GL_FRONT, GL_SHININESS, no_shininess)
	glMaterial(GL_FRONT, GL_EMISSION, mat_emission)
	glutSolidSphere(1.0, 16, 16)
	glPopMatrix()
	
	glutSwapBuffers()
end

myReshape = proc do |w, h|
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= (h * 2))
		glOrtho(-6.0, 6.0, -3.0*(h.to_f*2)/w,	3.0*(h.to_f*2)/w, -10.0, 10.0)
	else
		glOrtho(-6.0*w.to_f/(h*2), 6.0*w.to_f/(h*2), -3.0, 3.0, -10.0, 10.0)
	end
	glMatrixMode(GL_MODELVIEW)
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
glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(600, 600)
glutInitWindowPosition(100, 100)
glutCreateWindow()
myinit()
glutReshapeFunc(myReshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop()

