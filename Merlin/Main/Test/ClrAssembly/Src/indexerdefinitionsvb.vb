' ****************************************************************************
' 
'  Copyright (c) Microsoft Corporation. 
' 
'  This source code is subject to terms and conditions of the Microsoft Public License. A 
'  copy of the license can be found in the License.html file at the root of this distribution. If 
'  you cannot locate the  Microsoft Public License, please send an email to 
'  ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
'  by the terms of the Microsoft Public License.
' 
'  You must not remove this notice, or any other, from this software.
' 
' 
' ***************************************************************************

Imports System.Reflection

Namespace Merlin.Testing.Indexer
  Public Class ClassWithIndexer
    Private array As Integer()

    Sub Init()
      array = New Integer() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
    End Sub

    Public Property PropertyName(ByVal arg As Integer) As Integer
      Get
        Return array(arg)
      End Get
      Set(ByVal value As Integer)
        array(arg) = value
      End Set
    End Property
  End Class

  Public Structure StructWithIndexer
    Private array As Integer()

    Sub Init()
      array = New Integer() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
    End Sub
    Public Property PropertyName(ByVal arg As Integer) As Integer
      Get
        Return array(arg)
      End Get
      Set(ByVal value As Integer)
        array(arg) = value
      End Set
    End Property
  End Structure

  Public Class ClassWithSignature
    Private array As Integer() = New Integer() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}

    Public Property PropertyName(ByVal arg1 As Integer, Optional ByVal arg2 As Integer = 2) As Integer
      Get
        Return array(arg1 + arg2)
      End Get
      Set(ByVal value As Integer)
        array(arg1 + arg2) = value
      End Set
    End Property
  End Class

  Public Class ClassWithOnlyOptional
    Private array As Integer() = New Integer() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
    Public Property PropertyName(Optional ByVal arg As Integer = 2) As Integer
      Get
        Return array(arg)
      End Get
      Set(ByVal value As Integer)
        array(arg) = value
      End Set
    End Property
  End Class

  Public Class ClassWithOnlyParamArray
    Private saved As Integer = -99
    Private array As Integer() = New Integer() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}

    Public Property PropertyName(ByVal ParamArray arg As Integer()) As Integer
      Get
        If arg.Length = 0 Then
          Return saved
        Else
          Return array(arg(0))
        End If
      End Get
      Set(ByVal value As Integer)
        If arg.Length = 0 Then
          saved = value
        Else
          array(arg(0)) = value
        End If
      End Set
    End Property
  End Class

  'Public Class C
  '  Sub test()
  '    Dim x As ClassWithOnlyOptional = New ClassWithOnlyOptional()
  '    Dim y As Integer = x.PropertyName()
  '  End Sub
  'End Class

  Public Class ClassWithStaticIndexer
    Private Shared saved As Integer
    Public Shared Property PropertyName(ByVal arg As Integer) As Integer
      Get
        Return saved + arg
      End Get
      Set(ByVal value As Integer)
        saved = arg + value
      End Set
    End Property
  End Class

  Public Class ClassWithStaticIndexer2
    Private Shared saved As Integer
    Public Shared Property PropertyName(ByVal arg As Integer) As Integer
      Get
        Return saved + arg
      End Get
      Set(ByVal value As Integer)
        saved = arg + value
      End Set
    End Property
  End Class

    Public Class ClassWithOverloadedIndexers
        Private array As Integer()

        Sub Init()
            array = New Integer() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
        End Sub

        Public Property PropertyName(ByVal arg As Integer) As Integer
            Get
                Return array(arg)
            End Get
            Set(ByVal value As Integer)
                array(arg) = value
            End Set
        End Property

        Public Property PropertyName() As Integer
            Get
                Return array(2)
            End Get
            Set(ByVal value As Integer)
                array(2) = value
            End Set
        End Property
    End Class

End Namespace