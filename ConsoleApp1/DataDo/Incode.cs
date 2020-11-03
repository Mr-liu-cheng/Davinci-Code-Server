using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Incode
{
    /// <summary>
    /// 二级协议（含有子命令，封装在数据内容里）
    /// </summary>
    /// <param name="type">一级命令</param>
    /// <param name="length">能容长度</param>
    /// <param name="secondCmd">二级命令</param>
    /// <param name="contend">内容</param>
    /// <returns></returns>
    public static byte[] IncodeSecondaryCommand(int type, int secondCmd, byte[] contend)
    {
        byte[] allBts;
        byte[] head = BitConverter.GetBytes(type);
        byte[] lenth = BitConverter.GetBytes(sizeof(Int32)+ contend.Length);
        byte[] secondTypes = BitConverter.GetBytes(secondCmd);
        byte[] conStr = contend;
        allBts = new byte[head.Length + lenth.Length + secondTypes.Length + conStr.Length];

        Buffer.BlockCopy(head, 0, allBts, 0, 4);
        Buffer.BlockCopy(lenth, 0, allBts, 4, 4);
        Buffer.BlockCopy(secondTypes, 0, allBts, 8, 4);
        Buffer.BlockCopy(conStr, 0, allBts, 12, conStr.Length);
        return allBts;
    }

    /// <summary>
    /// 一级协议（命令+长度+数据）
    /// </summary>
    /// <param name="type"></param>
    /// <param name="ctd_length"></param>
    /// <param name="contend"></param>
    /// <returns></returns>
    public static byte[] IncodeFirstCommand(int type, byte[] contend)
    {
        byte[] allBts;
        byte[] head = BitConverter.GetBytes(type);
        byte[] ctdLength = BitConverter.GetBytes(contend.Length);
        byte[] conStr = contend;
        allBts = new byte[head.Length + ctdLength.Length + conStr.Length];

        Buffer.BlockCopy(head, 0, allBts, 0, 4);
        Buffer.BlockCopy(ctdLength, 0, allBts, 4, 4);
        Buffer.BlockCopy(conStr, 0, allBts, 8, conStr.Length);
        return allBts;
    }
 }
