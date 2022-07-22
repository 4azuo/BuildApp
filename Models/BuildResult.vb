Public Class BuildResult
    Public ReadOnly Property ResultCd As Integer
    Public ReadOnly Property Benchmark As TimeSpan
    Public ReadOnly Property BuildDir As String
    Public ReadOnly Property Log As LogObj

    Public Sub New(iResultCd As Integer, iBenchmark As TimeSpan, iBuildDir As String, iLog As LogObj)
        ResultCd = iResultCd
        Benchmark = iBenchmark
        BuildDir = iBuildDir
        Log = iLog
    End Sub
End Class
