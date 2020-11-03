using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ConsoleApp1
{
    class MessageInfo
    {
        public string roomNum;//群聊房间号  接到消息先判断是否为群发在分析发送对象为空则是私发  也是多余的可以根据type判读私发群发，群发在获取自己所在房间号
        public int sendId;          //发送者的id
        public int toIds;     //接受者的id
        //public List<int> toIds;     //接受者的id

        public bool isReaded;   //是否已读   不需要的字段
        public string content;//内容
        public float disposeTime;//处理时间
                                 //public static List<Message> unReadMsg = new List<Message>(); //未读邮件 
        public static List<MessageInfo> personalMsg = new List<MessageInfo>();//个人消息缓存 int好友id
        //public static Dictionary<int, List<Message>> massage = new Dictionary<int, List<Message>>();//所有消息缓存 int好友id
    }
}
