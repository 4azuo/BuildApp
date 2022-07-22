Imports System.Text
Imports System.Text.RegularExpressions

Module StringExt
    Public Function ConvertRawUtf8ToJapanese(text As String) As String
        Dim regex As New Regex("\\(\d{3})\\(\d{3})\\(\d{3})")
        For Each m As Match In regex.Matches(text)
            text = text.Replace(m.Value, Encoding.BigEndianUnicode.GetString(Encoding.Convert(Encoding.UTF8, Encoding.BigEndianUnicode,
                                                                                              New Byte() {CInt($"&O{m.Groups(1)}"), CInt($"&O{m.Groups(2)}"), CInt($"&O{m.Groups(3)}")})))
        Next
        'regex = New Regex("\p{IsCJKUnifiedIdeographs}")
        'For Each m As Match In regex.Matches(text)
        '    Console.WriteLine(m.Value)
        'Next
        Return text
    End Function
End Module
