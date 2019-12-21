Imports System.Threading

Public Class g

    Shared sqlserverLazy As Lazy(Of IFreeSql) = New Lazy(Of IFreeSql)(New Func(Of IFreeSql)(Function() New FreeSqlBuilder() _
        .UseConnectionString(DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=3") _
        .UseAutoSyncStructure(True) _
        .UseMonitorCommand(Sub(cmd) Trace.WriteLine(vbCrLf & "线程" & Thread.CurrentThread.ManagedThreadId & ": " & cmd.CommandText)) _
        .UseLazyLoading(True) _
        .Build()))

    Public Shared ReadOnly Property sqlserver As IFreeSql
        Get
            Return sqlserverLazy.Value
        End Get
    End Property

End Class
