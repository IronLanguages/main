Public Class IsTrueIsFalse

    Public Value As Boolean

    Public Shared Operator IsTrue(ByVal arg As IsTrueIsFalse) As Boolean
        Return arg.Value
    End Operator

    Public Shared Operator IsFalse(ByVal arg As IsTrueIsFalse) As Boolean
        Return Not arg.Value
    End Operator


End Class
