using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data;

namespace ConsoleApp1
{
    class SqlConn
    {
        /// <summary>
        /// 数据库的每次操作都要重新建立连接和断开连接：为了节省资源 防止数据库连接发生死锁；
        /// 每次打开连接就会建立一条到服务器数据库的通道，每台服务器的总通道数量是有限的，大概就2000左右，而且内存占用也会比较大；
        /// 如果不写关闭代码，一个终端退出后数据库通道还是占用的，那么很多个人连接之后就把通道占满了，这样其他用户再连就连不上了。
        /// close()关闭链接，不释放数据库连接池的资源，而是把连接放回连接池中待用。
        ///dispose()则会把这个连接彻底销毁掉，不会再放入连接池。如果所有的连接都被Dispose的话，每次使用数据库都必须重新创建连接，
        ///这样很耗费资源。因此不要用Dispose。
        ///.NET会维护连接池，连接Open的时候从连接池中取出一个没有使用的连接，用完以后Close()的时候再放回连接池，实际上没有创建新
        ///的连接，从而提高了性能。 因此为了性能，为了使用连接池，不可以dispose，但必须close() 。
        ///Close()是关闭，Dispose()是释放对象（回收）
        ///比如Conn对象，Close过后，Conn这个对象本身还存在内存中，需要在使用的时候，可以直接使用。而调用Dispose()后，
        ///Conn对象被回收，Conn对象已经不存在了，下次再需要使用的时候，对象就不存在了，需要重新创建（New）
        ///close()改变连接状态为关闭，可以正常打开（用open（））；
        ///dispose()销毁连接；
        /// </summary>

        static MySqlConnection sqlConn;

        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <returns></returns>
        static public MySqlConnection ConnectDatabase()
        {
            string connStr = string.Format("server={0};user id={1};password={2};port={3};" +
                "database=game_count;pooling=false;charset=utf8",
           "127.0.0.1", "root", "", 3306);
            try
            {
                sqlConn = new MySqlConnection(connStr);
                sqlConn.Open();
                Console.WriteLine("[数据库连接成功！]");
                return sqlConn;
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库连接失败]" + e.Message);
                return null;
            }
        }

        static public string Insert(string name, string psd)
        {
            if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
            string contentStr = "INSERT INTO counter(PassWord, name,status) VALUES(\'" + psd + "\', \'" + name + "\', 1 );";
            try
            {
                MySqlCommand insert = new MySqlCommand(contentStr, sqlConn);
                insert.ExecuteNonQuery();
                Console.WriteLine("插入成功");
                return insert.LastInsertedId.ToString();//通过访问数据库获取自动分配的id
            }
            catch (Exception e)
            {
                Console.WriteLine("[插入失败]" + e.Message);
                return null;
            }
            finally
            { sqlConn.Close(); }
        }

        //static public bool Register(string name)//注册账号查询
        //{
        //    if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
        //    string contentStr = "SELECT * FROM counter where name = \"" + name + "\";";
        //    MySqlCommand select = new MySqlCommand(contentStr, sqlConn);
        //    try
        //    {
        //        MySqlDataReader dataReader = select.ExecuteReader();
        //        while (dataReader.Read())
        //        {
        //            //Console.WriteLine("ID=" + dataReader[0].ToString() + " ,PassWord=" + dataReader[1].ToString() + " ,name=" + dataReader[2].ToString());
        //            Console.WriteLine("昵称已被注册");
        //        }
                
        //        dataReader.Close();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("[查询失败]" + e.Message);
        //        return false;
        //    }
        //    finally
        //    { sqlConn.Close(); }
        //}

        static public PersonalInfo Select(string contentStr)//登录查询
        {
            PersonalInfo personalInfo = new PersonalInfo();
            if (sqlConn.State!= ConnectionState.Open) sqlConn.Open();
            MySqlCommand select = new MySqlCommand(contentStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = select.ExecuteReader();
                while (dataReader.Read())
                {
                    personalInfo = new PersonalInfo
                    {
                        id = (int)dataReader["Id"],//password
                        password = (string)dataReader["PassWord"],
                        name = (string)dataReader["name"],
                        icon = (string)dataReader["icon"],
                        sex = (System.SByte)(dataReader["sex"]),
                        age = (System.SByte)dataReader["age"],
                        status = (System.SByte)dataReader["status"],
                        degree = (System.SByte)dataReader["degree"],
                        winRate = (float)dataReader["winRate"],
                        serialWin = (int)dataReader["serialWin"],
                        gameNum = (int)dataReader["gameNum"],
                        roomNum = (string)dataReader["roomNum"]
                    };
                }
                dataReader.Close();
                return personalInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine("[查询失败]" + e.Message);
                return null;
            }
            finally
            { sqlConn.Close(); }

        }

        static public void AlterInfo(string tableName, string column_conditions, string row_id)//修改数据库
        {
            if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
            try
            {
                string contentStr = "UPDATE " + tableName + " SET " + column_conditions + " WHERE " + row_id;
                //Console.WriteLine(contentStr);
                MySqlCommand alterInfo = new MySqlCommand(contentStr, sqlConn);
                alterInfo.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("[修改失败]" + e.Message);
            }
            finally
            { sqlConn.Close(); }

        }

        static public List<PersonalInfo> FindFriends(string contentStr)//获取在线好友
        {
            PersonalInfo personalInfo = new PersonalInfo();
            if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
            List<PersonalInfo> FriendTable = new List<PersonalInfo>();
            try
            {
                MySqlCommand select = new MySqlCommand(contentStr, sqlConn);
                MySqlDataReader dataReader = select.ExecuteReader();
                while (dataReader.Read())
                {
                    personalInfo = new PersonalInfo
                    {
                        id = (int)dataReader["Id"],
                        name = (string)dataReader["name"],
                        markName = dataReader["markName"] != DBNull.Value ? (string)dataReader["markName"] : (string)dataReader["name"],
                        icon = (string)dataReader["icon"],
                        sex = (System.SByte)(dataReader["sex"]),
                        age = (System.SByte)dataReader["age"],
                        status = (System.SByte)dataReader["status"],
                        degree = (System.SByte)dataReader["degree"],
                        winRate = (float)dataReader["winRate"],
                        serialWin = (int)dataReader["serialWin"],
                        gameNum = (int)dataReader["gameNum"],
                        roomNum = (string)dataReader["roomNum"],
                        coin = (int)dataReader["coin"]
                    };
                    FriendTable.Add(personalInfo);
                }
                dataReader.Close();
                return FriendTable;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            { sqlConn.Close(); }
        }

        static public List<PersonalInfo> GetRank()//获取排行榜
        {
            string contentStr = "SELECT * FROM counter order by mileage desc limit 10;";
            return FindFriends(contentStr);
        }

        /// <summary>
        /// 初始化玩家登录状态和房间信息 
        /// </summary>
        static public void InitializePersenInfo()
        {
            if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
            try
            {
                string contentStr = "UPDATE counter SET roomNum = '0',status='"+(int)PersonStatus.OffLine +"'";
                MySqlCommand alterInfo = new MySqlCommand(contentStr, sqlConn);
                alterInfo.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("[修改失败]" + e.Message);
            }
            finally
            { sqlConn.Close(); }
        }
    }
}
