using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace DbGate.Utility
{
    public class StatusManager
    {
        private const string Fmt = "%24s: %s%n";

        public static void SetStatus(IClientEntity clientEntity, EntityStatus status)
        {
            if (clientEntity == null)
            {
                return;
            }

            clientEntity.Status = status;

            var objectType = clientEntity.GetType();
            var properties = objectType.GetProperties();
            foreach (var propertyInfo in properties)
            {
                try
                {
                    var value = propertyInfo.GetValue(clientEntity, null);
                    if (value != null)
                    {
                        if (value is ICollection)
                        {
                            var enumerable = (ICollection) value;
                            foreach (var o in enumerable)
                            {
                                if (o is IClientEntity)
                                {
                                    SetStatus((IClientEntity) o, status);
                                }
                            }
                        }
                        else if (value is IClientEntity)
                        {
                            SetStatus((IClientEntity) value, status);
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
            var modified = false;
            if (obO == null)
            {
                return false;
            }

            if (obO is ICollection)
            {
                var enumerable = (ICollection) obO;
                foreach (var o in enumerable)
                {
                    modified = IsModified(o);
                    if (modified)
                    {
                        return true;
                    }
                }
            }
            else if (!(obO is IClientEntity))
            {
                return false;
            }

            if ((obO is IClientEntity))
            {
                var dbClass = (IClientEntity) obO;

                modified = dbClass.Status == EntityStatus.Deleted
                           || dbClass.Status == EntityStatus.New
                           || dbClass.Status == EntityStatus.Modified;
                if (modified)
                {
                    return true;
                }

                var objectType = dbClass.GetType();
                var properties = objectType.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    try
                    {
                        var value = propertyInfo.GetValue(dbClass, null);
                        if (value != null)
                        {
                            if (value is ICollection)
                            {
                                var enumerable = (ICollection) value;
                                foreach (var o in enumerable)
                                {
                                    modified = IsModified(o);
                                    if (modified)
                                    {
                                        return true;
                                    }
                                }
                            }
                            else if (value is IClientEntity)
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

        public static ICollection<IClientEntity> GetImmidiateChildrenAndClear(IClientEntity clientEntity)
        {
            var childList = new List<IClientEntity>();

            var objectType = clientEntity.GetType();
            var properties = objectType.GetProperties();

            foreach (var propertyInfo in properties)
            {
                try
                {
                    var value = propertyInfo.GetValue(clientEntity, null);
                    if (value != null)
                    {
                        if (value is IList)
                        {
                            var enumerable = (IList) value;
                            foreach (var o in enumerable)
                            {
                                if (o is IClientEntity)
                                {
                                    childList.Add((IClientEntity) o);
                                }
                            }
                            enumerable.Clear();
                        }
                        else if (value is IClientEntity)
                        {
                            childList.Add((IClientEntity) value);
                            propertyInfo.SetValue(clientEntity, null, null);
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