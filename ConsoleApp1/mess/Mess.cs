
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    class Mess
    {
        private byte[] m_data = new byte[10240];
        private int m_startIndex = 0;
        private int m_count = 0;     //为一条发送的字节数据的长度

        public int Count { get { return m_count; } }
        public byte[] Data { get { return m_data; } }
        public int StartIndex { get { return m_startIndex; } }
        public int RemainSize { get { return m_data.Length - m_count; } }

        /// <summary>
        /// 将字节数据与字节长度拼接
        /// </summary>
        /// <param name="_msg"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string _msg)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(_msg);
            int length = msg.Length;    //字节长度
            byte[] newMsg = BitConverter.GetBytes(length).Concat(msg).ToArray();   //将字节长度与msg拼接
            return newMsg;
        }

        /// <summary>
        /// 解析字节数据
        /// </summary>
        /// <returns></returns>
        public void ReadMessage(int _count)
        {
            m_startIndex = 4;
            m_count += _count;

            UpdataMessage();
        }

        /// <summary>
        /// 读取完成字节数据后删除第一条字节数据
        /// </summary>
        private void UpdataMessage()
        {
            while (true)
            {
                int count = BitConverter.ToInt32(m_data, 0);
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(m_data, m_startIndex, count));
                m_count -= count + 4;
                Array.Copy(m_data, count + 4, m_data, 0, m_count);
                if (m_count < count + 4) break;
            }

            m_startIndex = m_count;
        }
    }

