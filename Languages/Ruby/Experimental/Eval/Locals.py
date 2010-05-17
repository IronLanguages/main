def foo():
  y = 1
  
  def bar():
    z1 = 2
    z2 = 3
    
    def baz():
      w1 = 4
      w2 = 5
      w3 = 6
      print y,z1,z2
      print eval("y + z1 + w1")
    
    return baz
  
  return bar 
  
b = foo()
c = b()
c()

print '---'

def gen():
  x = 1
  yield 1
  print x
  yield 2
  print x
  yield 3
  print x  
  
for z in gen():
  print z
  
def make_tuple():
  return [10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10]
  
def big():
  t = make_tuple()
  a0,b0,c0,d0,e0, f0,g0,h0,i0,j0, k0,l0,m0,n0,o0, p0,q0,r0,s0,t0, u0,v0,w0,x0,y0 = t
  a1,b1,c1,d1,e1, f1,g1,h1,i1,j1, k1,l1,m1,n1,o1, p1,q1,r1,s1,t1, u1,v1,w1,x1,y1 = t
  a2,b2,c2,d2,e2, f2,g2,h2,i2,j2, k2,l2,m2,n2,o2, p2,q2,r2,s2,t2, u2,v2,w2,x2,y2 = t
  a3,b3,c3,d3,e3, f3,g3,h3,i3,j3, k3,l3,m3,n3,o3, p3,q3,r3,s3,t3, u3,v3,w3,x3,y3 = t
  a4,b4,c4,d4,e4, f4,g4,h4,i4,j4, k4,l4,m4,n4,o4, p4,q4,r4,s4,t4, u4,v4,w4,x4,y4 = t
  a5,b5 = 1,2
  a01,b01,c01,d01,e01, f01,g01,h01,i01,j01, k01,l01,m01,n01,o01, p01,q01,r01,s01,t01, u01,v01,w01,x01,y01 = t
  a11,b11,c11,d11,e11, f11,g11,h11,i11,j11, k11,l11,m11,n11,o11, p11,q11,r11,s11,t11, u11,v11,w11,x11,y11 = t
  a21,b21,c21,d21,e21, f21,g21,h21,i21,j21, k21,l21,m21,n21,o21, p21,q21,r21,s21,t21, u21,v21,w21,x21,y21 = t
  a31,b31,c31,d31,e31, f31,g31,h31,i31,j31, k31,l31,m31,n31,o31, p31,q31,r31,s31,t31, u31,v31,w31,x31,y31 = t
  a41,b41,c41,d41,e41, f41,g41,h41,i41,j41, k41,l41,m41,n41,o41, p41,q41,r41,s41,t41, u41,v41,w41,x41,y41 = t
  a51,b51,c51 = 1,2,3
  
  a02 = 1
  exec("print 'big'")
  print locals()
  
big()