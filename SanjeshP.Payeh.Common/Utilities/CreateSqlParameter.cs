//using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SanjeshP.Payeh.Common.Utilities
{
    public class CreateSqlParameter
    {
        public SqlParameter Create_Param<T>(
            string paramName,
            T paramValue,
            SqlDbType sqlDbType,
            int? size = null,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = new SqlParameter
            {
                ParameterName = paramName,
                Value = paramValue is null ? DBNull.Value : paramValue,
                SqlDbType = sqlDbType,
                Direction = direction,
                IsNullable = paramValue is null
            };

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            return parameter;
        }

        public SqlParameter Create_TableParam(
            string paramName,
            DataTable paramValue,
            string typeName,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = new SqlParameter
            {
                ParameterName = paramName,
                SqlDbType = SqlDbType.Structured,
                TypeName = typeName,
                Value = paramValue ?? throw new ArgumentNullException(nameof(paramValue)),
                Direction = direction
            };

            return parameter;
        }
    }
}
