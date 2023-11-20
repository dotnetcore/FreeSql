// by: Daily

#if net40 || NETSTANDARD2_0
#else

using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Extensions.DynamicEntity;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

public static class FreeSqlGlobalDynamicEntityExtensions
{
    /// <summary>
    /// 动态构建Class Type
    /// </summary>
    /// <returns></returns>
    public static DynamicCompileBuilder DynamicEntity(this ICodeFirst codeFirst, string className,
        params Attribute[] attributes)
    {
        return new DynamicCompileBuilder((codeFirst as CodeFirstProvider)._orm, className, attributes);
    }

    /// <summary>
    /// 根据字典，创建 table 对应的实体对象
    /// </summary>
    /// <param name="table"></param>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static object CreateInstance(this TableInfo table, Dictionary<string, object> dict)
    {
        if (table == null || dict == null) return null;
        var instance = table.Type.CreateInstanceGetDefaultValue();
        //加载默认值
        var defaultValueInit = table.Type.GetMethod("DefaultValueInit");
        if (defaultValueInit != null)
        {
            defaultValueInit.Invoke(instance, new object[0]);
        }

        foreach (var key in table.ColumnsByCs.Keys)
        {
            if (dict.ContainsKey(key) == false) continue;
            table.ColumnsByCs[key].SetValue(instance, dict[key]);
        }

        return instance;
    }

    /// <summary>
    /// 根据实体对象，创建 table 对应的字典
    /// </summary>
    /// <param name="table"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static Dictionary<string, object> CreateDictionary(this TableInfo table, object instance)
    {
        if (table == null || instance == null) return null;
        var dict = new Dictionary<string, object>();
        foreach (var key in table.ColumnsByCs.Keys)
            dict[key] = table.ColumnsByCs[key].GetValue(instance);
        return dict;
    }
}

namespace FreeSql.Extensions.DynamicEntity
{
    /// <summary>
    /// 动态创建实体类型
    /// </summary>
    public class DynamicCompileBuilder
    {
        private string _className = string.Empty;
        private Attribute[] _tableAttributes = null;
        private List<DynamicPropertyInfo> _properties = new List<DynamicPropertyInfo>();
        private Type _superClass = null;
        private IFreeSql _fsql = null;

        /// <summary>
        /// 配置Class
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="attributes">类标记的特性[Table(Name = "xxx")] [Index(xxxx)]</param>
        /// <returns></returns>
        public DynamicCompileBuilder(IFreeSql fsql, string className, params Attribute[] attributes)
        {
            _fsql = fsql;
            _className = className;
            _tableAttributes = attributes;
        }

        /// <summary>
        /// 配置属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="attributes">属性标记的特性-支持多个</param>
        /// <returns></returns>
        public DynamicCompileBuilder Property(string propertyName, Type propertyType,
            params Attribute[] attributes)
        {
            _properties.Add(new DynamicPropertyInfo()
            {
                PropertyName = propertyName,
                PropertyType = propertyType,
                DefaultValue = null,
                Attributes = attributes
            });
            return this;
        }

        /// <summary>
        /// 配置属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="isOverride">该属性是否重写父类属性</param>
        /// <param name="attributes">属性标记的特性-支持多个</param>
        /// <returns></returns>
        public DynamicCompileBuilder Property(string propertyName, Type propertyType, bool isOverride,
            params Attribute[] attributes)
        {
            _properties.Add(new DynamicPropertyInfo()
            {
                PropertyName = propertyName,
                PropertyType = propertyType,
                DefaultValue = null,
                IsOverride = isOverride,
                Attributes = attributes
            });
            return this;
        }

        /// <summary>
        /// 配置属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="isOverride">该属性是否重写父类属性</param>
        /// <param name="defaultValue">属性默认值</param>
        /// <param name="attributes">属性标记的特性-支持多个</param>
        /// <returns></returns>
        public DynamicCompileBuilder Property(string propertyName, Type propertyType, bool isOverride,
            object defaultValue, params Attribute[] attributes)
        {
            _properties.Add(new DynamicPropertyInfo()
            {
                PropertyName = propertyName,
                PropertyType = propertyType,
                DefaultValue = defaultValue,
                IsOverride = isOverride,
                Attributes = attributes
            });
            return this;
        }

        /// <summary>
        /// 配置父类
        /// </summary>
        /// <param name="superClass">父类类型</param>
        /// <returns></returns>
        public DynamicCompileBuilder Extend(Type superClass)
        {
            _superClass = superClass;
            return this;
        }

