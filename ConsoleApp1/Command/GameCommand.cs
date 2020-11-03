using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{

    public struct Info
    {
        public int myId;
        public string roomId;
        public byte[] content;
    }

    /// <summary>
    /// 单摸桌牌信息
    /// </summary>
    public struct SingleDraw
    {
        /// <summary>
        /// 托管判断
        /// </summary>
        public bool autoTrusteeship;
        public string roomNum;
        public int myId;
        public int myIndex;
        public int cardIndex;
        /// <summary>
        /// 是否是连续猜牌
        /// </summary>
        public bool isContinueGuess;

    }

    /// <summary>
    /// 摸牌信息
    /// </summary>
    public struct FirstDrawInfo
    {
        public string roomNum;
        public int myId;
        /// <summary>
        /// 黑牌数量
        /// </summary>
        public int blackCards;
        public int whiteCards;

    }

    /// <summary>
    /// 猜牌时发送的信息
    /// </summary>
    public struct GuessInfo
    {
        /// <summary>
        /// 要被删除牌的下标（选项窗口里的牌）
        /// </summary>
        public int delateIndex;
        /// <summary>
        /// 摸牌者的下标
        /// </summary>
        public int drawerIndex;
        /// <summary>
        /// 猜错时需要
        /// </summary>
        public BaseCard card;
        /// <summary>
        /// 被选中的玩家的下标
        /// </summary>
        public int whoIndex;
        /// <summary>
        /// 被选中牌的下标
        /// </summary>
        public int cardIndex;
        public string roomNum;
        public bool isGuessTrue;
        /// <summary>
        /// 是否是自动发送【只摸牌没猜牌时】
        /// </summary>
        public bool isAutoSend;
        /// <summary>
        /// 万能牌的插入位置
        /// </summary>
        public int lineCardSeatIndex;
    }

    public class GameCommand : Command
    {
        public static readonly int type = (int)FirstCommands.GameCommand;//游戏命令

        /// <summary>
        /// 服务器与客户端二级命令不一致，不能全部粘贴过来
        /// </summary>
        public enum SecondCommands
        {
            /// <summary>
            /// 初始化
            /// </summary>
            INITIALIZE = 0,
            BEGINGAME,
            /// <summary>
            /// 发牌
            /// </summary>
            DEAL,
            /// <summary>
            /// 猜先
            /// </summary>
            TOSS,
            /// <summary>
            /// 过 下一位
            /// </summary>
            PASS,//可能要加一个Next区分下一位和发送（房间自己刷新下一位、猜对放弃、猜错）
            /// <summary>
            /// 摸牌
            /// </summary>
            DRAW,
            GUESS_T,
            GUESS_F,
            SelectMyCardToOpen,
            AutoOpenOnesCard,
            Out,
            Over,
            CreateSelectMyCardTimer,
            Continue,
            Pass,
            MoveLineCardInFirstDraw,
            OffLine,
            Already
        };

        //时间值不能相同

        public enum Time
        {
            /// <summary>
            /// 首摸时间【服务器的时间要比客户端的时间长1秒，为了更方便执行发牌命令】
            /// </summary>
            _1stDrawTime = 4,
            /// <summary>
            /// 单摸+猜牌的时间
            /// </summary>
            _SingleDrawTime = 15,
            /// <summary>
            /// 骰子动画的时间  
            /// </summary>
            _DiceTime = 2,
            /// <summary>
            /// 首摸移动万能牌的时间  
            /// </summary>
            _MoveCardTime = 3,
            /// <summary>
            /// 自选公布牌的时间
            /// </summary>
            _SelectMyselfCardTime = 10,
            /// <summary>
            /// 猜对的延长时间
            /// </summary>
            _ExtraTime = 7,
            /// <summary>
            /// 按钮选择时间【猜对】
            /// </summary>
            _ClickThinkingTime = 3,
        }

        public override void Init(byte[] bts, Conn cnn)
        {
            base.Init(bts, cnn);
        }

        public override void DoCommand()
        {
            int command = BitConverter.ToInt32(bytes, 8);
            switch (command)
            {
                case 0:
                    //暂无
                    break;
                case 1:
                    Console.WriteLine("开始游戏-------创建一副牌，洗牌");
                    BeginGame();
                    break;
                case 2:
                    Console.WriteLine("接收到发牌命令-----处理 发牌 和 猜先 + 开始轮回（计时 + 下一位）");
                    DealCards();
                    break;
                //case 3:
                //    Console.WriteLine("猜先");
                //    break;
                case 4:
                //Console.WriteLine("pass");
                ////Next();
                //break;
                case 5:
                    Console.WriteLine("摸牌    " + DateTime.Now);
                    Draw();
                    break;
                case 6:
                    Console.WriteLine("猜牌结果");
                    SendResult2Other();
                    break;
                case 7:
                    Console.WriteLine("-----------玩家自选了要公布的牌");
                    OpenSelfSelectCard();
                    break;
                case 8:
                    Console.WriteLine("自动公布牌");
                    SendID((int)SecondCommands.AutoOpenOnesCard);
                    break;
                case 9:
                    Console.WriteLine("-------------------接收到" + BitConverter.ToInt32(bytes, 12) + "出局要求");
                    SendID((int)SecondCommands.Out);
                    break;
                case 10:
                    Console.WriteLine("-----------结束");

                    break;
                case 11:
                    Console.WriteLine("-----------创建自选计时器");
                    CreatSelectSelfCardTimer();
                    break;
                case 12:
                    Console.WriteLine("-----------玩家要继续猜牌");
                    Continue();
                    break;
                case 13:
                    Console.WriteLine("-----------玩家选择【过】");
                    Pass();
                    break;
                case 14:
                    Console.WriteLine("-----------首摸移动万能牌");
                    SendLineCardMoveInfo();
                    break;
                case 15:
                    Console.WriteLine("-----------准备好……");
                    RoomInfo roomInfo = GetRoom(Encoding.UTF8.GetString(Decode.DecodeSecondContendBtyes(bytes)));
                    if (!roomInfo.isStartTime)
                    {
                        roomInfo.WaitToDo(Time._1stDrawTime);
                    }
                    break;
            }
        }

        /// <summary>
        /// 根据roomId获取房间对象
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public static RoomInfo GetRoom(string roomId)
        {
            RoomInfo roomInfo;
            RoomCommand.allRooms.TryGetValue(roomId, out roomInfo);
            return roomInfo;
        }

        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="roomInfo"></param>
        /// <param name="secondCommand"></param>
        /// <param name="content"></param>
        void Broadcast(RoomInfo roomInfo, SecondCommands secondCommand, byte[] content)
        {
            if (roomInfo == null)
            {
                Console.WriteLine("该房间不存在");
                return;
            }
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, (int)secondCommand, content);
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 发送万能牌移动信息
        /// </summary>
        void SendLineCardMoveInfo()
        {
            byte[] content = Decode.DecodeSecondContendBtyes(bytes);
            Info info = DataDo.Json2Object<Info>(content);
            Broadcast(GetRoom(info.roomId), SecondCommands.MoveLineCardInFirstDraw, content);
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        /// <param name="_1stDrawTime">首摸时间</param>
        /// <param name="_SingleDrawTime">单摸时间</param>
        /// <param name="roomInfo">房间</param>
        void Initialize(RoomInfo roomInfo)
        {
            Dictionary<string, int> times = new Dictionary<string, int>();
            times.Add("_1stDrawTime", (int)Time._1stDrawTime - 1);
            times.Add("_SingleDrawTime", (int)Time._SingleDrawTime);
            times.Add("_DiceTime", (int)Time._DiceTime);
            times.Add("_SelectMyselfCardTime", (int)Time._SelectMyselfCardTime);
            times.Add("_ExtraTime", (int)Time._ExtraTime);
            times.Add("_ClickThinkingTime", (int)Time._ClickThinkingTime);
            times.Add("_MoveCardTime", (int)Time._MoveCardTime);
            
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                roomInfo.member[i].IsInWaitRoom = false;
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, (int)SecondCommands.INITIALIZE, DataDo.Object2Json(times));
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 开局（创建牌，洗牌）
        /// </summary>
        void BeginGame()
        {
            string roomID = System.Text.Encoding.UTF8.GetString(Decode.DecodeSecondContendBtyes(bytes));
            RoomInfo roomInfo = GetRoom(roomID);
            if (roomInfo != null)
            {
                roomInfo.Isbegin = true;
                roomInfo.isStartTime = false;
                Initialize(roomInfo);
                //重新添加临时成员
                roomInfo.tempMember.Clear();
                for (int i = 0; i < roomInfo.member.Count; i++)
                {
                    roomInfo.tempMember.Add(roomInfo.member[i].id);
                }
                roomInfo.cardsLibrary = MixUpCards(CreatCardLibrary());

                ForeachSendRoom((int)SecondCommands.BEGINGAME, roomInfo);
                //修改所有成员的状态
            }
        }

        /// <summary>
        /// 玩家自选计时器
        /// </summary>
        void CreatSelectSelfCardTimer()
        {
            string roomID = System.Text.Encoding.UTF8.GetString(Decode.DecodeSecondContendBtyes(bytes));
            RoomInfo roomInfo = GetRoom(roomID);
            if (roomInfo != null)
            {
                //将当前选派人发出去
                ForeachSendOneOpen((int)SecondCommands.CreateSelectMyCardTimer, roomInfo, null);
            }
        }

        /// <summary>
        /// 出局
        /// </summary>
        void Out(RoomInfo roomInfo, int id)
        {
            //将服务器的房间内的玩家移除
            roomInfo.RemoveTempMember(id);
        }

        /// <summary>
        /// 获取随机数  ( 使用Guid产生的种子生成真随机数 )
        /// </summary>
        /// <param name="min">包含最小值</param>
        /// <param name="max">不包含最大值</param>
        public static int GetRandom(int min, int max)
        {
            // 使用Guid生成种子
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int index = random.Next(min, max);//  min=<index<max
            return index;
        }

        /// <summary>
        /// 发牌
        /// </summary>
        void DealCards()
        {
            FirstDrawInfo firstDrawInfo = DataDo.Json2Object<FirstDrawInfo>(Decode.DecodeSecondContendBtyes(bytes));
            Console.WriteLine("白牌的数目：" + firstDrawInfo.blackCards + "     白牌的数目：" + firstDrawInfo.whiteCards);
            RoomInfo roomInfo = GetRoom(firstDrawInfo.roomNum);
            if (roomInfo != null)
            {
                roomInfo.sealers.Add(firstDrawInfo.myId);
                DoSeal(roomInfo, firstDrawInfo);
            }
        }

       static public void DoSeal(RoomInfo roomInfo, FirstDrawInfo firstDrawInfo)
        {
            //首摸是空
            if (roomInfo.playersCards == null)
            {
                roomInfo.playersCards = new List<BaseCard>();//先分配空间，后面添加才有地方存 不能删
            }

    
            #region 玩家自选
            if (firstDrawInfo.blackCards > 0)
            {
                DoDeal(firstDrawInfo.blackCards, roomInfo, firstDrawInfo.myId, CardColors.BLACK);
            }
            if (firstDrawInfo.whiteCards > 0)
            {
                DoDeal(firstDrawInfo.whiteCards, roomInfo, firstDrawInfo.myId, CardColors.WHITE);
            }
            #endregion




            #region 系统随机摸牌

            int drawCardCount=0;//允许摸牌数
            switch (roomInfo.member.Count)
            {
                case 2:
                    drawCardCount = 4;
                    break;
                case 3:
                    drawCardCount = 4;
                    break;
                case 4:
                    drawCardCount = 3;
                    break;
            }
            if (firstDrawInfo.blackCards + firstDrawInfo.whiteCards < drawCardCount)
            {
                //剩余的牌（玩家还没选够,交由服务器系统随机抽）
                int margin = drawCardCount - (firstDrawInfo.blackCards + firstDrawInfo.whiteCards);
                DoDeal(margin, roomInfo, firstDrawInfo.myId);
            }
            #endregion
        }

        /// <summary>
        /// 猜先
        /// </summary>
        /// <param name="roomInfo"></param>
        public static void SetForthgoer(RoomInfo roomInfo)
        {
            //从1到6
            int[] dices = { GetRandom(1, 7), GetRandom(1, 7) };//获取随机数
            Console.WriteLine("dices[0]:" + dices[0] + "      dices[1]:" + dices[1]);// 输出生成的随机数
            roomInfo.curr_playerIndex = (dices[0] + dices[1]) % roomInfo.member.Count - 1;
            //Console.WriteLine("当前玩家：" + roomInfo.curr_playerIndex + ": " + roomInfo.member[roomInfo.curr_playerIndex].id);
            ForeachSendForthgoer((int)SecondCommands.TOSS, roomInfo, dices);//发送猜先结果

            ToStartWhile(roomInfo);//轮回开始     
        }

        /// <summary>
        /// 启动游戏房间的轮回
        /// </summary>
        public static void ToStartWhile(RoomInfo room)
        {
            if (room != null)
            {
                room.WaitToDo(Time._DiceTime);
            }
        }

        /// <summary>
        /// 抽取指定 颜色 及 张数 的牌
        /// </summary>
        /// <param name="colorCardsCount">指定颜色的牌数</param>
        /// <param name="roomInfo">房间信息（修改手牌和牌库）</param>
        /// <param name="id">玩家id</param>
        /// <param name="cardColors">对应牌色</param>
        public static void DoDeal(int colorCardsCount, RoomInfo roomInfo, int id, CardColors cardColors)
        {
            for (int i = 0; i < colorCardsCount; i++)
            {
                BaseCard card = roomInfo.cardsLibrary.Find(it =>
                {
                    if (it.CardColor == cardColors)//找到满足条件的牌
                    {
                        return true;
                    }
                    else return false;
                });
                int index = roomInfo.member.FindIndex(it =>
                 {
                     if (it.id == id)//找到该玩家在成员列表中的下标（索引）
                    {
                         return true;
                     }
                     else return false;
                 });
                roomInfo.cardsLibrary.Remove(card);
                card.CardBelongTo = (CardBelongTo)(index + 1);//为每个玩家发牌 0是牌库
                roomInfo.playersCards.Add(card);
            }
        }

        /// <summary>
        /// 抽取指定 张数 的牌
        /// </summary>
        /// <param name="colorCardsCount">指定颜色的牌数</param>
        /// <param name="roomInfo">房间信息（修改手牌和牌库）</param>
        /// <param name="id">玩家id</param>
        public static void DoDeal(int colorCardsCount, RoomInfo roomInfo, int id)
        {
            for (int i = 0; i < colorCardsCount; i++)
            {
                #region 测试代码


                /////////////////////////////////////////////////////////////    测试代码   ///////////////////////////////
                //if (id==1)
                //{
                //    BaseCard tempCard=null;
                //    foreach (var item in roomInfo.cardsLibrary)
                //    {
                //        if (item.CardWeight == CardWeight.Line)
                //        {
                //            tempCard = item;
                //        }
                //    }
                //    BaseCard card;
                //    if (tempCard!=null)
                //    {
                //        card = tempCard;
                //    }
                //    else
                //    {
                //       card = roomInfo.cardsLibrary[0];
                //    }
                //    int index = roomInfo.member.FindIndex(it =>
                //    {
                //        if (it.id == 1)//找到该玩家在成员列表中的下标（索引）
                //        {
                //            return true;
                //        }
                //        else return false;
                //    });
                //    roomInfo.cardsLibrary.Remove(card);
                //    card.CardBelongTo = (CardBelongTo)(index + 1);//为每个玩家发牌 0是牌库 成员的0代表所属的1
                //    roomInfo.playersCards.Add(card);
                //}
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  else
                #endregion
                {
                    BaseCard card = roomInfo.cardsLibrary[0];
                    int index = roomInfo.member.FindIndex(it =>
                    {
                        if (it.id == id)//找到该玩家在成员列表中的下标（索引）
                        {
                            return true;
                        }
                        else return false;
                    });
                    roomInfo.cardsLibrary.Remove(card);
                    card.CardBelongTo = (CardBelongTo)(index + 1);//为每个玩家发牌 0是牌库 成员的0代表所属的1
                    roomInfo.playersCards.Add(card);
                }
            }
        }

        #region MyRegion
        /// <summary>
        /// 发牌
        /// </summary>
        //void Deal(RoomInfo roomInfo)
        //{
        //    int cards = 0;
        //    if (roomInfo.member.Count == 3 || roomInfo.member.Count == 2)
        //    {
        //        cards = 4;
        //    }
        //    else if (roomInfo.member.Count == 4)
        //    {
        //        cards = 3;
        //    }

        //    if (cards == 4 || cards == 3)
        //    {
        //        BaseCard card;
        //        roomInfo.playersCards = new List<BaseCard>();
        //        for (int i = 1; i <= roomInfo.member.Count; i++)//人员
        //        {
        //            for (int j = 1; j <= cards; j++)//牌数
        //            {
        //                card = roomInfo.cardsLibrary[0];
        //                card.CardBelongTo = (CardBelongTo)i;//为每个玩家发牌 0是牌库
        //                Console.WriteLine("手牌：" + card.CardName);
        //                roomInfo.playersCards.Add(card);
        //                roomInfo.cardsLibrary.Remove(card);
        //            }
        //        }
        //        Console.WriteLine("手牌数：" + roomInfo.playersCards.Count + " 牌库：  " + roomInfo.cardsLibrary.Count);
        //    }
        //}
        #endregion

        /// <summary>
        /// 公布自选牌
        /// </summary>
        void OpenSelfSelectCard()
        {
            byte[] content = Decode.DecodeSecondContendBtyes(bytes);
            SingleDraw singleDraw = DataDo.Json2Object<SingleDraw>(content);
            RoomInfo roomInfo = GetRoom(singleDraw.roomNum);
            //找到自己在成员列表所对应的下标
            if (roomInfo == null) return;

            Next(roomInfo);
            Console.WriteLine("Next-------------OpenSelfSelectCard");


            //广播成员
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, (int)SecondCommands.SelectMyCardToOpen, content);
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 摸牌
        /// </summary>
        void Draw()
        {
            SingleDraw singleDraw = DataDo.Json2Object<SingleDraw>(Decode.DecodeSecondContendBtyes(bytes));
            RoomInfo roomInfo = GetRoom(singleDraw.roomNum);
            //找到自己在成员列表所对应的下标
            if (roomInfo == null) return;
            int cardIndex = singleDraw.cardIndex;
            BaseCard card = roomInfo.cardsLibrary[cardIndex];//////出错 “牌不一致”
            card.CardBelongTo = (CardBelongTo)singleDraw.myIndex;//为每个玩家发牌
            //服务器房间牌库数据修改
            roomInfo.playersCards.Add(card);
            roomInfo.cardsLibrary.Remove(card);
            Console.WriteLine("玩家牌数：" + roomInfo.playersCards.Count + "   牌库：" + roomInfo.cardsLibrary.Count);
            Console.WriteLine("singleDraw：   cardIndex:" + singleDraw.cardIndex + "   myId：" + singleDraw.myId + "   时间：" + DateTime.Now);

            //自动摸牌（托管）
            if (singleDraw.autoTrusteeship)
            {
                card.CardDisplay = CardDisplay.True;
                //每人的计时是_SingleDrawTime + _SelectMyselfCardTime+出牌按钮+再次摸牌时间
                Next(roomInfo);
                Console.WriteLine("Next-------------Draw");
            }
            ForeachSendDraw((int)SecondCommands.DRAW, roomInfo, cardIndex, card);
        }

        /// <summary>
        /// 遍历房间成员发送房间命令
        /// </summary>
        /// <param name="byt">内容</param>
        /// <param name="secondCommand">二级命令</param>
        public static void ForeachSendRoom(int secondCommand, RoomInfo roomInfo)
        {
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, secondCommand, DataDo.Object2Json(roomInfo));
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 遍历房间成员发送出牌者  命令
        /// </summary>
        /// <param name="byt">内容</param>
        /// <param name="secondCommand">二级命令</param>
        public static void ForeachSendForthgoer(int secondCommand, RoomInfo roomInfo, int[] diceIndex)
        {
            byte[] datas = new byte[8];
            byte[] dice_1 = BitConverter.GetBytes(diceIndex[0]);
            byte[] dice_2 = BitConverter.GetBytes(diceIndex[1]);
            Buffer.BlockCopy(dice_1, 0, datas, 0, 4);
            Buffer.BlockCopy(dice_2, 0, datas, 4, 4);
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, secondCommand, datas);
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 发单摸结果
        /// </summary>
        /// <param name="secondCommand"></param>
        /// <param name="roomInfo"></param>
        /// <param name="cardIndex">所摸牌在桌牌中的下标</param>
        /// <param name="card">所摸牌</param>
        void ForeachSendDraw(int secondCommand, RoomInfo roomInfo, int cardIndex, BaseCard card)
        {
            byte[] data_1 = BitConverter.GetBytes(cardIndex);
            byte[] data_3;
            if (card == null)
            {
                data_3 = new byte[0];
            }
            else
            {
                data_3 = DataDo.Object2Json(card);
            }
            byte[] datas = new byte[data_1.Length + data_3.Length];
            Buffer.BlockCopy(data_1, 0, datas, 0, 4);
            Buffer.BlockCopy(data_3, 0, datas, 4, data_3.Length);
            Conn conn;
            Console.WriteLine("----roomInfo.member.Count:" + roomInfo.member.Count);
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, secondCommand, datas);
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 创建一副牌
        /// </summary>
        public static List<BaseCard> CreatCardLibrary()
        {
            /// <summary>
            /// 牌库队列
            /// </summary>
            List<BaseCard> cardQueue = new List<BaseCard>();
            CardColors colors;
            CardWeight weight;
            int id = 0;
            for (int i = 0; i < 13; i++)//牌的权值数目
            {
                for (int j = 0; j < 2; j++)//牌色
                {

                    colors = (CardColors)j;
                    weight = (CardWeight)i;
                    BaseCard card = new BaseCard(colors.ToString() + weight.ToString(), id, colors, weight, CardBelongTo.ZERO, CardDisplay.False);//创建一张
                    cardQueue.Add(card);//入队
                    id++;
                }
            }
            //for (int i = 0; i < cardQueue.Count; i++)
            //{
            //    Console.WriteLine(cardQueue[i].CardId+ cardQueue[i].CardName);
            //}
            return cardQueue;
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        public static List<BaseCard> MixUpCards(List<BaseCard> cardQueue)
        {
            List<BaseCard> newList = new List<BaseCard>();
            foreach (BaseCard BaseCard in cardQueue)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                int index = random.Next(0, newList.Count + 1);//随机数要用种子机 使用Guid生成Seed；
                newList.Insert(index, BaseCard);
                //Console.WriteLine("index："+ index);
            }
            cardQueue.Clear();  //清空队列
            return newList;//多余了直接return newList
        }

        /// <summary>
        /// 猜牌结果发给其他人 并且 执行下一位操作
        /// </summary>
        void SendResult2Other()
        {
            byte[] content = Decode.DecodeSecondContendBtyes(bytes);
            GuessInfo guess = DataDo.Json2Object<GuessInfo>(content);

            RoomInfo room = GetRoom(guess.roomNum);
            if (room != null)
            {
                Console.WriteLine("--------SendResult2Other()");
                if (guess.isGuessTrue)
                {
                    ForeachSendGuess((int)SecondCommands.GUESS_T, room, content);
                    //修改计时器【累加】  延长计时（计时器-时间）
                    room.Time -= ((int)Time._ExtraTime + (int)Time._ClickThinkingTime);//如果玩家没选择按钮，托管发送会怎么样(延长时间还加上了_SelectMyselfCardTime,因为托管需要客户端驱动下一位，所以时长要放宽)
                }
                else
                {
                    ForeachSendGuess((int)SecondCommands.GUESS_F, room, content);
                    Next(room);
                }
            }
        }

        /// <summary>
        /// 下一位【改成房间的成员方法】
        /// </summary>
        /// <param name="roomInfo"></param>
        public static void Next(RoomInfo roomInfo)
        {
            roomInfo.Time = (int)Time._SingleDrawTime + (int)Time._SelectMyselfCardTime;
            //Console.WriteLine("执行下一位   " + DateTime.Now);
        }

        /// <summary>
        /// 遍历房间成员发送猜错命令
        /// </summary>
        void ForeachSendGuess(int secondCommand, RoomInfo roomInfo, byte[] guess)
        {
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, secondCommand, guess);
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 发送Id
        /// </summary>
        void SendID(int secondCommand)
        {
            byte[] content = Decode.DecodeSecondContendBtyes(bytes);
            string roomId = Encoding.UTF8.GetString(content, 4, content.Length - 4);
            RoomInfo roomInfo = GetRoom(roomId);
            if (roomInfo == null) return;
            if (secondCommand == (int)SecondCommands.Out)
            {
                Out(roomInfo, BitConverter.ToInt32(content, 0));
            }
            else
            {
                Next(roomInfo);
                ForeachSendOneOpen(secondCommand, roomInfo, bytes.Skip(12).Take(4).ToArray());
            }
        }

        /// <summary>
        /// 遍历房间成员发送公布玩家手牌  命令
        /// </summary>
        /// <param name="byt">内容</param>
        /// <param name="secondCommand">二级命令</param>
        static void ForeachSendOneOpen(int secondCommand, RoomInfo roomInfo, byte[] id)
        {
            if (id == null)
            {
                id = new byte[0];
            }
            Conn conn;
            for (int i = 0; i < roomInfo.member.Count; i++)
            {
                Server.connMap.TryGetValue(roomInfo.member[i].id.ToString(), out conn);
                byte[] data = Incode.IncodeSecondaryCommand(type, secondCommand, id);
                conn.SendBts(data);
            }
        }

        /// <summary>
        /// 结束
        /// </summary>
        public static void GameOver(RoomInfo roomInfo, List<int> exitList)
        {
            ForeachSendOneOpen((int)SecondCommands.Over, roomInfo, DataDo.Object2Json(exitList));
            Console.WriteLine("发送结束命令");
        }

        /// <summary>
        /// 继续【服务器房间+时间，玩家也要加】
        /// </summary>
        void Continue()
        {
            RoomInfo roomInfo = GetRoom(Encoding.UTF8.GetString(Decode.DecodeSecondContendBtyes(bytes)));
            if (roomInfo == null) return;
            //修改计时器【累加】  延长计时（计时器-时间）
            //roomInfo.Time -= (_ExtraTime+_ClickThinkingTime);
            //通知下去 只是发个信号
            ForeachSendOneOpen((int)SecondCommands.Continue, roomInfo, null);
        }

        /// <summary>
        /// 过
        /// </summary>
        void Pass()
        {
            RoomInfo roomInfo = GetRoom(Encoding.UTF8.GetString(Decode.DecodeSecondContendBtyes(bytes)));
            if (roomInfo == null) return;
            Next(roomInfo);
            Console.WriteLine("Next-------------Pass");

        }
    }
}
