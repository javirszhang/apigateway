   /***************************************************
 *
 * Data Access Layer Of Winner Framework
 * FileName : Tgw_Merchant.generate.cs
 * CreateTime : 2019-09-04 11:56:49
 * CodeGenerateVersion : 1.0.0.0
 * TemplateVersion: 2.0.0
 * E_Mail : zhj.pavel@gmail.com
 * Blog : 
 * Copyright (C) YXH
 * 
 ***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Winner.Framework.Core.DataAccess.Oracle;

namespace Winner.Gateway.DataResolver
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tgw_Merchant : DataAccessBase
	{
		#region 构造和基本
		public Tgw_Merchant():base()
		{}
		public Tgw_Merchant(DataRow dataRow):base(dataRow)
		{}
		public const string _ID = "ID";
		public const string _MERCHANT_NAME = "MERCHANT_NAME";
		public const string _MERCHANT_CODE = "MERCHANT_CODE";
		public const string _SIGN_TYPE = "SIGN_TYPE";
		public const string _SECRET_KEY = "SECRET_KEY";
		public const string _GATEWAY_KEY = "GATEWAY_KEY";
		public const string _CREATE_TIME = "CREATE_TIME";
		public const string _STATUS = "STATUS";
		public const string _REMARKS = "REMARKS";
		public const string _TableName = "TGW_MERCHANT";
		protected override DataRow BuildRow()
		{
			DataTable table = new DataTable("TGW_MERCHANT");
			table.Columns.Add(_ID,typeof(int)).DefaultValue=0;
			table.Columns.Add(_MERCHANT_NAME,typeof(string)).DefaultValue=string.Empty;
			table.Columns.Add(_MERCHANT_CODE,typeof(string)).DefaultValue=string.Empty;
			table.Columns.Add(_SIGN_TYPE,typeof(string)).DefaultValue="MD5";
			table.Columns.Add(_SECRET_KEY,typeof(string)).DefaultValue=string.Empty;
			table.Columns.Add(_GATEWAY_KEY,typeof(string)).DefaultValue=string.Empty;
			table.Columns.Add(_CREATE_TIME,typeof(DateTime)).DefaultValue=DateTime.Now;
			table.Columns.Add(_STATUS,typeof(int)).DefaultValue=1;
			table.Columns.Add(_REMARKS,typeof(string)).DefaultValue=DBNull.Value;
			return table.NewRow();
		}
		#endregion
		
		#region 属性
		protected override string TableName
		{
			get{return _TableName;}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: 0;   Length: 10Byte
		/// </para>
		/// </summary>
		public int Id
		{
			get{ return  Convert.ToInt32(DataRow[_ID]);}
			 set{setProperty(_ID, value);}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: string.Empty;   Length: 30Byte
		/// </para>
		/// </summary>
		public string Merchant_Name
		{
			get{ return DataRow[_MERCHANT_NAME].ToString();}
			 set{setProperty(_MERCHANT_NAME, value);}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: string.Empty;   Length: 50Byte
		/// </para>
		/// </summary>
		public string Merchant_Code
		{
			get{ return DataRow[_MERCHANT_CODE].ToString();}
			 set{setProperty(_MERCHANT_CODE, value);}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: "MD5";   Length: 10Byte
		/// </para>
		/// </summary>
		public string Sign_Type
		{
			get{ return DataRow[_SIGN_TYPE].ToString();}
			 set{setProperty(_SIGN_TYPE, value);}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: string.Empty;   Length: 4000Byte
		/// </para>
		/// </summary>
		public string Secret_Key
		{
			get{ return DataRow[_SECRET_KEY].ToString();}
			 set{setProperty(_SECRET_KEY, value);}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: string.Empty;   Length: 4000Byte
		/// </para>
		/// </summary>
		public string Gateway_Key
		{
			get{ return DataRow[_GATEWAY_KEY].ToString();}
			 set{setProperty(_GATEWAY_KEY, value);}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: DateTime.Now;   Length: 7Byte
		/// </para>
		/// </summary>
		public DateTime Create_Time
		{
			get{ return  Convert.ToDateTime(DataRow[_CREATE_TIME]);}
			 set{setProperty(_CREATE_TIME, value);}
		}
		/// <summary>
		/// (必填)
		/// <para>
		/// defaultValue: 1;   Length: 10Byte
		/// </para>
		/// </summary>
		public int Status
		{
			get{ return  Convert.ToInt32(DataRow[_STATUS]);}
			 set{setProperty(_STATUS, value);}
		}
		/// <summary>
		/// (可空)
		/// <para>
		/// defaultValue: DBNull.Value;   Length: 200Byte
		/// </para>
		/// </summary>
		public string Remarks
		{
			get{ return DataRow[_REMARKS].ToString();}
			 set{setProperty(_REMARKS, value);}
		}
		#endregion
		
		#region 基本方法
		protected bool SelectByCondition(string condition)
		{
			string sql = "SELECT ID,MERCHANT_NAME,MERCHANT_CODE,SIGN_TYPE,SECRET_KEY,GATEWAY_KEY,CREATE_TIME,STATUS,REMARKS FROM TGW_MERCHANT WHERE "+condition;
			return base.SelectBySql(sql);
		}
		protected bool DeleteByCondition(string condition)
		{
			string sql = "DELETE FROM TGW_MERCHANT WHERE "+condition;
			return base.DeleteBySql(sql);
		}
		
		public bool Delete(int id)
		{
			string condition = " ID=:ID";
			AddParameter(_ID,id);
			return DeleteByCondition(condition);
		}
		public bool Delete()
		{
			string condition = " ID=:ID";
			AddParameter(_ID,DataRow[_ID]);
			return DeleteByCondition(condition);
		}
				
		public bool Insert()
		{		
			int id = this.Id = GetSequence("SELECT SEQ_TGW_MERCHANT.nextval FROM DUAL");
			string sql = @"INSERT INTO TGW_MERCHANT(ID,MERCHANT_NAME,MERCHANT_CODE,SIGN_TYPE,SECRET_KEY,GATEWAY_KEY,STATUS,REMARKS)
			VALUES (:ID,:MERCHANT_NAME,:MERCHANT_CODE,:SIGN_TYPE,:SECRET_KEY,:GATEWAY_KEY,:STATUS,:REMARKS)";
			AddParameter(_ID,DataRow[_ID]);
			AddParameter(_MERCHANT_NAME,DataRow[_MERCHANT_NAME]);
			AddParameter(_MERCHANT_CODE,DataRow[_MERCHANT_CODE]);
			AddParameter(_SIGN_TYPE,DataRow[_SIGN_TYPE]);
			AddParameter(_SECRET_KEY,DataRow[_SECRET_KEY]);
			AddParameter(_GATEWAY_KEY,DataRow[_GATEWAY_KEY]);
			AddParameter(_STATUS,DataRow[_STATUS]);
			AddParameter(_REMARKS,DataRow[_REMARKS]);
			return InsertBySql(sql);
		}
		
		public bool Update()
		{
			return UpdateByCondition(string.Empty);
		}
		public bool Update(Dictionary<Tgw_MerchantCollection.Field,object> conditionDic)
		{
            if (conditionDic.Count <= 0)
                return false;
			ChangePropertys.Remove(_ID);
			if (ChangePropertys.Count == 0)
            {
                return true;
            }
            
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE TGW_MERCHANT SET");
			while (ChangePropertys.MoveNext())
            {
         		sql.AppendFormat(" {0}{1}=:TO_{1} ", (ChangePropertys.CurrentIndex == 0 ? string.Empty : ","), ChangePropertys.Current);
                AddParameter("TO_"+ChangePropertys.Current, DataRow[ChangePropertys.Current]);
            }
			sql.Append(" WHERE ID=:ID ");
			AddParameter(_ID, DataRow[_ID]);			
            foreach (Tgw_MerchantCollection.Field key in conditionDic.Keys)
            {
				if(Tgw_MerchantCollection.Field.Id == key) continue;

                object value = conditionDic[key];
                string name = string.Concat("condition_", key);
                sql.Append(" AND ").Append(key.ToString().ToUpper()).Append("=:").Append(name.ToUpper());
                AddParameter(name, value);
            }            
            return UpdateBySql(sql.ToString());
		}
		protected bool UpdateByCondition(string condition)
		{
			ChangePropertys.Remove(_ID);
			if (ChangePropertys.Count == 0)
            {
                return true;
            }
            
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE TGW_MERCHANT SET");
			while (ChangePropertys.MoveNext())
            {
         		sql.AppendFormat(" {0}{1}=:{1} ", (ChangePropertys.CurrentIndex == 0 ? string.Empty : ","), ChangePropertys.Current);
                AddParameter(ChangePropertys.Current, DataRow[ChangePropertys.Current]);
            }
			sql.Append(" WHERE ID=:ID");
			AddParameter(_ID, DataRow[_ID]);			
			if (!string.IsNullOrEmpty(condition))
            {
				sql.AppendLine(" AND " + condition);
			}
			bool result = base.UpdateBySql(sql.ToString());
            ChangePropertys.Clear();
            return result;
		}	
		public bool SelectByMerchantCode(string merchant_code)
		{
			string condition = null;
			condition += "MERCHANT_CODE=:MERCHANT_CODE";
			AddParameter(_MERCHANT_CODE,merchant_code);

			return SelectByCondition(condition);
		}
		public bool SelectByPK(int id)
		{
			string condition = " ID=:ID";
			AddParameter(_ID,id);
			return SelectByCondition(condition);
		}
		#endregion
	}
	/// <summary>
	/// [集合对象]
	/// </summary>
	public partial class Tgw_MerchantCollection : DataAccessCollectionBase
	{
		#region 构造和基本
		public Tgw_MerchantCollection():base()
		{			
		}
		
		protected override DataTable BuildTable()
		{
			return new Tgw_Merchant().CloneSchemaOfTable();
		}
		protected override DataAccessBase GetItemByIndex(int index)
        {
            return new Tgw_Merchant(DataTable.Rows[index]);
        }
		protected override string TableName
		{
			get{return Tgw_Merchant._TableName;}
		}
		public Tgw_Merchant this[int index]
        {
            get { return new Tgw_Merchant(DataTable.Rows[index]); }
        }
		public enum Field
        {
			Id=0,
			Merchant_Name=1,
			Merchant_Code=2,
			Sign_Type=3,
			Secret_Key=4,
			Gateway_Key=5,
			Create_Time=6,
			Status=7,
			Remarks=8,
		}
		#endregion
		#region 基本方法
		protected bool ListByCondition(string condition)
		{
			string sql = "SELECT ID,MERCHANT_NAME,MERCHANT_CODE,SIGN_TYPE,SECRET_KEY,GATEWAY_KEY,CREATE_TIME,STATUS,REMARKS FROM TGW_MERCHANT WHERE "+condition;
			return ListBySql(sql);
		}

		public bool ListAll()
		{
			string condition = " 1=1 ORDER BY ID DESC";
			return ListByCondition(condition);
		}
		#region Linq
		public Tgw_Merchant Find(Predicate<Tgw_Merchant> match)
        {
            foreach (Tgw_Merchant item in this)
            {
                if (match(item))
                    return item;
            }
            return null;
        }
        public Tgw_MerchantCollection FindAll(Predicate<Tgw_Merchant> match)
        {
            Tgw_MerchantCollection list = new Tgw_MerchantCollection();
            foreach (Tgw_Merchant item in this)
            {
                if (match(item))
                    list.Add(item);
            }
            return list;
        }
        public bool Contains(Predicate<Tgw_Merchant> match)
        {
            foreach (Tgw_Merchant item in this)
            {
                if (match(item))
                    return true;
            }
            return false;
        }
		public bool DeleteAt(Predicate<Tgw_Merchant> match)
        {
            BeginTransaction();
            foreach (Tgw_Merchant item in this)
            {
                item.ReferenceTransactionFrom(Transaction);
                if (!match(item))
                    continue;
                if (!item.Delete())
                {
                    Rollback();
                    return false;
                }
            }
            Commit();
            return true;
        }
		#endregion
		#endregion		
	}
}