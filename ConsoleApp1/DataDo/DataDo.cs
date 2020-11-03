using System;
using Newtonsoft.Json;
using System.Data.OleDb;
using System.Data;

namespace ConsoleApp1
{
   public class DataDo
    {
        /// <summary>
        /// 当你已有重载构造函数时，要把默认构造函数写出来，因为调用的是默认构造函数（所有字段属性要是共有的）
        /// 可以解析数组、列表、字典、类， 不能解析队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataBts"></param>
        /// <returns></returns>
        public static T Json2Object<T>(byte[] dataBts) {
            try
            {
                string jsonStr = System.Text.Encoding.UTF8.GetString(dataBts);
                T jsonObj = JavaScriptConvert.DeserializeObject<T>(jsonStr);
                return jsonObj;
            }
            catch (Exception w)
            {
                Console.Write("DataDo__Json2Object__:消息包不完整");
                throw w;
            }
        }

        /// <summary>
        /// （所有字段属性要是共有的）
        ///  可以解析数组、列表、字典、类， 不能解析队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Object2Json<T>(T obj) {
           string jsonStr = JavaScriptConvert.SerializeObject(obj);
            return System.Text.Encoding.UTF8.GetBytes(jsonStr);
        }

        #region MyRegion
        /// <summary>
        /// 读取本地Excel表
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        //public static DataSet ExcelToDS(string Path)
        //{
        //    string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
        //    OleDbConnection conn = new OleDbConnection(strConn);
        //    conn.Open();
        //    string strExcel = "";
        //    OleDbDataAdapter myCommand = null;
        //    DataSet ds = null;
        //    strExcel = "select * from [sheet1$]";
        //    myCommand = new OleDbDataAdapter(strExcel, strConn);
        //    ds = new DataSet();
        //    myCommand.Fill(ds, "table1");
        //    return ds;
        //}
        #endregion
    }
}
