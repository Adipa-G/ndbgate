using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace dbgate.utility
{
    public class StatusManager
    {
        private const string fmt = "%24s: %s%n";

        public static void SetStatus(IDbClass dbClass, DbClassStatus status)
        {
            if (dbClass == null)
            {
                return;
            }

            dbClass.Status = status;

            Type objectType = dbClass.GetType();
            PropertyInfo[] properties = objectType.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                try
                {
                    Object value = propertyInfo.GetValue(dbClass, null);
                    if (value != null)
                    {
                        if (value is ICollection)
                        {
                            var enumerable = (ICollection) value;
                            foreach (Object o in enumerable)
                            {
                                if (o is IDbClass)
                                {
                                    SetStatus((IDbClass) o, status);
                                }
                            }
                        }
                        else if (value is IDbClass)
                        {
                            SetStatus((IDbClass) value, status);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof (StatusManager)).Fatal(
                        "Exception occured while trying to update status", e);
                }
            }
        }

        public static bool IsModified(Object obO)
        {
            bool modified = false;
            if (obO == null)
            {
                return false;
            }

            if (obO is ICollection)
            {
                var enumerable = (ICollection) obO;
                foreach (Object o in enumerable)
                {
                    modified = IsModified(o);
                    if (modified)
                    {
                        return true;
                    }
                }
            }
            else if (!(obO is IDbClass))
            {
                return false;
            }

            if ((obO is IDbClass))
            {
                var dbClass = (IDbClass) obO;

                modified = dbClass.Status == DbClassStatus.Deleted
                           || dbClass.Status == DbClassStatus.New
                           || dbClass.Status == DbClassStatus.Modified;
                if (modified)
                {
                    return true;
                }

                Type objectType = dbClass.GetType();
                PropertyInfo[] properties = objectType.GetProperties();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    try
                    {
                        Object value = propertyInfo.GetValue(dbClass, null);
                        if (value != null)
                        {
                            if (value is ICollection)
                            {
                                var enumerable = (ICollection) value;
                                foreach (Object o in enumerable)
                                {
                                    modified = IsModified(o);
                                    if (modified)
                                    {
                                        return true;
                                    }
                                }
                            }
                            else if (value is IDbClass)
                            {
                                modified = IsModified(value);
                                if (modified)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger(typeof (StatusManager)).Fatal(
                            "Exception occured while trying to check if modified", e);
                    }
                }
            }
            return modified;
        }

        public static ICollection<IDbClass> GetImmidiateChildrenAndClear(IDbClass dbClass)
        {
            var childList = new List<IDbClass>();

            Type objectType = dbClass.GetType();
            PropertyInfo[] properties = objectType.GetProperties();

            foreach (PropertyInfo propertyInfo in properties)
            {
                try
                {
                    Object value = propertyInfo.GetValue(dbClass, null);
                    if (value != null)
                    {
                        if (value is IList)
                        {
                            var enumerable = (IList) value;
                            foreach (Object o in enumerable)
                            {
                                if (o is IDbClass)
                                {
                                    childList.Add((IDbClass) o);
                                }
                            }
                            enumerable.Clear();
                        }
                        else if (value is IDbClass)
                        {
                            childList.Add((IDbClass) value);
                            propertyInfo.SetValue(dbClass, null, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof (StatusManager)).Fatal(
                        "Exception occured while trying to retrieve child objects", e);
                }
            }

            return childList;
        }
    }
}