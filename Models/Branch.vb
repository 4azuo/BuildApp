Public Class Branch
    Public ReadOnly Property Name As String
    Public ReadOnly Property FullName As String

    Public Sub New(iName As String, iFullName As String)
        Name = iName
        FullName = iFullName
    End Sub
End Class
