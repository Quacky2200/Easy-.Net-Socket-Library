Imports System.Runtime.Serialization.Formatters.Binary

Namespace Networking

    Public Class Serialization
        ''' <summary>
        ''' Serialize an object into bytes using a MemoryStream and BinaryFormatter
        ''' </summary>
        ''' <param name="Obj">The object you want to serialize</param>
        ''' <returns>Byte()</returns>
        ''' <remarks></remarks>
        Public Shared Function SerializeObject(Obj As Object) As Byte()
            Dim sStream As New IO.MemoryStream()
            Dim bfTemp As New BinaryFormatter
            bfTemp.AssemblyFormat = Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            bfTemp.Serialize(sStream, Obj)

            ' Debug.Print("Note: SerializeObjectWithJSON is faster and uses less bandwidth. You could've saved {0} bytes", sStream.ToArray.Length - SerializeObjectWithJSON(Obj).Length)
            Return sStream.ToArray
        End Function 'SerializeObject
        ''' <summary>
        ''' Deserialize an object from bytes using a MemoryStream and BinaryFormatter
        ''' </summary>
        ''' <param name="Bytes">Bytes to convert into an object</param>
        ''' <returns>Object</returns>
        ''' <remarks></remarks>
        Public Shared Function DeserializeObject(Bytes As Byte()) As Object
            Dim bfTemp As New BinaryFormatter
            Dim sStream As New IO.MemoryStream(Bytes)
            Dim Obj As Object = bfTemp.UnsafeDeserialize(sStream, Nothing)
            ' Debug.Print("Note: SerializeObjectWithJSON is faster and uses less bandwidth. You could've saved {0} bytes", SerializeObject(Obj).Length - SerializeObjectWithJSON(Obj).Length)
            Return Obj
        End Function 'DeserializeObject
        ' ''' <summary>
        ' ''' Serialize an object into bytes with JSON
        ' ''' </summary>
        ' ''' <param name="Obj">The object to convert into bytes</param>
        ' ''' <returns>Byte()</returns>
        ' ''' <remarks></remarks>
        'Public Shared Function SerializeObjectWithJSON(Obj As Object) As Byte()
        '    Return System.Text.Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Obj))
        'End Function 'SerializeObjectWithJSON
        ' ''' <summary>
        ' ''' Deserialize an object into bytes with JSON
        ' ''' </summary>
        ' ''' <param name="Bytes">Bytes to convert into an object</param>
        ' ''' <returns>Object</returns>
        ' ''' <remarks></remarks>
        'Public Shared Function DeserializeObjectWithJSON(Bytes As Byte()) As Object
        '    Return Newtonsoft.Json.JsonConvert.DeserializeObject(System.Text.Encoding.ASCII.GetString(Bytes))
        'End Function ' DeserializeObjectWithJSON
    End Class

End Namespace