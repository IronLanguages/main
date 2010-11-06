Public Class ToBool
    Public Value As Boolean
    Public Shared Widening Operator CType(ByVal arg As ToBool) As Boolean
        Return arg.Value
    End Operator
End Class
