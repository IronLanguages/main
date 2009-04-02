module Fox
  class FXQuatf < FXVec4f
    # Return an initialized FXQuatf instance.
    def initialize; end
    
    #
    # Construct an FXQuatf instance from a rotation axis and angle.
    #
    # ==== Parameters:
    #
    # +axis+::		the rotation axis [FXVec3f]
    # +angle+::		the rotation angle (in radians) [Float]
    #
    def initialize(axis, phi=0.0); end
    
    #
    # Construct from Euler angles yaw (z), pitch (y) and roll (x).
    #
    # ==== Parameters:
    #
    # +roll+::		roll angle in radians [Float]
    # +pitch+::		pitch angle in radians [Float]
    # +yaw+::		yaw angle in radians [Float]
    #
    def initialize(roll, pitch, yaw); end

    #
    # Construct quaternion from axes
    #
    # ==== Parameters:
    #
    # +ex+::	x-axis [FXVec3f]
    # +ey+::	y-axis [FXVec3f]
    # +ez+::	z-axis [FXVec3f]
    #
    def initialize(ex, ey, ez); end
   
    #
    # Construct quaternion from 3x3 matrix (where _mat_ is an FXMat3f instance).
    #
    def initialize(mat); end

    #
    # Construct an FXQuatf from components.
    #
    # ==== Parameters:
    #
    # +x+::		x [Float]
    # +y+::		y [Float]
    # +z+::		z [Float]
    # +width+::		w [Float]
    #
    def initialize(x, y, z, w); end
    
    # Adjust quaternion length; returns a reference to self.
    def adjust!; end
    
    #
    # Set quaternion from rotation axis and angle.
    #
    # ==== Parameters:
    #
    # +axis+::		the rotation axis [FXVec3f]
    # +angle+::		the rotation angle (in radians) [Float]
    #
    def setAxisAngle(axis, phi=0.0); end
    
    #
    # Return the rotation axis and angle for this quaternion, i.e.
    #
    #     axis, angle = aQuaternion.getAxisAngle()
    #
    # where _axis_ is an FXVec3f instance and _angle_ is the angle
    # of rotation in radians.
    #
    def getAxisAngle(); end
    
    #
    # Set quaternion from yaw (z), pitch (y) and roll (x).
    #
    # ==== Parameters:
    #
    # +roll+::		roll angle in radians [Float]
    # +pitch+::		pitch angle in radians [Float]
    # +yaw+::		yaw angle in radians [Float]
    #
    def setRollPitchYaw(roll, pitch, yaw); end
    
    #
    # Obtain roll, pitch and yaw angles (in radians) from quaternion, e.g.
    #
    #     roll, pitch, yaw = aQuaternion.getRollPitchYaw()
    #
    def getRollPitchYaw(); end
    
    # Set quaternion from axes (where _ex_, _ey_ and _ez_ are FXVec3f instances).
    def setAxes(ex, ey, ez); end
    
    # Get quaternion axes as a 3-element array of FXVec3f instances.
    def getAxes(); end

    # Return the local x-axis as an FXVec3f instance.
    def getXAxis(); end

    # Return the local y-axis as an FXVec3f instance.
    def getYAxis(); end

    # Return the local z-axis as an FXVec3f instance.
    def getZAxis(); end

    #
    # Return the exponentiation of this quaternion (a new FXQuatf instance).
    #
    def exp; end
    
    #
    # Return the logarithm of this quaternion (a new FXQuatf instance).
    #
    def log; end
    
    #
    # Return the inverse of this quaternion (a new FXQuatf instance).
    #
    def invert; end
    
    #
    # Invert unit quaternion (returns a new FXQuatf instance).
    #
    def unitinvert; end
    
    #
    # Return the conjugate of this quaternion (a new FXQuatf instance).
    #
    def conj; end
    
    #
    # Return the product of this quaternion and _other_ (another FXQuatf instance).
    #
    def *(other); end
    
    #
    # Compute the rotation of a vector _vec_ by this quaternion; returns the
    # rotated vector (a new FXVec3f instance).
    #
    # ==== Parameters:
    #
    # +vec+::		the vector to be rotated [FXVec3f]
    #
    def *(vec); end
    
    #
    # Construct a quaternion from arc a->b on unit sphere and
    # return reference to self.
    #
    # ==== Parameters:
    #
    # +a+::	[FXVec3f]
    # +b+::	[FXVec3f]
    #
    def arc!(a, b); end
    
    #
    # Spherical lerp, return reference to self.
    #
    # ==== Parameters:
    #
    # +u+::	[FXQuatf]
    # +v+::	[FXQuatf]
    # +f+:: 	[Float]
    #
    def lerp!(u, v, f); end
  end
end
