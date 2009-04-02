#!/usr/bin/env ruby -rubygems
#/* Copyright (c) Mark J. Kilgard, 1994. */
#
#/*
# * (c) Copyright 1993, Silicon Graphics, Inc.
# * ALL RIGHTS RESERVED 
# * Permission to use, copy, modify, and distribute this software for 
# * any purpose and without fee is hereby granted, provided that the above
# * copyright notice appear in all copies and that both the copyright notice
# * and this permission notice appear in supporting documentation, and that 
# * the name of Silicon Graphics, Inc. not be used in advertising
# * or publicity pertaining to distribution of the software without specific,
# * written prior permission. 
# *
# * THE MATERIAL EMBODIED ON THIS SOFTWARE IS PROVIDED TO YOU "AS-IS"
# * AND WITHOUT WARRANTY OF ANY KIND, EXPRESS, IMPLIED OR OTHERWISE,
# * INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF MERCHANTABILITY OR
# * FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL SILICON
# * GRAPHICS, INC.  BE LIABLE TO YOU OR ANYONE ELSE FOR ANY DIRECT,
# * SPECIAL, INCIDENTAL, INDIRECT OR CONSEQUENTIAL DAMAGES OF ANY
# * KIND, OR ANY DAMAGES WHATSOEVER, INCLUDING WITHOUT LIMITATION,
# * LOSS OF PROFIT, LOSS OF USE, SAVINGS OR REVENUE, OR THE CLAIMS OF
# * THIRD PARTIES, WHETHER OR NOT SILICON GRAPHICS, INC.  HAS BEEN
# * ADVISED OF THE POSSIBILITY OF SUCH LOSS, HOWEVER CAUSED AND ON
# * ANY THEORY OF LIABILITY, ARISING OUT OF OR IN CONNECTION WITH THE
# * POSSESSION, USE OR PERFORMANCE OF THIS SOFTWARE.
# * 
# * US Government Users Restricted Rights 
# * Use, duplication, or disclosure by the Government is subject to
# * restrictions set forth in FAR 52.227.19(c)(2) or subparagraph
# * (c)(1)(ii) of the Rights in Technical Data and Computer Software
# * clause at DFARS 252.227-7013 and/or in similar or successor
# * clauses in the FAR or the DOD or NASA FAR Supplement.
# * Unpublished-- rights reserved under the copyright laws of the
# * United States.  Contractor/manufacturer is Silicon Graphics,
# * Inc., 2011 N.  Shoreline Blvd., Mountain View, CA 94039-7311.
# *
# * OpenGL(TM) is a trademark of Silicon Graphics, Inc.
# */
#/*
# *  plane.c
# *  This program demonstrates the use of local versus 
# *  infinite lighting on a flat plane.
# */

require "gl"
require "glut"
require "mathn"

# /*  Initialize material property, light source, and lighting model.
# */
def myinit
    mat_ambient = [ 0.0, 0.0, 0.0, 1.0 ];
#/*   mat_specular and mat_shininess are NOT default values	*/
    mat_diffuse = [ 0.4, 0.4, 0.4, 1.0 ];
    mat_specular = [ 1.0, 1.0, 1.0, 1.0 ];
    mat_shininess = [ 15.0 ];

    light_ambient = [ 0.0, 0.0, 0.0, 1.0 ];
    light_diffuse = [ 1.0, 1.0, 1.0, 1.0 ];
    light_specular = [ 1.0, 1.0, 1.0, 1.0 ];
    lmodel_ambient = [ 0.2, 0.2, 0.2, 1.0 ];

    Gl.glMaterial(Gl::GL_FRONT, Gl::GL_AMBIENT, mat_ambient);
    Gl.glMaterial(Gl::GL_FRONT, Gl::GL_DIFFUSE, mat_diffuse);
    Gl.glMaterial(Gl::GL_FRONT, Gl::GL_SPECULAR, mat_specular);
    Gl.glMaterial(Gl::GL_FRONT, Gl::GL_SHININESS, *mat_shininess);
    Gl.glLight(Gl::GL_LIGHT0, Gl::GL_AMBIENT, light_ambient);
    Gl.glLight(Gl::GL_LIGHT0, Gl::GL_DIFFUSE, light_diffuse);
    Gl.glLight(Gl::GL_LIGHT0, Gl::GL_SPECULAR, light_specular);
    Gl.glLightModel(Gl::GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);

    Gl.glEnable(Gl::GL_LIGHTING);
    Gl.glEnable(Gl::GL_LIGHT0);
    Gl.glDepthFunc(Gl::GL_LESS);
    Gl.glEnable(Gl::GL_DEPTH_TEST);
