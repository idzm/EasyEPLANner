﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechObject
{
    /// <summary>
    /// Класс реализующий базовую операцию для технологического объекта
    /// </summary>
    public class BaseOperation : Editor.TreeViewItem
    {
        public BaseOperation(Mode owner)
        {
            Name = "";
            LuaName = "";
            Properties = new List<BaseParameter>();
            Steps = new List<BaseParameter>();
            this.owner = owner;
        }

        /// <summary>
        /// Возвращает пустой объект - базовая операция.
        /// </summary>
        /// <returns></returns>
        public static BaseOperation EmptyOperation()
        {
            return new BaseOperation("", "", new List<BaseParameter>(), 
                new List<BaseParameter>());
        }

        /// <summary>
        /// Конструктор для инициализации базовой операции и параметров
        /// </summary>
        /// <param name="name">Имя операции</param>
        /// <param name="luaName">Lua имя операции</param>
        /// <param name="baseOperationProperties">Свойства операции</param>
        /// <param name="baseSteps">Базовые шаги операции</param>
        public BaseOperation(string name, string luaName, 
            List<BaseParameter> baseOperationProperties, 
            List<BaseParameter> baseSteps)
        {
            Name = name;
            LuaName = luaName;
            Properties = baseOperationProperties;
            Steps = baseSteps;
        }

        /// <summary>
        /// Добавить базовый шаг
        /// </summary>
        /// <param name="luaName">Lua-имя</param>
        /// <param name="name">Имя</param>
        public void AddStep(string luaName, string name)
        {
            if (Steps.Count == 0)
            {
                // Пустой объект, если не должно быть выбрано никаких объектов
                Steps.Add(new ActiveParameter("", ""));
            }

            Steps.Add(new ActiveParameter(luaName, name));
        }

        /// <summary>
        /// Добавить активный параметр
        /// </summary>
        /// <param name="luaName">Lua-имя</param>
        /// <param name="name">Имя</param>
        /// <param name="defaultValue">Значение по-умолчанию</param>
        public void AddActiveParameter(string luaName, string name, 
            string defaultValue)
        {
            Properties.Add(new ActiveParameter(luaName, name, defaultValue));
        }
        
        /// <summary>
        /// Добавить активный булевый параметр
        /// </summary>
        /// <param name="luaName">Lua-имя</param>
        /// <param name="name">Имя</param>
        /// <param name="defaultValue">Значение по-умолчанию</param>
        public void AddActiveBoolParameter(string luaName, string name,
            string defaultValue)
        {
            Properties.Add(new ActiveBoolParameter(luaName, name, 
                defaultValue));
        }

        /// <summary>
        /// Получить имя операции
        /// </summary>
        public string Name
        {
            get
            {
                return operationName;
            }

            set
            {
                operationName = value;
            }
        }

        /// <summary>
        /// Получить Lua имя операции
        /// </summary>
        public string LuaName
        {
            get
            {
                return luaOperationName;
            }

            set
            {
                luaOperationName = value;
            }
        }

        /// <summary>
        /// Шаги операции.
        /// </summary>
        public List<BaseParameter> Steps
        {
            get
            {
                return baseSteps;
            }
            set
            {
                baseSteps = value;
            }
        }

        /// <summary>
        /// Инициализация базовой операции по имени
        /// </summary>
        /// <param name="baseOperName">Имя операции</param>
        public void Init(string baseOperName)
        {
            TechObject techObject = owner.Owner.Owner;
            string baseTechObjectName = techObject.BaseTechObject.Name;

            ResetOperationSteps();

            if (baseTechObjectName != "")
            {
                BaseOperation operation;
                operation = techObject.BaseTechObject
                    .GetBaseOperationByName(baseOperName);
                if (operation == null)
                {
                    operation = techObject.BaseTechObject
                        .GetBaseOperationByLuaName(baseOperName);
                }

                if (operation != null)
                {
                    Name = operation.Name;
                    LuaName = operation.LuaName;
                    Properties = operation.Properties
                        .Select(x => x.Clone())
                        .ToList();
                    baseSteps = operation.Steps;
                }
            }
            else
            {
                Name = "";
                LuaName = "";
                baseOperationProperties = new List<BaseParameter>();
                baseSteps = new List<BaseParameter>();
            }

            techObject.AttachedObjects.Check();
            SetItems();
        }

        /// <summary>
        /// Сбросить базовые шаги базовой операции
        /// </summary>
        private void ResetOperationSteps()
        {
            foreach (var step in owner.MainSteps)
            {
                step.SetNewValue("", true);
            }
        }

        /// <summary>
        /// Добавление полей в массив для отображения на дереве
        /// </summary>
        private void SetItems()
        {
            var showedParameters = new List<BaseParameter>();
            foreach (var parameter in Properties)
            {
                showedParameters.Add(parameter);
            }
            items = showedParameters.ToArray();
        }

        /// <summary>
        /// Сохранение в виде таблицы Lua
        /// </summary>
        /// <param name="prefix">Префикс (отступ)</param>
        /// <returns></returns>
        public string SaveAsLuaTable(string prefix)
        {
            var res = "";
            
            if (Properties == null)
            {
                return res;
            }

            var propertiesCountForSave = Properties.Count();
            if (propertiesCountForSave <= 0)
            {
                return res;
            }

            res += prefix + "props =\n" + prefix + "\t{\n";
            foreach (var operParam in Properties)
            {
                res += "\t" + prefix + operParam.LuaName + " = \'" +
                    operParam.Value + "\',\n";
            }
            res += prefix + "\t},\n";
            return res;
        }

        /// <summary>
        /// Установка свойств базовой операции
        /// </summary>
        /// <param name="extraParams">Свойства операции</param>
        public void SetExtraProperties(Editor.ObjectProperty[] extraParams)
        {
            foreach (Editor.ObjectProperty extraParam in extraParams)
            {
                var property = Properties
                    .Where(x => x.LuaName
                    .Equals(extraParam.DisplayText[0]))
                    .FirstOrDefault();

                if (property != null)
                {
                    property.SetValue(extraParam.DisplayText[1]);
                }
            }
        }

        /// <summary>
        /// Получить свойства базовой операции
        /// </summary>
        public List<BaseParameter> Properties
        {
            get
            {
                return baseOperationProperties;
            }
            set
            {
                baseOperationProperties = value;
            }
        }
       
        /// <summary>
        /// Добавить свойства базовой операции.
        /// </summary>
        /// <param name="properties">Массив свойств</param>
        public void AddProperties(List<BaseParameter> properties)
        {
            // Пока не решен вопрос с параметрами, отключаем для операции
            // мойка добавление доп. свойств узлов.
            if (this.LuaName == "WASHING_CIP")
            {
                return;
            }

            foreach(var property in properties)
            {
                var equalPropertiesCount = Properties
                    .Where(x => x.LuaName == property.LuaName).Count();
                if (equalPropertiesCount == 0)
                {
                    Properties.Add(property.Clone());
                }
            }

            SetItems();
        }

        /// <summary>
        /// Удалить свойства базовой операции.
        /// </summary>
        /// <param name="properties">Массив свойств</param>
        public void RemoveProperties(List<BaseParameter> properties)
        {
            foreach (var property in properties)
            {
                var deletingProperty = Properties
                    .Where(x => x.LuaName == property.LuaName)
                    .FirstOrDefault();
                if (deletingProperty != null)
                {
                    Properties.Remove(deletingProperty);
                }
            }
            SetItems();
        }

        /// <summary>
        /// Копирование объекта
        /// </summary>
        /// <param name="owner">Новая операция-владелец объекта</param>
        /// <returns></returns>
        public BaseOperation Clone(Mode owner)
        {
            var operation = Clone();
            operation.owner = this.owner;
            return operation;
        }

        /// <summary>
        /// Копирование объекта
        /// </summary>
        /// <returns></returns>
        public BaseOperation Clone()
        {
            var properties = new List<BaseParameter>(baseOperationProperties
                .Count);
            for (int i = 0; i < baseOperationProperties.Count; i++)
            {
                properties.Add(baseOperationProperties[i].Clone());
            }

            var steps = new List<BaseParameter>();
            for (int i = 0; i < Steps.Count; i++)
            {
                steps.Add(Steps[i].Clone());
            }

            var operation = new BaseOperation(operationName, luaOperationName,
                properties, steps);
            operation.SetItems();

            return operation;
        }

        #region Реализация ITreeViewItem
        override public string[] DisplayText
        {
            get
            {
                if (items.Count() > 0)
                {
                    string res = string.Format("Доп. свойства ({0})", 
                        items.Count());
                    return new string[] { res, "" };
                }
                else
                {
                    string res = string.Format("Доп. свойства");
                    return new string[] { res, "" };
                }
            }
        }

        override public Editor.ITreeViewItem[] Items
        {
            get
            {
                return items;
            }
        }
        #endregion

        private Editor.ITreeViewItem[] items = new Editor.ITreeViewItem[0];
        
        private List<BaseParameter> baseOperationProperties;
        private string operationName;
        private string luaOperationName;
        private List<BaseParameter> baseSteps;

        private Mode owner;
    }
}
