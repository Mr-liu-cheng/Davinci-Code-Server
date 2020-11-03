using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace ConsoleApp1
{
    /// <summary>
    /// 房间信息是在客户端创建初始化发过来的  【房间信息的修改要响应到客户端】
    /// </summary>
    public class RoomInfo
    {
        public string roomID;
        public const int MaxSIZE = 4;//房间最大容量
        /// <summary>
        /// 房间成员（房间信息是在客户端创建初始化发过来的）
        /// </summary>
        public List<PersonalInfo> member;//成员id
        private bool isbegin;
        public int host_Id;//房主id//也作为房间的id
        public string roomName;
        public string icon;
        private int timer=0;//时间
        int currId;
        public bool isStartTime;

        //public enum Status
        //{
        //    Creat,
        //    Group,
        //    Fight,
        //    Destroy
        //}

        /// <summary>
        /// 临时成员，玩家出局就从列表中移除
        /// </summary>
        public List<int> tempMember;

        /// <summary>
        /// 记录当前出牌玩家的下标
        /// </summary>
        public int curr_playerIndex;

        /// <summary>
        /// 是否结束出牌，切换至下一家
        /// </summary>
        public bool isSwitch;

        /// <summary>
        /// 记录有多少人已发送发牌的请求
        /// </summary>
        public List<int> sealers;

        /// <summary>
        /// 牌库（所有牌）（根据牌的归属划分）
        /// </summary>
        public List<BaseCard> cardsLibrary;

        /// <summary>
        /// 玩家手牌
        /// </summary>
        public List<BaseCard> playersCards;

        List<int> exitList;

        Thread thread;

        /// <summary>
        /// 计时器【与客户端计时方式相反  服务器是顺序累加计时直到指定值，客户端时倒计时递减直到为0】
        /// </summary>
        public int Time 
        { 
            get => timer; 
            set 
            {
                timer = value; 
                if (timer== (int)GameCommand.Time._SingleDrawTime + (int)GameCommand.Time._SelectMyselfCardTime) 
                {
                    //下一位
                    DoNext();
                    timer = 0;
                }
            } 
        }

        /// <summary>
        /// 是否开启【创建房间和结束战斗回到房间界面赋予false、开始游戏为true】
        /// </summary>
        public bool Isbegin { 
            get => isbegin;
            set
            {
                #region MyRegion


                //switch (value)
                //{
                //    case Status.Creat:

                //        break;

                //    case Status.Group:
                //        RoomCommand.freeRooms.Add(roomID, this);
                //        for (int i = 0; i < member.Count(); i++)
                //        {
                //            PersonalInfo.ChangeStatusInfo(member[i].id, roomID, (int)PersonStatus.Combine);
                //        }
                //        break;

                //    case Status.Fight:
                //        RoomCommand.freeRooms.Remove(roomID);//房间移除分两种情况开始和未开始
                //        foreach (var person in member)
                //        {
                //            PersonalInfo.ChangeStatusInfo(person.id, roomID, (int)PersonStatus.Fighting);
                //        }
                //        break;

                //    case Status.Destroy:

                //        break;
                //}
                #endregion

                if (value)
                {
                    RoomCommand.freeRooms.Remove(roomID);//房间移除分两种情况开始和未开始
                    foreach (var person in member)
                    {
                        PersonalInfo.ChangeStatusInfo(person.id, roomID, (int)PersonStatus.Fighting);
                    }
                }
                else//数据清空
                {
                    if (playersCards!=null)
                    {
                        playersCards.Clear();
                    }
                    if (exitList != null)
                    {
                        exitList.Clear();
                    }
                    RoomCommand.freeRooms.Add(roomID,this);
                    for (int i = 0; i < member.Count(); i++)
                    {
                        PersonalInfo.ChangeStatusInfo(member[i].id, roomID, (int)PersonStatus.Combine);
                    }
                }
                isbegin = value;
            }
        }

        public void CreateExitList()
        {
            exitList = new List<int>();
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        public void AddMember(PersonalInfo personalInfo)
        {
            member.Add(personalInfo);
            //tempMember.Add(personalInfo.id);
            if (member.Count()== MaxSIZE)
            {
                RoomCommand.freeRooms.Remove(roomID);
            }
        }

        /// <summary>
        /// 移除成员
        /// </summary>
        public void RemoveMember(int id)
        {
            for (int i = 0; i < member.Count; i++)
            {
                if (member[i].id == id)
                {
                    member.RemoveAt(i);//不能直接删除personalInfo因为修改过值已经不是同一个了
                    if (host_Id == id)//房主退出
                    {
                        if (member.Count>0)
                        {
                            host_Id = member[0].id;//房主转移
                            roomName = host_Id + "的房间";
                        }
                    }
                }
            }

            RemoveTempMember(id);
    
            if (!Isbegin && member.Count>=0)
            {
                if (member.Count() < MaxSIZE)
                {
                    if (!RoomCommand.freeRooms.ContainsKey(roomID))
                    {
                        RoomCommand.freeRooms.Add(roomID,this);
                    }
                }
                RoomCommand.ForeachSendRoom((int)RoomCommand.SecondCommands.EXITROOM, this);
            }
            DestoryRoom();
        }

        /// <summary>
        /// 移除临时活跃成员
        /// </summary>
        public void RemoveTempMember(int id)
        {
            tempMember.Remove(id);
            int removeIndex = tempMember.FindIndex(val => {
                if (val == id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (removeIndex <= curr_playerIndex)
            {
                if (removeIndex == curr_playerIndex)
                {
                    ConsoleHelper.WriteColorLine("离线者为当前出牌者", ConsoleColor.Red);
                }
                else
                {
                    ConsoleHelper.WriteColorLine("离线者为当前出牌者的前者", ConsoleColor.Green);
                }
                if (tempMember.Count>0)
                {
                    ConsoleHelper.WriteColorLine("计算前："+ curr_playerIndex, ConsoleColor.Yellow);
                    curr_playerIndex = (curr_playerIndex + tempMember.Count-1) % tempMember.Count;
                    ConsoleHelper.WriteColorLine("计算后：" + curr_playerIndex, ConsoleColor.Yellow);
                }
            }
            else
            {
                ConsoleHelper.WriteColorLine("离线者为当前出牌者的后者", ConsoleColor.Yellow);
            }


            if (Isbegin)
            {
                exitList.Add(id);

                if (tempMember.Count >= 1)
                {
                    Console.WriteLine("++++++出局者是否为当前出牌者+++++   CurrID:" + currId+ "   出局者ID:" + id);
                    if (id == currId && tempMember.Count != 1)//
                    {
                        ConsoleHelper.WriteColorLine("当前离线玩家==当前出牌者",ConsoleColor.Red);
                        DoNext();
                        Time = 0;
                    }
                    SendID(GameCommand.SecondCommands.Out, BitConverter.GetBytes(id));
                    SendID(GameCommand.SecondCommands.AutoOpenOnesCard, BitConverter.GetBytes(id));

                    GameOverCheck();
                }
            }
        }

        /// <summary>
        /// 游戏结束的检测【停止线程，发送结束信号】
        /// </summary>
        void GameOverCheck()
        {
            if (tempMember.Count == 1)
            {
                StopTimer();
                exitList.Add(tempMember[0]);
                GameCommand.GameOver(this, exitList);
                Isbegin = false;
            }
        }

        /// <summary>
        /// 销毁房间
        /// </summary>
        void DestoryRoom()
        {
            if (member.Count == 0)
            {
                RoomCommand.allRooms.Remove(roomID);

                if (RoomCommand.freeRooms.ContainsKey(roomID))
                {
                    RoomCommand.freeRooms.Remove(roomID);
                }
            }
        }

        public void WaitToDo(GameCommand.Time time)
        {
            isStartTime = true;
            if (member.Count() > 1)
            {
                ElapsedEventHandler eventHandler=null;
                switch (time)
                {
                    case GameCommand.Time._1stDrawTime:
                        eventHandler = Deal;
                        break;
                    case GameCommand.Time._MoveCardTime:
                        eventHandler = StartMoveCardTimer;
                        break;
                    case GameCommand.Time._DiceTime:
                        eventHandler = StartTimer;
                        break;
                }
            
                System.Timers.Timer timer = new System.Timers.Timer((int)time * 1000)
                {
                    AutoReset = false
                };
                timer.Elapsed += eventHandler;
                timer.Start();
            }
        }

        /// <summary>
        /// 执行首摸计时器任务
        /// </summary>
        void Deal(object sender, ElapsedEventArgs e)
        {
            if (member.Count() > 1)
            {
                if (sealers.Count<tempMember.Count)
                {
                    for (int i = 0; i < tempMember.Count; i++)
                    {
                        //补发【针对客户端性能差导致服务器倒点发送空集合（玩家手牌）】
                        if (!sealers.Contains(tempMember[i]))
                        {
                            Console.WriteLine("玩家"+ tempMember[i]+"的电脑卡没及时发送首摸信息，代执行");
                            GameCommand.DoSeal(this, new FirstDrawInfo {blackCards=0,whiteCards=0,myId= tempMember[i]});
                        }
                    }
                    sealers.Clear();
                }
                GameCommand.ForeachSendRoom((int)GameCommand.SecondCommands.DEAL, this);
                Console.WriteLine("当前房间："+ this.roomID +"    手牌："+playersCards.Count+"    牌库："+cardsLibrary.Count);
                WaitToDo(GameCommand.Time._MoveCardTime);//不管所有玩家手里是否有万能牌都要等这个时间
                Console.WriteLine(System.DateTime.Now + "执行首摸计时器任务");
            }
        }

        /// <summary>
        /// 启动计时器
        /// </summary>
        void StartMoveCardTimer(object sender, ElapsedEventArgs e)
        {
            if (member.Count()>1)
            {
                GameCommand.SetForthgoer(this);
                Console.WriteLine(System.DateTime.Now + "执行移动万能牌的计时器");
            }
        }

        /// <summary>
        /// 启动计时器
        /// </summary>
        void StartTimer(object sender, ElapsedEventArgs e)
        {
            if (member.Count() > 1)
            {
                GameCommand.Next(this);
                Console.WriteLine("Next-------------StartTimer【计时器激活处】");

                Console.WriteLine(System.DateTime.Now + "执行计时器");
                thread = new Thread(new ThreadStart(RefreshNext));
                thread.Start();
            }
        }

        /// <summary>
        /// 关闭计时器
        /// </summary>
        void StopTimer()
        {
            if (thread != null)
            {
                thread.Abort();
                Console.WriteLine("终止房间线程");
            }
        }

        /// <summary>
        /// 定时刷新下一位出牌者（回合推动器）
        /// </summary>
        void RefreshNext()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Time++;
                //Console.WriteLine((GameCommand._SingleDrawTime+GameCommand._SelectMyselfCardTime- GameCommand._SelectMyselfCardTime) - Time+"  ==  "+DateTime.Now);
            }
        }

        /// <summary>
        /// 执行下一位操作
        /// </summary>
        /// <param name="offset">当出局/离线时，offset为当前：0，正常情况为1</param>
        void DoNext()
        {

            DestoryRoom();
            curr_playerIndex = (curr_playerIndex + 1+ tempMember.Count) % tempMember.Count;
            currId = tempMember[curr_playerIndex];

            Console.WriteLine("下一位下标：" + curr_playerIndex + "  id: " + currId + "    ---"+DateTime.Now);
            SendID(GameCommand.SecondCommands.PASS, BitConverter.GetBytes(tempMember[curr_playerIndex]));
        }

        /// <summary>
        /// 发送id
        /// </summary>
        void SendID(GameCommand.SecondCommands secondCommands,byte[] id)
        {
            Conn conn;
            for (int i = 0; i < member.Count; i++)
            {
                Server.connMap.TryGetValue(member[i].id.ToString(), out conn);
                //游戏命令 下一位
                if (conn != null)
                {
                    byte[] data = Incode.IncodeSecondaryCommand(GameCommand.type, (int)secondCommands, id);
                    conn.SendBts(data);
                }
            }
            //Console.WriteLine("------发送玩家离线信号-----");
        }
    }
}