        private void SetTableAttribute(ref TypeBuilder typeBuilder)
        {
            if (_tableAttributes == null) return;

            foreach (var tableAttribute in _tableAttributes)
            {
                var propertyValues = new ArrayList();

                if (tableAttribute == null) continue;

                var classCtorInfo = tableAttribute.GetType().GetConstructor(Type.EmptyTypes);
                var classCtorInfos = tableAttribute.GetType().GetConstructors();
                var propertyInfos = tableAttribute.GetType().GetProperties().Where(p => p.CanWrite == true).ToArray();
                foreach (var propertyInfo in propertyInfos)
                    propertyValues.Add(propertyInfo.GetValue(tableAttribute));
                //是否存在有参构造函数
                var existConstructorArguments = classCtorInfos.Any(c => c.GetParameters().Length > 0);
                if (existConstructorArguments)
                {
                    var defaultParamsCtor = classCtorInfos.Where(c => c.GetParameters().Length > 0)
                        .OrderBy(c => c.GetParameters().Length).First();
                    //获取参数默认值
                    var defaultParams = new List<object>();
                    foreach (var parameterInfo in defaultParamsCtor.GetParameters())
                    {
                        defaultParams.Add(parameterInfo.ParameterType.CreateInstanceGetDefaultValue());
                    }

                    //思路：先通过构造函数的默认值实例化对象，然后通过属性的方式赋值
                    var customAttributeBuilder = new CustomAttributeBuilder(defaultParamsCtor, defaultParams.ToArray(),
                        propertyInfos,
                        propertyValues.ToArray());
                    typeBuilder.SetCustomAttribute(customAttributeBuilder);
                }
                else
                {
                    //不存在构造函数赋值直接属性赋值
                    var customAttributeBuilder = new CustomAttributeBuilder(classCtorInfo, new object[0], propertyInfos,
                        propertyValues.ToArray());
                    typeBuilder.SetCustomAttribute(customAttributeBuilder);
                }
            }
        }

        private void SetPropertys(ref TypeBuilder typeBuilder)
        {
            var defaultValues = new Dictionary<FieldBuilder, object>();
            foreach (var pinfo in _properties)
            {
                if (pinfo == null)
                    continue;
                var propertyName = pinfo.PropertyName;
                var propertyType = pinfo.PropertyType;
                //设置字段
                var field = typeBuilder.DefineField($"_{FirstCharToLower(propertyName)}", propertyType,
                    FieldAttributes.Private | FieldAttributes.HasDefault);
                var firstCharToUpper = FirstCharToUpper(propertyName);

                MethodAttributes maAttributes = MethodAttributes.Public;

                //是否重写
                if (pinfo.IsOverride)
                {
                    maAttributes = MethodAttributes.Public | MethodAttributes.Virtual;
                }

                //设置属性方法
                var methodGet = typeBuilder.DefineMethod($"get_{firstCharToUpper}", maAttributes, propertyType, null);
                var methodSet = typeBuilder.DefineMethod($"set_{firstCharToUpper}", maAttributes, null,
                    new Type[] { propertyType });

                var ilOfGet = methodGet.GetILGenerator();
                ilOfGet.Emit(OpCodes.Ldarg_0);
                ilOfGet.Emit(OpCodes.Ldfld, field);
                ilOfGet.Emit(OpCodes.Ret);

                var ilOfSet = methodSet.GetILGenerator();
                ilOfSet.Emit(OpCodes.Ldarg_0);
                ilOfSet.Emit(OpCodes.Ldarg_1);
                ilOfSet.Emit(OpCodes.Stfld, field);
                ilOfSet.Emit(OpCodes.Ret);


                //是否重写
                if (pinfo.IsOverride)
                {
                    //重写Get、Set方法
                    OverrideProperty(ref typeBuilder, methodGet, PropertyMethodEnum.GET, pinfo.PropertyName);
                    OverrideProperty(ref typeBuilder, methodSet, PropertyMethodEnum.SET, pinfo.PropertyName);
                }

                //设置属性
                var propertyBuilder =
                    typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);
                propertyBuilder.SetGetMethod(methodGet);
                propertyBuilder.SetSetMethod(methodSet);

                foreach (var pinfoAttribute in pinfo.Attributes)
                {
                    //设置特性
                    SetPropertyAttribute(ref propertyBuilder, pinfoAttribute);
                }

                if (pinfo.DefaultValue != null)
                {
                    defaultValues.Add(field, pinfo.DefaultValue);
                }
            }

            //动态构建方法，设置默认值
            var methodDefaultValue = typeBuilder.DefineMethod($"DefaultValueInit", MethodAttributes.Public, null, null);
            var methodDefaultValueLlGenerator = methodDefaultValue.GetILGenerator();
            foreach (var kv in defaultValues)
            {
                methodDefaultValueLlGenerator.Emit(OpCodes.Ldarg_0);
                OpCodesAdapter(ref methodDefaultValueLlGenerator, kv.Key, kv.Value);
                methodDefaultValueLlGenerator.Emit(OpCodes.Stfld, kv.Key);
            }

            methodDefaultValueLlGenerator.Emit(OpCodes.Ret);
        }

