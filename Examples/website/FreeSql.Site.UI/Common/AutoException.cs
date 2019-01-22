using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace FreeSql.Site.UI.Common
{
    public class AutoException
    {
        /// <summary>
        /// 执行方法外壳，包括异常抓取，固定格式返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="isTransaction"></param>
        /// <returns></returns>
        public static ServiceResult<T> Excute<T>(
            Action<ServiceResult<T>> action, bool isTransaction = false)
        {
            TransactionScope ts = null;
            if (isTransaction) ts = new TransactionScope();
            ServiceResult<T> result = new ServiceResult<T>() { Status = EnumServiceResult.Success.GetHashCode() };
            try
            {
                action.Invoke(result);
                if (isTransaction) ts.Complete();
            }
            catch (Exception ex)
            {
                result.Msg = ex.Message;
                result.Status = EnumServiceResult.Failure.GetHashCode();
            }
            finally
            {
                if (isTransaction) ts.Dispose();
            }
            return result;
        }

        /// <summary>
        /// 规范接口调用方法
        /// </summary>
        /// <typeparam name="T">返回值参数 </typeparam>
        /// <param name="action">执行方法内容</param>
        /// <param name="isTransaction">是否启用事务</param>
        /// <returns></returns>
        public static ServiceResult Execute(Action<ServiceResult> action, bool isTransaction = false)
        {
            TransactionScope ts = null;
            if (isTransaction) ts = new TransactionScope();
            ServiceResult result = new ServiceResult() { Status = EnumServiceResult.Success.GetHashCode(), Msg = "保存成功" };
            try
            {
                action.Invoke(result);
                if (result.Status == EnumServiceResult.Success.GetHashCode())
                {
                    if (isTransaction) ts.Complete();
                }
            }
            catch (Exception ex)
            {
                result.Msg = ex.Message;
                result.Status = EnumServiceResult.Failure.GetHashCode();
            }
            finally
            {
                if (isTransaction) ts.Dispose();
            }
            return result;
        }
    }
}
