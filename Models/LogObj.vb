Public Class LogObj
    Public ReadOnly Property RunnedCmd As String
    Public ReadOnly Property Name As String
    Public ReadOnly Property Path As String
    Public ReadOnly Property Details1 As String
    Public ReadOnly Property Details2 As String
    Public ReadOnly Property LogError As String

    Public Sub New(iRunnedCmd As String, iName As String, iPath As String, iDetails1 As String, iDetails2 As String, iLogError As String)
        RunnedCmd = iRunnedCmd
        Name = iName
        Path = iPath
        Details1 = iDetails1
        Details2 = iDetails2
        LogError = iLogError
    End Sub
End Class
