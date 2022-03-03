using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace BT0301Batch
{
    public class DatabaseHelper
    {
        private readonly ISqlMapper sqlMapper;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DatabaseHelper()
        {
            DomSqlMapBuilder builder = new DomSqlMapBuilder();
            this.sqlMapper = builder.Configure($"{ConfigurationManager.AppSettings["iBatisConfigPath"]}");

        }

        /// <summary>
        /// SELECT文を実行しリストを返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statementName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IList<T> QueryForList<T>(string statementName, object parameters = null)
        {
            this.DebugLog(statementName, parameters);
            return this.sqlMapper.QueryForList<T>(statementName, parameters);
        }

        /// <summary>
        /// SELECT文を実行し単一オブジェクトを返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statementName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T QueryForObject<T>(string statementName, object parameters = null)
        {
            this.DebugLog(statementName, parameters);
            return this.sqlMapper.QueryForObject<T>(statementName, parameters);
        }

        /// <summary>
        /// INSERT文を実行する
        /// </summary>
        /// <param name="statementName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Object Insert(string statementName, object parameters = null)
        {
            this.DebugLog(statementName, parameters);
            return this.sqlMapper.Insert(statementName, parameters);
        }

        /// <summary>
        /// UPDATE文を実行する
        /// </summary>
        /// <param name="statementName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Update(string statementName, object parameters = null)
        {
            this.DebugLog(statementName, parameters);
            return this.sqlMapper.Update(statementName, parameters);
        }

        /// <summary>
        /// DELETE文を実行する
        /// </summary>
        /// <param name="statementName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Delete(string statementName, object parameters = null)
        {
            this.DebugLog(statementName, parameters);
            return this.sqlMapper.Delete(statementName, parameters);
        }

        /// <summary>
        /// データベースに接続する
        /// </summary>
        public void Open()
        {
            CLogger.Logger("DB Open");
            if (!this.sqlMapper.IsSessionStarted)
            {
                this.sqlMapper.OpenConnection();
            }
        }

        /// <summary>
        /// データベースの接続を閉じる
        /// </summary>
        public void Close()
        {
            CLogger.Logger("DB Close");
            if (this.sqlMapper.IsSessionStarted)
            {
                this.sqlMapper.CloseConnection();
            }
        }

        /// <summary>
        /// トランザクションを開始する
        /// </summary>
        public void Begin()
        {
            CLogger.Logger("Begin Transaction");
            this.sqlMapper.BeginTransaction(false);
        }

        /// <summary>
        /// トランザクションをコミットする
        /// </summary>
        public void Commit()
        {
            CLogger.Logger("Commit Transaction");
            this.sqlMapper.CommitTransaction(false);
        }

        /// <summary>
        /// トランザクションをロールバックする
        /// </summary>
        public void Rollback()
        {
            CLogger.Logger("Rollback Transaction");
            this.sqlMapper.RollBackTransaction(false);
        }

        /// <summary>
        /// SQL文のデバッグログ出力
        /// </summary>
        /// <param name="statementName"></param>
        /// <param name="parameters"></param>
        private void DebugLog(string statementName, object parameters = null)
        {
            var localSession = new SqlMapSession(this.sqlMapper);
            var mappedStatement = this.sqlMapper.GetMappedStatement(statementName);
            var scope = mappedStatement.Statement.Sql.GetRequestScope(mappedStatement, parameters, localSession);
            mappedStatement.PreparedCommand.Create(scope, localSession, mappedStatement.Statement, parameters);
            var sql = scope.PreparedStatement.PreparedSql;
            CLogger.Debug(sql);
            if(parameters != null && parameters.GetType() == typeof(Hashtable))
            {
                Hashtable hashParams = (Hashtable)parameters;
                var messageBuilder = new StringBuilder(); 
                foreach (DictionaryEntry entry in hashParams)
                {
                    if(messageBuilder.Length == 0)
                    {
                        messageBuilder.Append("params: ");
                    }
                    else
                    {
                        messageBuilder.Append(", ");
                    }
                    messageBuilder.Append($"{entry.Key}={entry.Value}");
                }
                CLogger.Debug(messageBuilder.ToString());
            }
        }
    }
}