Imports System

Module Program
    Sub Main(args As String())
        Console.WriteLine("Hello World!")

        Dim fsql = New FreeSql.FreeSqlBuilder() _
            .UseConnectionString(FreeSql.DataType.Sqlite, "data source=testvb.db") _
            .UseAutoSyncStructure(True) _
            .UseMonitorCommand(Sub(cmd) Trace.WriteLine(cmd.CommandText)) _
        .Build()

        REM Microsoft.VisualBasic.CompilerServices.Operators.CompareString()
        Dim List1 = fsql.Select(Of Testvb).Where(Function(a) a.Id = 100).ToList()
        Dim List2 = fsql.Select(Of Testvb).Where(Function(a) a.Title = "xxx").ToList()
        Dim List3 = fsql.Select(Of Testvb).Where(Function(a) a.Title <> "xxx").ToList()

        fsql.Dispose()
    End Sub
End Module

Class Testvb
    Property Id As Integer
    Property Title As String
End Class

