Imports System.IO
Imports System.Text.RegularExpressions

Module ProccessExt
    Public Function CreateProcesser(iExecuter As String, Optional iParams As String = Nothing, Optional iCreateNoWindow As Boolean = True, Optional iInput As Boolean = False, Optional iOutput As Boolean = True, Optional iError As Boolean = True) As Process
        Dim processer As New Process
        processer.StartInfo.FileName = iExecuter
        processer.StartInfo.CreateNoWindow = iCreateNoWindow
        processer.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        processer.StartInfo.UseShellExecute = False
        If Not String.IsNullOrEmpty(iParams) Then processer.StartInfo.Arguments = iParams
        processer.StartInfo.RedirectStandardInput = iInput
        processer.StartInfo.RedirectStandardOutput = iOutput
        processer.StartInfo.RedirectStandardError = iError
        Return processer
    End Function

    Public Function GetCmdOutputs(cmd As String, allOutput As String) As String()
        Return GetCmdOutput(cmd, allOutput).Split({vbLf}, StringSplitOptions.RemoveEmptyEntries)
    End Function

    Public Function GetCmdOutput(cmd As String, allOutput As String) As String
        Dim a = allOutput.IndexOf(cmd)
        Dim b = allOutput.LastIndexOf(vbNewLine, a)
        Dim c = allOutput.IndexOf(vbNewLine, a)
        Dim cd = allOutput.Substring(b + vbNewLine.Length, a - b - vbNewLine.Length)
        Dim nextCd = allOutput.IndexOf(cd, a)
        Return If(Replace(Regex.Replace(allOutput.Substring(c, nextCd - c), $"""|^\s+|\s+$|([\r\n]+)\s+|\s+([\r\n]+)", "$1"), vbCr, ""), "")
    End Function

    Public Sub Copy(dstFolder As String, srcFolder As String, Optional ignoredTypes As String = "")
        If Not Directory.Exists(srcFolder) Then Return
        If Not Directory.Exists(dstFolder) Then Directory.CreateDirectory(dstFolder)
        For Each f As String In Directory.GetFiles(srcFolder)
            Dim fileInfo As New FileInfo(f)
            If Not ignoredTypes.Contains(fileInfo.Extension.ToLower) Then
                File.Copy(f, $"{dstFolder}\{fileInfo.Name}", True)
            End If
        Next
    End Sub
End Module
