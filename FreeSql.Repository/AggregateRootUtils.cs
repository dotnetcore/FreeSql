using FreeSql;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

static class AggregateRootUtils
{
    public static void CompareEntityValue(IFreeSql fsql, Type rootEntityType, object rootEntityBefore, object rootEntityAfter, string rootNavigatePropertyName,
        List<NativeTuple<Type, object>> insertLog,
        List<NativeTuple<Type, object, object, List<string>>> updateLog,
        List<NativeTuple<Type, object[]>> deleteLog)
    {
        Dictionary<Type, Dictionary<string, bool>> ignores = new Dictionary<Type, Dictionary<string, bool>>();
        LocalCompareEntityValue(rootEntityType, rootEntityBefore, rootEntityAfter, rootNavigatePropertyName);
        ignores.Clear();

        void LocalCompareEntityValue(Type entityType, object entityBefore, object entityAfter, string navigatePropertyName)
        {
            if (entityType == null) entityType = entityBefore?.GetType() ?? entityAfter?.GetType();

            if (entityBefore != null)
            {
                var stateKey = $":before://{fsql.GetEntityKeyString(entityType, entityBefore, false)}";
                if (ignores.TryGetValue(entityType, out var stateKeys) == false) ignores.Add(entityType, stateKeys = new Dictionary<string, bool>());
                if (stateKeys.ContainsKey(stateKey)) return;
                stateKeys.Add(stateKey, true);
            }
            if (entityAfter != null)
            {
                var stateKey = $":after://{fsql.GetEntityKeyString(entityType, entityAfter, false)}";
                if (ignores.TryGetValue(entityType, out var stateKeys) == false) ignores.Add(entityType, stateKeys = new Dictionary<string, bool>());
                if (stateKeys.ContainsKey(stateKey)) return;
                stateKeys.Add(stateKey, true);
            }

            var table = fsql.CodeFirst.GetTableByEntity(entityType);
            if (table == null) return;
            if (entityBefore == null && entityAfter == null) return;
            if (entityBefore == null && entityAfter != null)
            {
                insertLog.Add(NativeTuple.Create(entityType, entityAfter));
                return;
            }
            if (entityBefore != null && entityAfter == null)
            {
                deleteLog.Add(NativeTuple.Create(entityType, new[] { entityBefore }));
                NavigateReader(fsql, entityType, entityBefore, (path, tr, ct, stackvs) =>
                {
                    var dellist = stackvs.First() as object[] ?? new[] { stackvs.First() };
                    deleteLog.Add(NativeTuple.Create(ct, dellist));
                });
                return;
            }
            var changes = new List<string>();
            foreach (var col in table.ColumnsByCs.Values)
            {
                if (table.ColumnsByCsIgnore.ContainsKey(col.CsName)) continue;
                if (table.ColumnsByCs.ContainsKey(col.CsName))
                {
                    if (col.Attribute.IsVersion) continue;
                    var propvalBefore = table.GetPropertyValue(entityBefore, col.CsName);
                    var propvalAfter = table.GetPropertyValue(entityAfter, col.CsName);
                    if (object.Equals(propvalBefore, propvalAfter) == false) changes.Add(col.CsName);
                    continue;
                }
            }
            if (changes.Any())
                updateLog.Add(NativeTuple.Create(entityType, entityBefore, entityAfter, changes));

            foreach (var prop in table.Properties.Values)
            {
                var tbref = table.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
                if (navigatePropertyName != null && prop.Name != navigatePropertyName) continue;
                var propvalBefore = table.GetPropertyValue(entityBefore, prop.Name);
                var propvalAfter = table.GetPropertyValue(entityAfter, prop.Name);
                switch (tbref.RefType)
                {
                    case TableRefType.OneToOne:
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entityBefore, propvalBefore);
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entityAfter, propvalAfter);
                        LocalCompareEntityValue(tbref.RefEntityType, propvalBefore, propvalAfter, null);
                        break;
                    case TableRefType.OneToMany:
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entityBefore, propvalBefore);
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entityAfter, propvalAfter);
                        LocalCompareEntityValueCollection(tbref, propvalBefore as IEnumerable, propvalAfter as IEnumerable);
                        break;
                    case TableRefType.ManyToMany:
                        var middleValuesBefore = GetManyToManyObjects(fsql, table, tbref, entityBefore, prop);
                        var middleValuesAfter = GetManyToManyObjects(fsql, table, tbref, entityAfter, prop);
                        LocalCompareEntityValueCollection(tbref, middleValuesBefore as IEnumerable, middleValuesAfter as IEnumerable);
                        break;
                    case TableRefType.PgArrayToMany:
                    case TableRefType.ManyToOne: //不属于聚合根
                        break;
                }
            }
        }
        void LocalCompareEntityValueCollection(TableRef tbref, IEnumerable collectionBefore, IEnumerable collectionAfter)
        {
            var elementType = tbref.RefType == TableRefType.ManyToMany ? tbref.RefMiddleEntityType : tbref.RefEntityType;
            if (collectionBefore == null && collectionAfter == null) return;
            if (collectionBefore == null && collectionAfter != null)
            {
                foreach (var item in collectionAfter)
                    insertLog.Add(NativeTuple.Create(elementType, item));
                return;
            }
            if (collectionBefore != null && collectionAfter == null)
            {
                foreach (var item in collectionBefore as IEnumerable)
                {
                    deleteLog.Add(NativeTuple.Create(elementType, new[] { item }));
                    NavigateReader(fsql, elementType, item, (path, tr, ct, stackvs) =>
                    {
                        var dellist = stackvs.First() as object[] ?? new [] { stackvs.First() };
                        deleteLog.Add(NativeTuple.Create(ct, dellist));
                    });
                }
                return;
            }
            Dictionary<string, object> dictBefore = new Dictionary<string, object>();
            Dictionary<string, object> dictAfter = new Dictionary<string, object>();
            foreach (var item in collectionBefore as IEnumerable)
            {
                var beforeKey = fsql.GetEntityKeyString(elementType, item, false);
                dictBefore.Add(beforeKey, item);
            }
            foreach (var item in collectionAfter as IEnumerable)
            {
                var afterKey = fsql.GetEntityKeyString(elementType, item, false);
                if (afterKey != null) insertLog.Add(NativeTuple.Create(elementType, item));
                else dictBefore.Add(afterKey, item);
            }
            foreach (var key in dictBefore.Keys.ToArray())
            {
                if (dictAfter.ContainsKey(key) == false)
                {
                    var value = dictBefore[key];
                    deleteLog.Add(NativeTuple.Create(elementType, new[] { value }));
                    NavigateReader(fsql, elementType, value, (path, tr, ct, stackvs) =>
                    {
                        var dellist = stackvs.First() as object[] ?? new[] { stackvs.First() };
                        deleteLog.Add(NativeTuple.Create(ct, dellist));
                    });
                    dictBefore.Remove(key);
                }
            }
            foreach (var key in dictAfter.Keys.ToArray())
            {
                if (dictBefore.ContainsKey(key) == false)
                {
                    insertLog.Add(NativeTuple.Create(elementType, dictAfter[key]));
                    dictAfter.Remove(key);
                }
            }
            foreach (var key in dictBefore.Keys)
                LocalCompareEntityValue(elementType, dictBefore[key], dictAfter[key], null);
        }
    }

    public static void NavigateReader(IFreeSql fsql, Type rootType, object rootEntity, Action<string, TableRef, Type, List<object>> callback)
    {
        Dictionary<Type, Dictionary<string, bool>> ignores = new Dictionary<Type, Dictionary<string, bool>>();
        var statckPath = new Stack<string>();
        var stackValues = new List<object>();
        statckPath.Push("_");
        stackValues.Add(rootEntity);
        LocalNavigateReader(rootType, rootEntity);
        ignores.Clear();

        void LocalNavigateReader(Type entityType, object entity)
        {
            if (entity == null) return;
            if (entityType == null) entityType = entity.GetType();
            var table = fsql.CodeFirst.GetTableByEntity(entityType);
            if (table == null) return;

            var stateKey = fsql.GetEntityKeyString(entityType, entity, false);
            if (ignores.TryGetValue(entityType, out var stateKeys) == false) ignores.Add(entityType, stateKeys = new Dictionary<string, bool>());
            if (stateKeys.ContainsKey(stateKey)) return;
            stateKeys.Add(stateKey, true);

            foreach (var prop in table.Properties.Values)
            {
                var tbref = table.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
                switch (tbref.RefType)
                {
                    case TableRefType.OneToOne:
                        var propval = table.GetPropertyValue(entity, prop.Name);
                        statckPath.Push(prop.Name);
                        stackValues.Add(propval);
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entity, propval);
                        callback?.Invoke(string.Join(".", statckPath), tbref, tbref.RefEntityType, stackValues);
                        LocalNavigateReader(tbref.RefEntityType, propval);
                        stackValues.RemoveAt(stackValues.Count - 1);
                        statckPath.Pop();
                        break;
                    case TableRefType.OneToMany:
                        var propvalOtm = table.GetPropertyValue(entity, prop.Name);
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entity, propvalOtm);
                        var propvalOtmList = new List<object>();
                        foreach (var val in propvalOtm as IEnumerable)
                            propvalOtmList.Add(val);
                        statckPath.Push($"{prop.Name}[]");
                        stackValues.Add(propvalOtmList.ToArray());
                        callback?.Invoke(string.Join(".", statckPath), tbref, tbref.RefEntityType, stackValues);
                        foreach (var val in propvalOtm as IEnumerable)
                            LocalNavigateReader(tbref.RefEntityType, val);
                        stackValues.RemoveAt(stackValues.Count - 1);
                        statckPath.Pop();
                        break;
                    case TableRefType.ManyToMany:
                        var middleValues = GetManyToManyObjects(fsql, table, tbref, entity, prop).ToArray();
                        statckPath.Push($"{prop.Name}[]");
                        stackValues.Add(middleValues);
                        callback?.Invoke(string.Join(".", statckPath), tbref, tbref.RefEntityType, stackValues);
                        stackValues.RemoveAt(stackValues.Count - 1);
                        statckPath.Pop();
                        break;
                    case TableRefType.PgArrayToMany:
                    case TableRefType.ManyToOne: //不属于聚合根
                        break;
                }
            }
        }
    }

    public static void MapEntityValue(IFreeSql fsql, Type rootEntityType, object rootEntityFrom, object rootEntityTo)
    {
        Dictionary<Type, Dictionary<string, bool>> ignores = new Dictionary<Type, Dictionary<string, bool>>();
        LocalMapEntityValue(rootEntityType, rootEntityFrom, rootEntityTo);
        ignores.Clear();

        void LocalMapEntityValue(Type entityType, object entityFrom, object entityTo)
        {
            if (entityFrom == null || entityTo == null) return;
            if (entityType == null) entityType = entityFrom?.GetType() ?? entityTo?.GetType();
            var table = fsql.CodeFirst.GetTableByEntity(entityType);
            if (table == null) return;

            var stateKey = fsql.GetEntityKeyString(entityType, entityFrom, false);
            if (ignores.TryGetValue(entityType, out var stateKeys) == false) ignores.Add(entityType, stateKeys = new Dictionary<string, bool>());
            if (stateKeys.ContainsKey(stateKey)) return;
            stateKeys.Add(stateKey, true);

            foreach (var prop in table.Properties.Values)
            {
                if (table.ColumnsByCsIgnore.ContainsKey(prop.Name)) continue;
                if (table.ColumnsByCs.ContainsKey(prop.Name))
                {
                    table.SetPropertyValue(entityTo, prop.Name, table.GetPropertyValue(entityFrom, prop.Name));
                    continue;
                }
                var tbref = table.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
                var propvalFrom = EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, entityType, entityFrom, prop.Name);
                if (propvalFrom == null)
                {
                    EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, entityType, entityTo, prop.Name, null);
                    return;
                }
                switch (tbref.RefType)
                {
                    case TableRefType.OneToOne:
                        var propvalTo = tbref.RefEntityType.CreateInstanceGetDefaultValue();
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entityFrom, propvalFrom);
                        LocalMapEntityValue(tbref.RefEntityType, propvalFrom, propvalTo);
                        EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, entityType, entityTo, prop.Name, propvalTo);
                        break;
                    case TableRefType.OneToMany:
                        SetNavigateRelationshipValue(fsql, tbref, tbref.RefEntityType, entityFrom, propvalFrom);
                        LocalMapEntityValueCollection(entityType, entityFrom, entityTo, tbref, propvalFrom as IEnumerable, prop, true);
                        break;
                    case TableRefType.ManyToMany:
                        LocalMapEntityValueCollection(entityType, entityFrom, entityTo, tbref, propvalFrom as IEnumerable, prop, false);
                        break;
                    case TableRefType.PgArrayToMany:
                    case TableRefType.ManyToOne: //不属于聚合根
                        break;
                }
            }
        }
        void LocalMapEntityValueCollection(Type entityType, object entityFrom, object entityTo, TableRef tbref, IEnumerable propvalFrom, PropertyInfo prop, bool cascade)
        {
            var propvalTo = typeof(List<>).MakeGenericType(tbref.RefEntityType).CreateInstanceGetDefaultValue();
            var propvalToIList = propvalTo as IList;
            foreach (var fromItem in propvalFrom)
            {
                var toItem = tbref.RefEntityType.CreateInstanceGetDefaultValue();
                if (cascade) LocalMapEntityValue(tbref.RefEntityType, fromItem, toItem);
                else EntityUtilExtensions.MapEntityValue(fsql, tbref.RefEntityType, fromItem, toItem);
                propvalToIList.Add(toItem);
            }
            var propvalType = prop.PropertyType.GetGenericTypeDefinition();
            if (propvalType == typeof(List<>) || propvalType == typeof(ICollection<>))
                EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, entityType, entityTo, prop.Name, propvalTo);
            else if (propvalType == typeof(ObservableCollection<>))
            {
                //var propvalTypeOcCtor = typeof(ObservableCollection<>).MakeGenericType(tbref.RefEntityType).GetConstructor(new[] { typeof(List<>).MakeGenericType(tbref.RefEntityType) });
                var propvalTypeOc = Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(tbref.RefEntityType), new object[] { propvalTo });
                EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, entityType, entityTo, prop.Name, propvalTypeOc);
            }
        }
    }

    public static List<object> GetManyToManyObjects(IFreeSql fsql, TableInfo table, TableRef tbref, object entity, PropertyInfo prop)
    {
        if (tbref.RefType != TableRefType.ManyToMany) return null;
        var rights = table.GetPropertyValue(entity, prop.Name) as IEnumerable;
        if (rights == null) return null;
        var middles = new List<object>();
        var leftpkvals = new object[tbref.Columns.Count];
        for (var x = 0; x < tbref.Columns.Count; x++)
            leftpkvals[x] = Utils.GetDataReaderValue(tbref.MiddleColumns[x].CsType, EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, table.Type, entity, tbref.Columns[x].CsName));
        foreach (var right in rights)
        {
            var midval = tbref.RefMiddleEntityType.CreateInstanceGetDefaultValue();
            for (var x = 0; x < tbref.Columns.Count; x++)
                EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, tbref.RefMiddleEntityType, midval, tbref.MiddleColumns[x].CsName, leftpkvals[x]);

            for (var x = tbref.Columns.Count; x < tbref.MiddleColumns.Count; x++)
            {
                var refcol = tbref.RefColumns[x - tbref.Columns.Count];
                var refval = EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, tbref.RefEntityType, right, refcol.CsName);
                if (refval == refcol.CsType.CreateInstanceGetDefaultValue()) throw new Exception($"ManyToMany 关联对象的主键属性({tbref.RefEntityType.DisplayCsharp()}.{refcol.CsName})不能为空");
                refval = Utils.GetDataReaderValue(tbref.MiddleColumns[x].CsType, refval);
                EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, tbref.RefMiddleEntityType, midval, tbref.MiddleColumns[x].CsName, refval);
            }
            middles.Add(midval);
        }
        return middles;
    }
    public static void SetNavigateRelationshipValue(IFreeSql orm, TableRef tbref, Type leftType, object leftItem, object rightItem)
    {
        if (rightItem == null) return;
        switch (tbref.RefType)
        {
            case TableRefType.OneToOne:
                for (var idx = 0; idx < tbref.Columns.Count; idx++)
                {
                    var colval = Utils.GetDataReaderValue(tbref.RefColumns[idx].CsType, EntityUtilExtensions.GetEntityValueWithPropertyName(orm, leftType, leftItem, tbref.Columns[idx].CsName));
                    EntityUtilExtensions.SetEntityValueWithPropertyName(orm, tbref.RefEntityType, rightItem, tbref.RefColumns[idx].CsName, colval);
                }
                break;
            case TableRefType.OneToMany:
                var rightEachOtm = rightItem as IEnumerable;
                if (rightEachOtm == null) break;
                var leftColValsOtm = new object[tbref.Columns.Count];
                for (var idx = 0; idx < tbref.Columns.Count; idx++)
                    leftColValsOtm[idx] = Utils.GetDataReaderValue(tbref.RefColumns[idx].CsType, EntityUtilExtensions.GetEntityValueWithPropertyName(orm, leftType, leftItem, tbref.Columns[idx].CsName));
                foreach (var rightEle in rightEachOtm)
                    for (var idx = 0; idx < tbref.Columns.Count; idx++)
                        EntityUtilExtensions.SetEntityValueWithPropertyName(orm, tbref.RefEntityType, rightEle, tbref.RefColumns[idx].CsName, leftColValsOtm[idx]);
                break;
            case TableRefType.ManyToOne:
                for (var idx = 0; idx < tbref.RefColumns.Count; idx++)
                {
                    var colval = Utils.GetDataReaderValue(tbref.Columns[idx].CsType, EntityUtilExtensions.GetEntityValueWithPropertyName(orm, tbref.RefEntityType, rightItem, tbref.RefColumns[idx].CsName));
                    EntityUtilExtensions.SetEntityValueWithPropertyName(orm, leftType, leftItem, tbref.Columns[idx].CsName, colval);
                }
                break;
        }
    }
}