end

def drawPlane
    Gl.glBegin(Gl::GL_QUADS);
    Gl.glNormal(0.0, 0.0, 1.0);
    Gl.glVertex(-1.0, -1.0, 0.0);
    Gl.glVertex(0.0, -1.0, 0.0);
    Gl.glVertex(0.0, 0.0, 0.0);
    Gl.glVertex(-1.0, 0.0, 0.0);

    Gl.glNormal(0.0, 0.0, 1.0);
    Gl.glVertex(0.0, -1.0, 0.0);
    Gl.glVertex(1.0, -1.0, 0.0);
    Gl.glVertex(1.0, 0.0, 0.0);
    Gl.glVertex(0.0, 0.0, 0.0);

    Gl.glNormal(0.0, 0.0, 1.0);
    Gl.glVertex(0.0, 0.0, 0.0);
    Gl.glVertex(1.0, 0.0, 0.0);
    Gl.glVertex(1.0, 1.0, 0.0);
    Gl.glVertex(0.0, 1.0, 0.0);

    Gl.glNormal(0.0, 0.0, 1.0);
    Gl.glVertex(0.0, 0.0, 0.0);
    Gl.glVertex(0.0, 1.0, 0.0);
    Gl.glVertex(-1.0, 1.0, 0.0);
    Gl.glVertex(-1.0, 0.0, 0.0);
    Gl.glEnd();
end

display = Proc.new {
    infinite_light = [ 1.0, 1.0, 1.0, 0.0 ];
    local_light = [ 1.0, 1.0, 1.0, 1.0 ];

    Gl.glClear(Gl::GL_COLOR_BUFFER_BIT | Gl::GL_DEPTH_BUFFER_BIT);

    Gl.glPushMatrix();
    Gl.glTranslate(-1.5, 0.0, 0.0);
    Gl.glLight(Gl::GL_LIGHT0, Gl::GL_POSITION, infinite_light);
    drawPlane();
    Gl.glPopMatrix();

    Gl.glPushMatrix();
    Gl.glTranslate(1.5, 0.0, 0.0);
    Gl.glLight(Gl::GL_LIGHT0, Gl::GL_POSITION, local_light);
    drawPlane();
    Gl.glPopMatrix();
    Gl.glFlush();
}

myReshape = Proc.new {|w, h|
    Gl.glViewport(0, 0, w, h);
    Gl.glMatrixMode(Gl::GL_PROJECTION);
    Gl.glLoadIdentity();
    if (w <= h) 
	Gl.glOrtho(-1.5, 1.5, -1.5*h/w, 1.5*h/w, -10.0, 10.0);
    else 
	Gl.glOrtho(-1.5*w/h, 1.5*w/h, -1.5, 1.5, -10.0, 10.0);
    end
    Gl.glMatrixMode(Gl::GL_MODELVIEW);
}

# Keyboard handler to exit when ESC is typed
keyboard = lambda do |key, x, y|
  case(key)
  when ?\e
    exit(0)
  end
end

#/*  Main Loop
# *  Open window with initial window size, title bar, 
# *  RGBA display mode, and handle input events.
# */
#int main(int argc, char** argv)
#{
    Glut.glutInit
    Glut.glutInitDisplayMode(Glut::GLUT_SINGLE | Glut::GLUT_RGB | Glut::GLUT_DEPTH);
    Glut.glutInitWindowSize(500, 200);
    Glut.glutCreateWindow($0);
    myinit();
    Glut.glutReshapeFunc(myReshape);
    Glut.glutDisplayFunc(display);
    Glut.glutKeyboardFunc(keyboard);
    Glut.glutMainLoop();
