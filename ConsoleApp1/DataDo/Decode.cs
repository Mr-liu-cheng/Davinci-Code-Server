using System;
using System.Collections.Generic;
using System.Linq;

public class Decode
{

    public static byte[] DecodHeadBtyes(Byte[] bts)
    {
        Byte[] head = bts.Skip(0).Take(4).ToArray();
        return head;
    }

    /// <summary>
    /// 解析一级协议命令的内容
    /// </summary>
    /// <param name="bts"></param>
    /// <returns></returns>
    public static byte[] DecodeFirstContendBtyes(Byte[] bts)
    {

        Byte[] contend = bts.Skip(8).ToArray();
        return contend;
    }

    /// <summary>
    /// 解析二级协议命令的内容
    /// </summary>
    /// <param name="bts"></param>
    /// <returns></returns>
    public static byte[] DecodeSecondContendBtyes(Byte[] bts)//聊天专用（消息）
    {
        Byte[] contend = bts.Skip(12).ToArray();
        return contend;
    }

}