        //IL命令类型适配
        private void OpCodesAdapter(ref ILGenerator generator, FieldInfo info, object value)
        {
            var fieldTypeName = info.FieldType.Name;
            switch (fieldTypeName)
            {
                case "Int32":
                    generator.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
                    break;
                case "Boolean":
                    generator.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
                    break;
                case "Char":
                    generator.Emit(OpCodes.Ldc_I4, Convert.ToChar(value));
                    break;
                case "String":
                    generator.Emit(OpCodes.Ldstr, Convert.ToString(value));
                    break;
                case "DateTime":
                    generator.Emit(OpCodes.Ldstr, Convert.ToString(value));
                    generator.Emit(OpCodes.Call, typeof(DateTime).GetMethod("Parse", new[] { typeof(string) }));
                    break;
                case "Int64":
                    generator.Emit(OpCodes.Ldc_I4, Convert.ToString(value));
                    generator.Emit(OpCodes.Conv_I8);
                    break;
                case "Double":
                    generator.Emit(OpCodes.Ldc_R8, Convert.ToDouble(value));
                    break;
                case "Single":
                    generator.Emit(OpCodes.Ldc_R4, Convert.ToSingle(value));
                    break;
                case "Decimal":
                    Console.WriteLine(Convert.ToString(value));
                    generator.Emit(OpCodes.Ldstr, Convert.ToString(value));
                    generator.Emit(OpCodes.Call, typeof(Decimal).GetMethod("Parse", new[] { typeof(string) }));
                    break;
            }
        }

        private void SetPropertyAttribute<T>(ref PropertyBuilder propertyBuilder, T tAttribute)
        {
            if (tAttribute == null) return;
            var propertyInfos = tAttribute.GetType().GetProperties().Where(p => p.CanWrite == true).ToArray();
            var constructor = tAttribute.GetType().GetConstructor(Type.EmptyTypes);
            var propertyValues = new ArrayList();
            foreach (var propertyInfo in propertyInfos)
                propertyValues.Add(propertyInfo.GetValue(tAttribute));

            //可能存在有参构造
            //if (constructor == null)
            //{
            //    var constructorTypes = propertyInfos.Select(p => p.PropertyType).ToList();
            //    constructor = tAttribute.GetType().GetConstructor(constructorTypes.ToArray());
            //    var customAttributeBuilder = new CustomAttributeBuilder(constructor, constructorTypes.ToArray(),
            //        propertyInfos, propertyValues.ToArray());
            //    propertyBuilder.SetCustomAttribute(customAttributeBuilder);
            //}
            //else
            //{
            var customAttributeBuilder = new CustomAttributeBuilder(constructor, new object[0], propertyInfos,
                propertyValues.ToArray());
            propertyBuilder.SetCustomAttribute(customAttributeBuilder);
            // }
        }

        /// <summary>
        /// Override属性
        /// </summary>
        /// <param name="typeBuilder"></param>
        private void OverrideProperty(ref TypeBuilder typeBuilder, MethodBuilder methodBuilder,
            PropertyMethodEnum methodEnum,
            string propertyName)
        {
            //查找父类的属性信息
            var propertyInfo = typeBuilder.BaseType.GetProperty(propertyName);
            if (propertyInfo == null) return;
            var pm = methodEnum == PropertyMethodEnum.GET ? propertyInfo.GetGetMethod() : propertyInfo.GetSetMethod();
            //重写父类GET SET 方法
            typeBuilder.DefineMethodOverride(methodBuilder, pm);
        }


        /// <summary>
        /// Emit动态创建出Class - Type
        /// </summary>
        /// <returns></returns>
        public TableInfo Build()
        {
            //初始化AssemblyName的一个实例
            var assemblyName = new AssemblyName("FreeSql.DynamicCompileBuilder");
            //设置程序集的名称
            var defineDynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            //动态在程序集内创建一个模块
            var defineDynamicModule =
                defineDynamicAssembly.DefineDynamicModule("FreeSql.DynamicCompileBuilder.Dynamics");
            //动态的在模块内创建一个类
            var typeBuilder =
                defineDynamicModule.DefineType(_className, TypeAttributes.Public | TypeAttributes.Class, _superClass);

            //设置TableAttribute
            SetTableAttribute(ref typeBuilder);

            //设置属性
            SetPropertys(ref typeBuilder);

            //创建类的Type对象
            var type = typeBuilder.CreateTypeInfo().AsType();

            return _fsql.CodeFirst.GetTableByEntity(type);
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            string str = input.First().ToString().ToLower() + input.Substring(1);
            return str;
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            string str = input.First().ToString().ToUpper() + input.Substring(1);
            return str;
        }

        private static string Md5Encryption(string inputStr)
        {
            var result = string.Empty;
            //32位大写
            using (var md5 = MD5.Create())
            {
                var resultBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(inputStr));
                result = BitConverter.ToString(resultBytes);
            }

            return result;
        }

        class DynamicPropertyInfo
        {
            public string PropertyName { get; set; } = string.Empty;
            public Type PropertyType { get; set; }
            public object DefaultValue { get; set; }
            public bool IsOverride { get; set; } = false;
            public Attribute[] Attributes { get; set; }
        }

        enum PropertyMethodEnum
        {
            GET,
            SET
        }
    }
}
#endif