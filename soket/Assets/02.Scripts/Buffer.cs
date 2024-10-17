using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Buffer
{
    struct Packet // 데이터를 담는 그릇.
    {
        public int pos;
        public int size;
    };

    MemoryStream stream;
    List<Packet> list;
    int pos = 0;

    Object o = new Object();

    public Buffer()
    {
        stream = new MemoryStream();
        list = new List<Packet>();
    }

    public int Write(byte[] bytes, int length)
    {
        Packet packet = new Packet();

        packet.pos = pos;
        packet.size = length;

        lock (o) // 데이터 작성시 다른 사람의 침범을 막기위해 사용.
        {
            list.Add(packet);

            stream.Position = pos;
            stream.Write(bytes, 0, length);
            stream.Flush();
            pos += length;
        }

        return length;
    }

    public int Read(ref byte[] bytes, int length)
    {
        if (list.Count <= 0)
            return -1;

        int ret = 0;
        lock (o)
        {
            Packet packet = list[0]; // 리스트의 첫번째 위치의 데이터를 읽어옴

            int dataSize = Math.Min(length, packet.size);
            stream.Position = packet.pos;
            ret = stream.Read(bytes, 0, dataSize);

            if (ret > 0)
                list.RemoveAt(0); // 리스트 첫번째 위치의 데이터를 삭제.

            if (list.Count == 0) // 데이터를 전부 읽어오면 한번 클리어.
            {
                byte[] b = stream.GetBuffer();
                Array.Clear(b, 0, b.Length);

                stream.Position = 0;
                stream.SetLength(0);

                pos = 0;
            }
        }

        return ret;
    }
}
