using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PDFDocument
{
    public Guid Id { get; set; }
    public List<byte[]> Pages { get; set; }

    public PDFDocument()
    {
        Id = Guid.NewGuid();
        Pages = new List<byte[]>();
    }

    public PDFDocument(Guid id, List<byte[]> pages)
    {
        Id = id;
        Pages = pages;
    }

    public static PDFDocument FromByteArray(byte[] bytes)
    {
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = (PDFDocument)binForm.Deserialize(memStream);
            return obj;
        }
    }

    public byte[] ToByteArray()
    {
        var bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, this);
            return ms.ToArray();
        }
    }
}