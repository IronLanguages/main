' **********************************************************************************
'
'  Copyright (c) Microsoft Corporation. All rights reserved.
'
' This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
' copy of the license can be found in the License.html file at the root of this distribution. If 
' you cannot locate the  Apache License, Version 2.0, please send an email to 
' ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
' by the terms of the Apache License, Version 2.0.
'
' You must not remove this notice, or any other, from this software.
'
'
' **********************************************************************************


Imports System
Imports System.Collections

Public Delegate Function Transformer(ByVal input As Integer) As Integer

Public Class SimpleEnum
    Implements IEnumerator

    Private data As Integer
    Private curr As Integer

    Public Sub New(ByVal data As Integer)
        Me.data = data
        Me.curr = -1
    End Sub

    Public ReadOnly Property Current() As Object _
            Implements IEnumerator.Current
        Get
            Return New Simple(curr)
        End Get
    End Property

    Public Function MoveNext() As Boolean _
                Implements IEnumerator.MoveNext
        curr += 1
        Return curr < data
    End Function

    Public Sub Reset() Implements IEnumerator.Reset
        curr = -1
    End Sub
End Class

Public Class Simple
    Implements IEnumerable
    Private data As Integer

    Public Sub New(ByVal data As Integer)
        Me.data = data
    End Sub

    Overrides Function ToString() As String
        Return String.Format("Simple<{0}>", data)
    End Function

    Function GetEnumerator() As IEnumerator _
                Implements IEnumerable.GetEnumerator
        Return New SimpleEnum(data)
    End Function

    Function Transform(ByVal t As Transformer) As Integer
        Return t(data)
    End Function

    Shared Operator +(ByVal a As Simple, ByVal b As Simple) As Simple
        Return New Simple(a.data + b.data)
    End Operator

End Class