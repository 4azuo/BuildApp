Public Class Project
    Inherits AutoNotifiableObject

    Public ReadOnly Property Name As String
    Public ReadOnly Property Path As String
    Public ReadOnly Property IsExists As Boolean
    Public Property Mode As Integer = 0

    Public Sub New(iName As String, iPath As String, Optional iExists As Boolean = False)
        Name = iName
        Path = iPath
        IsExists = iExists
    End Sub
End Class